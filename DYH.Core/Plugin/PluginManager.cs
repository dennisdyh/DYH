using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Compilation;
using System.Web.Hosting;
using DYH.Core.Basic;
using DYH.Core.Plugin;
using DYH.Core.Utils;

//便于插件加载，该特性能够使一些应用在网站初始化阶段完成
[assembly: PreApplicationStartMethod(typeof(PluginManager), "Initialize")]
namespace DYH.Core.Plugin
{
    /// <summary>
    /// 为插件提供各种管理
    /// </summary>
    public class PluginManager
    {
        /// <summary>
        /// 文件路径:用来持久化已经安装的插件信息
        /// </summary>
        private const string InstalledPluginsFilePath = "~/App_Data/InstalledPlugins.txt";
        /// <summary>
        /// 插件目录
        /// </summary>
        private const string PluginsPath = "~/Plugins";
        /// <summary>
        /// 卷影拷贝目录，插件的bin目录
        /// </summary>
        private const string ShadowCopyPath = "~/Plugins/bin";
        /// <summary>
        /// 资源文件锁，多线程读取，单线程写入
        /// </summary>
        private static readonly ReaderWriterLockSlim Locker = new ReaderWriterLockSlim();
        private static DirectoryInfo _shadowCopyFolder;
        private static bool _clearShadowDirectoryOnStartup;

        /// <summary>
        /// 获取或设置一个已经加载到当前应用程序域中的插件的集合
        /// </summary>
        public static IEnumerable<PluginDescriptor> ReferencedPlugins { get; set; }
        ///// <summary>
        ///// 返回一个与当前应用不兼容的所有插件的集合
        ///// </summary>
        //public static IEnumerable<string> IncompatiblePlugins { get; set; }


        /// <summary>
        /// 初始化插件，插件初始化有两种模式，一种完全信任，一种不是中度信任
        /// Initialize
        /// </summary>
        public static void Initialize()
        {
            using (new WriteLockDisposable(Locker))
            {
                // TODO: Add verbose exception handling / raising here since this is happening on app startup and could

                // prevent app from starting altogether
                //插件目录
                var pluginFolder = new DirectoryInfo(HostingEnvironment.MapPath(PluginsPath));
                //卷影拷贝目录
                _shadowCopyFolder = new DirectoryInfo(HostingEnvironment.MapPath(ShadowCopyPath));

                //保存已经加载到当前应用程序域中的插件的集合
                var referencedPlugins = new List<PluginDescriptor>();

                //保存不兼容的插件信息
                //var incompatiblePlugins = new List<string>();

                //是否清除卷影拷贝中的文件也就是 ~/Plugins/bin的文件
                _clearShadowDirectoryOnStartup =
                    !String.IsNullOrEmpty(ConfigurationManager.AppSettings["ClearPluginsShadowDirectoryOnStartup"]) &&
                   Convert.ToBoolean(ConfigurationManager.AppSettings["ClearPluginsShadowDirectoryOnStartup"]);

                try
                {
                    //获取已经安装的插件的集合
                    var installedPluginSystemNames = PluginFileParser.ParseInstalledPluginsFile(GetInstalledPluginsFilePath());

                    Debug.WriteLine("Creating shadow copy folder and querying for dlls");
                    //ensure folders are created
                    //创建目录  "~/Plugins"
                    Directory.CreateDirectory(pluginFolder.FullName);

                    //创建目录~/Plugins/bin
                    Directory.CreateDirectory(_shadowCopyFolder.FullName);

                    //get list of all files in bin
                    //获取~/Plugins/bin目录和当前目录的子目录下的所有文件集合
                    var binFiles = _shadowCopyFolder.GetFiles("*", SearchOption.AllDirectories);

                    if (_clearShadowDirectoryOnStartup)
                    {
                        //clear out shadow copied plugins
                        //遍历清除卷影拷贝中的文件也就是~/Plugins/bin中的文件
                        foreach (var f in binFiles)
                        {
                            Debug.WriteLine("Deleting " + f.Name);
                            try
                            {
                                File.Delete(f.FullName);
                            }
                            catch (Exception exc)
                            {
                                Debug.WriteLine("Error deleting file " + f.Name + ". Exception: " + exc);
                            }
                        }
                    }

                    //load description files
                    //遍历所有的插件描述文件 该文件从 /Plugins 中获取
                    foreach (var dfd in GetDescriptionFilesAndDescriptors(pluginFolder))
                    {
                        var descriptionFile = dfd.Key;
                        var pluginDescriptor = dfd.Value;

                        ////ensure that version of plugin is valid
                        ////确认插件是否支持当前的nopCommerce
                        //if (!pluginDescriptor.SupportedVersions.Contains(NopVersion.CurrentVersion, StringComparer.InvariantCultureIgnoreCase))
                        //{
                        //    incompatiblePlugins.Add(pluginDescriptor.SystemName);
                        //    continue;
                        //}

                        //some validation
                        //是否有系统名称
                        if (String.IsNullOrWhiteSpace(pluginDescriptor.SystemName))
                            throw new Exception(string.Format("A plugin '{0}' has no system name. Try assigning the plugin a unique name and recompiling.", descriptionFile.FullName));
                        //是否具有相同系统名称的插件
                        if (referencedPlugins.Contains(pluginDescriptor))
                            throw new Exception(string.Format("A plugin with '{0}' system name is already defined", pluginDescriptor.SystemName));

                        //set 'Installed' property
                        //设置插件安装状态
                        pluginDescriptor.Installed = installedPluginSystemNames
                            .FirstOrDefault(x => x.Equals(pluginDescriptor.SystemName, StringComparison.InvariantCultureIgnoreCase)) != null;

                        try
                        {
                            if (descriptionFile.Directory == null)
                                throw new Exception(string.Format("Directory cannot be resolved for '{0}' description file", descriptionFile.Name));

                            //get list of all DLLs in plugins (not in bin!)
                            //获取Description.txt所在文件夹中以及在当前目录下的子目录中所有的DLL文件，并且该文件不存在/Plugins/bin文件夹中
                            //descriptionFile : Description.txt
                            //插件文件集合：Description.txt所在文件夹得插件文件集合
                            var pluginFiles = descriptionFile.Directory.GetFiles("*.dll", SearchOption.AllDirectories)
                                //just make sure we're not registering shadow copied plugins
                                //找出~/Plugins/bin/目录中与Description.txt所在文件目录中不匹配的文件
                                .Where(x => !binFiles.Select(q => q.FullName).Contains(x.FullName))
                                //确认当前的父目录是/Plugins/
                                .Where(x => IsPackagePluginFolder(x.Directory))
                                //得到所有插件集合
                                .ToList();

                            //other plugin description info
                            //仅仅取得插件文件，注意：插件也有可能引用别的DLL文件，所以我们需要排除掉被该插件引用的DLL文件
                            var mainPluginFile = pluginFiles
                                .FirstOrDefault(x => x.Name.Equals(pluginDescriptor.PluginFileName, StringComparison.InvariantCultureIgnoreCase));

                            //将插件自己本身信息赋值给插件描述器
                            pluginDescriptor.OriginalAssemblyFile = mainPluginFile;
                            //执行部署，将插件添加到当前应用程序中
                            var deploiedAssembly = PerformFileDeploy(mainPluginFile);

                            //shadow copy main plugin file
                            //执行部署后返回程序集自己
                            pluginDescriptor.ReferencedAssembly = deploiedAssembly;

                            //load all other referenced assemblies now
                            foreach (var plugin in pluginFiles
                                //从插件集合中找出与当前插件不相同的集合
                                .Where(x => !x.Name.Equals(mainPluginFile.Name, StringComparison.InvariantCultureIgnoreCase))
                                //找出不是当前应用程序的程序集集合
                                .Where(x => !IsAlreadyLoaded(x)))
                                //执行部署，将其加入到应用程序域中
                                PerformFileDeploy(plugin);

                            //init plugin type (only one plugin per assembly is allowed)
                            //遍历当前加载的插件程序集中的所有类型
                            foreach (var type in pluginDescriptor.ReferencedAssembly.GetTypes())
                            {
                                //判断type是否继承自IPlugin接口或者说实现了IPlugin接口
                                if (typeof(IPlugin).IsAssignableFrom(type))
                                {
                                    //判断type不是一个接口
                                    if (!type.IsInterface)
                                    {
                                        //判断type是一个类并且type不是一个抽象类
                                        if (type.IsClass && !type.IsAbstract)
                                        {
                                            pluginDescriptor.PluginType = type;
                                            break;
                                        }
                                    }
                                }
                            }
                            //已经部署好的插件
                            referencedPlugins.Add(pluginDescriptor);
                        }
                        catch (ReflectionTypeLoadException ex)
                        {
                            var msg = string.Empty;
                            foreach (var e in ex.LoaderExceptions)
                                msg += e.Message + Environment.NewLine;

                            var fail = new Exception(msg, ex);
                            Debug.WriteLine(fail.Message, fail);

                            throw fail;
                        }
                    }
                }
                catch (Exception ex)
                {
                    var msg = string.Empty;
                    for (var e = ex; e != null; e = e.InnerException)
                        msg += e.Message + Environment.NewLine;

                    var fail = new Exception(msg, ex);
                    Debug.WriteLine(fail.Message, fail);

                    throw fail;
                }


                ReferencedPlugins = referencedPlugins;
                //IncompatiblePlugins = incompatiblePlugins;
            }
        }

        /// <summary>
        /// 获取已安装的插件的绝对路径
        /// </summary>
        /// <returns></returns>
        private static string GetInstalledPluginsFilePath()
        {
            var filePath = HostingEnvironment.MapPath(InstalledPluginsFilePath);
            return filePath;
        }

        /// <summary>
        /// 指定的目录是否具有父目录并且父目录是Plugins
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        private static bool IsPackagePluginFolder(DirectoryInfo folder)
        {
            if (folder == null)
                return false;
            //当前文件夹必须有父目录
            if (folder.Parent == null)
                return false;
            //当前文件夹的父目录必须是Plugins
            if (!folder.Parent.Name.Equals("Plugins", StringComparison.InvariantCultureIgnoreCase))
                return false;
            return true;
        }

        /// <summary>
        /// 初始化插件，当前运行环境是中等信任级别
        /// <para>需要比较文件的版本</para>
        /// </summary>
        /// <param name="plug">插件文件信息</param>
        /// <param name="shadowCopyPlugFolder">卷影拷贝目录</param>
        /// <returns></returns>
        private static FileInfo InitializeMediumTrust(FileInfo plug, DirectoryInfo shadowCopyPlugFolder)
        {
            var shouldCopy = true;
            //当前卷影拷贝目录+插件文件名称
            var filePath = Path.Combine(shadowCopyPlugFolder.FullName, plug.Name);
            //卷影拷贝目录文件全路径
            var shadowCopiedPlug = new FileInfo(filePath);

            //卷影拷贝目录里的文件是否存在
            if (shadowCopiedPlug.Exists)
            {
                //it's better to use LastWriteTimeUTC, but not all file systems have this property
                //maybe it is better to compare file hash?

                //根据创建时间，比较文件新旧
                var areFilesIdentical = shadowCopiedPlug.CreationTimeUtc.Ticks > plug.CreationTimeUtc.Ticks;
                if (areFilesIdentical)
                {
                    Debug.WriteLine("Not copying; files appear identical: '{0}'", shadowCopiedPlug.Name);
                    shouldCopy = false;
                }
                else
                {
                    //delete an existing file
                    //More info: http://www.nopcommerce.com/boards/t/11511/access-error-nopplugindiscountrulesbillingcountrydll.aspx?p=4#60838
                    Debug.WriteLine("New plugin found; Deleting the old file: '{0}'", shadowCopiedPlug.Name);
                    //删除旧文件
                    File.Delete(shadowCopiedPlug.FullName);
                }
            }

            //应该更新文件
            if (shouldCopy)
            {
                try
                {
                    //将现有文件复制到新文件,允许覆盖同名的文件。
                    File.Copy(plug.FullName, shadowCopiedPlug.FullName, true);
                }
                catch (IOException)
                {
                    Debug.WriteLine(shadowCopiedPlug.FullName + " is locked, attempting to rename");
                    //this occurs when the files are locked,
                    //for some reason devenv locks plugin files some times and for another crazy reason you are allowed to rename them
                    //which releases the lock, so that it what we are doing here, once it's renamed, we can re-shadow copy
                    try
                    {
                        //需要被覆盖的文件被锁定，重命名一个文件名字
                        var oldFile = shadowCopiedPlug.FullName + Guid.NewGuid().ToString("N") + ".old";
                        //将文件被锁定的文件移动到新文件中
                        File.Move(shadowCopiedPlug.FullName, oldFile);
                    }
                    catch (IOException exc)
                    {
                        throw new IOException(shadowCopiedPlug.FullName + " rename failed, cannot initialize plugin", exc);
                    }
                    //ok, we've made it this far, now retry the shadow copy
                    File.Copy(plug.FullName, shadowCopiedPlug.FullName, true);
                }
            }

            return shadowCopiedPlug;
        }

        /// <summary>
        /// 初始化插件，当前运行环境是完全信任级别
        /// <para>不需要比较文件的版本</para>
        /// </summary>
        /// <param name="plug">插件文件信息</param>
        /// <param name="shadowCopyPlugFolder">
        /// 卷影拷贝目录
        /// <para>Asp.net运行的临时目录C:\Windows\Microsoft.NET\Framework64\v4.0.30319\Temporary ASP.NET Files\root\e22c2559\92c7e946</para>
        /// </param>
        /// <returns></returns>
        private static FileInfo InitializeFullTrust(FileInfo plug, DirectoryInfo shadowCopyPlugFolder)
        {
            //当前卷影拷贝目录+插件文件名称
            var filePath = Path.Combine(shadowCopyPlugFolder.FullName, plug.Name);
            //卷影拷贝目录文件全路径
            var shadowCopiedPlug = new FileInfo(filePath);

            try
            {
                //将现有文件复制到新文件,允许覆盖同名的文件。
                File.Copy(plug.FullName, shadowCopiedPlug.FullName, true);
            }
            catch (IOException)
            {
                Debug.WriteLine(shadowCopiedPlug.FullName + " is locked, attempting to rename");
                //this occurs when the files are locked,
                //for some reason devenv locks plugin files some times and for another crazy reason you are allowed to rename them
                //which releases the lock, so that it what we are doing here, once it's renamed, we can re-shadow copy
                try
                {
                    //需要被覆盖的文件被锁定，重命名一个文件名字
                    var oldFile = shadowCopiedPlug.FullName + Guid.NewGuid().ToString("N") + ".old";
                    //将文件被锁定的文件移动到新文件中
                    File.Move(shadowCopiedPlug.FullName, oldFile);
                }
                catch (IOException exc)
                {
                    throw new IOException(shadowCopiedPlug.FullName + " rename failed, cannot initialize plugin", exc);
                }
                //ok, we've made it this far, now retry the shadow copy
                File.Copy(plug.FullName, shadowCopiedPlug.FullName, true);
            }

            return shadowCopiedPlug;
        }

        /// <summary>
        /// 执行文件部署，将插件程序集加载到当前应用程序域中，也就是说把不属于本应用程序的程序集装载到当前应用程序
        /// </summary>
        /// <param name="plug"> 需要部署的插件文件 </param>
        /// <returns></returns>
        private static Assembly PerformFileDeploy(FileInfo plug)
        {
            //插件文件不具有一个父目录
            if (plug.Directory != null && plug.Directory.Parent == null)
            {
                throw new InvalidOperationException("The plugin directory for the " + plug.Name +
                                                   " file exists in a folder outside of the allowed application folder hierarchy.");
            }

            FileInfo shadowCopiedPlug;

            //Unrestricted : 指示任何要求获得使用应用程序的所有功能的权限的请求都会得到准许。 这等效于在配置文件的 trust 节中授予 Full 信任级别
            if (CommonUtils.GetTrustLevel() != AspNetHostingPermissionLevel.Unrestricted)
            {
                //all plugins will need to be copied to ~/Plugins/bin/
                //所有插件需要拷贝到 ~/Plugins/bin/
                //this is aboslutely required because all of this relies on probingPaths being set statically in the web.config

                //were running in med trust, so copy to custom bin folder
                var shadowCopyPlugFolder = Directory.CreateDirectory(_shadowCopyFolder.FullName);
                shadowCopiedPlug = InitializeMediumTrust(plug, shadowCopyPlugFolder);
            }
            else
            {
                //IIS Express C:\Users\Administrator\AppData\Local\Temp\Temporary ASP.NET Files\root\abdf3408\394aa106
                //            C:\Users\Administrator\AppData\Local\Temp\Temporary ASP.NET Files\vs\453cb726\b296f2a

                //获取动态目录，也就是Asp.net运行的临时目录  C:\Windows\Microsoft.NET\Framework64\v4.0.30319\Temporary ASP.NET Files\root\e22c2559\92c7e946
                var directory = AppDomain.CurrentDomain.DynamicDirectory;

                Debug.WriteLine(plug.FullName + " to " + directory);

                //were running in full trust so copy to standard dynamic folder
                shadowCopiedPlug = InitializeFullTrust(plug, new DirectoryInfo(directory));
            }

            //we can now register the plugin definition

            //获取给定文件的 AssemblyName
            var assemblyName = AssemblyName.GetAssemblyName(shadowCopiedPlug.FullName);
            //在给定程序集的 AssemblyName 的情况下，加载程序集, 加载指定目录的程序集
            var shadowCopiedAssembly = Assembly.Load(assemblyName);

            //add the reference to the build manager
            Debug.WriteLine("Adding to BuildManager: '{0}'", shadowCopiedAssembly.FullName);

            //将一个程序集添加到应用程序所引用的一组程序集中
            BuildManager.AddReferencedAssembly(shadowCopiedAssembly);


            return shadowCopiedAssembly;
        }

        /// <summary>
        /// 判断当前的dll文件是否是当前程序域中已经存在的dll，插件dll是不会存在的，因为他是一个额外的组件
        /// </summary>
        /// <param name="fileInfo">指定的文件信息</param>
        /// <returns>Result</returns>
        private static bool IsAlreadyLoaded(FileInfo fileInfo)
        {
            //do not compare the full assembly name, just filename
            try
            {
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileInfo.FullName);

                if (string.IsNullOrEmpty(fileNameWithoutExt))
                    throw new Exception(string.Format("Cannot get file extnension for {0}", fileInfo.Name));

                //遍历当前应用程序域中的所有程序集
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    string assemblyName = assembly.FullName.Split(new[] { ',' }).FirstOrDefault();
                    //指定文件的文件名称是否与程序集名称相等
                    if (fileNameWithoutExt.Equals(assemblyName, StringComparison.InvariantCultureIgnoreCase))
                        return true;
                }
            }
            catch (Exception exc)
            {
                Debug.WriteLine("Cannot validate whether an assembly is already loaded. " + exc);
            }
            return false;
        }


        /// <summary>
        /// 获取描述插件的文件集合
        /// </summary>
        /// <param name="pluginFolder">插件目录文件夹信息</param>
        /// <returns>Original and parsed description files</returns>
        private static IEnumerable<KeyValuePair<FileInfo, PluginDescriptor>> GetDescriptionFilesAndDescriptors(DirectoryInfo pluginFolder)
        {
            if (pluginFolder == null)
                throw new ArgumentNullException("pluginFolder");

            //create list (<file info, parsed plugin descritor>)
            var result = new List<KeyValuePair<FileInfo, PluginDescriptor>>();
            //add display order and path to list
            //遍历获取插件文件夹下面的所有描述文件
            foreach (var descriptionFile in pluginFolder.GetFiles("Description.json", SearchOption.AllDirectories))
            {
                if (!IsPackagePluginFolder(descriptionFile.Directory))
                    continue;

                //parse file 解析描述文件
                var pluginDescriptor = PluginFileParser.ParsePluginDescriptionFile(descriptionFile.FullName);

                //populate list
                //以键值对的形式保存数据
                result.Add(new KeyValuePair<FileInfo, PluginDescriptor>(descriptionFile, pluginDescriptor));
            }

            //sort list by display order. NOTE: Lowest DisplayOrder will be first i.e 0 , 1, 1, 1, 5, 10
            //it's required: http://www.nopcommerce.com/boards/t/17455/load-plugins-based-on-their-displayorder-on-startup.aspx
            result.Sort((firstPair, nextPair) => firstPair.Value.DisplayOrder.CompareTo(nextPair.Value.DisplayOrder));
            return result;
        }

        /// <summary>
        /// 卸载所有插件，其实就是删除所有插件文件
        /// </summary>
        public static void MarkAllPluginsAsUninstalled()
        {
            var filePath = HostingEnvironment.MapPath(InstalledPluginsFilePath);
            if (string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                File.Delete(filePath);
        }

        /// <summary>
        /// 标记指定的插件为卸载，其实就是从已经安装的文件描述中，将该文件名字删除
        /// Mark plugin as uninstalled
        /// </summary>
        /// <param name="systemName">插件系统名称</param>
        public static void MarkPluginAsUninstalled(string systemName)
        {
            if (String.IsNullOrEmpty(systemName))
                throw new ArgumentNullException("systemName");

            var filePath = HostingEnvironment.MapPath(InstalledPluginsFilePath);
            if (!File.Exists(filePath))
                using (File.Create(filePath))
                {
                    //we use 'using' to close the file after it's created
                }


            var installedPluginSystemNames = PluginFileParser.ParseInstalledPluginsFile(GetInstalledPluginsFilePath());
            //判断该插件是否存在于已经安装的文件描述中
            bool alreadyMarkedAsInstalled = installedPluginSystemNames
                                .FirstOrDefault(x => x.Equals(systemName, StringComparison.InvariantCultureIgnoreCase)) != null;
            //如果存在则移除
            if (alreadyMarkedAsInstalled)
                installedPluginSystemNames.Remove(systemName);
            PluginFileParser.SaveInstalledPluginsFile(installedPluginSystemNames, filePath);
        }

        /// <summary>
        /// 标记当指定插件已经安装，就是将当前文件名字写入到已经安装的文件描述中
        /// </summary>
        /// <param name="systemName">插件系统名称</param>
        public static void MarkPluginAsInstalled(string systemName)
        {
            if (String.IsNullOrEmpty(systemName))
                throw new ArgumentNullException("systemName");

            var filePath = HostingEnvironment.MapPath(InstalledPluginsFilePath);
            if (!File.Exists(filePath))
                using (File.Create(filePath))
                {
                    //we use 'using' to close the file after it's created
                }

            //获取已经安装的名字列表
            var installedPluginSystemNames = PluginFileParser.ParseInstalledPluginsFile(GetInstalledPluginsFilePath());
            //判断是否已经安装
            bool alreadyMarkedAsInstalled = installedPluginSystemNames
                                .FirstOrDefault(x => x.Equals(systemName, StringComparison.InvariantCultureIgnoreCase)) != null;
            //如果没有安装，也就是说还没有添加到已经安装的文件描述文件中，将其添加
            if (!alreadyMarkedAsInstalled)
                installedPluginSystemNames.Add(systemName);
            //保存插件描述文件
            PluginFileParser.SaveInstalledPluginsFile(installedPluginSystemNames, filePath);
        }
    }
}
