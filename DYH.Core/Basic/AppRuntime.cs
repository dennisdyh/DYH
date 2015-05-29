using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;
using DYH.Core.Abstract;
using DYH.Core.Utils;

namespace DYH.Core.Basic
{
    public class AppRuntime : IApp
    {
        private ContainerManager _containerManager;

        /// <summary>
        /// <para>从当前上下文中返回指定类型的实例</para>
        /// </summary>
        /// <typeparam name="T">T 类型占位符</typeparam>
        /// <returns></returns>
        public T Resolve<T>() where T : class
        {
            return ContainerManager.Resolve<T>();
        }

        /// <summary>
        /// <para>从当前上下文中返回指定类型的实例</para>
        /// </summary>
        /// <param name="type">指定类型</param>
        /// <returns></returns>
        public object Resolve(Type type)
        {
            return ContainerManager.Resolve(type);
        }

        /// <summary>
        /// <para>从当前上下文中返回指定类型的实例集合</para>
        /// </summary>
        /// <typeparam name="T">指令类型</typeparam>
        /// <returns></returns>
        public T[] ResolveAll<T>()
        {
            return ContainerManager.ResolveAll<T>();
        }

        /// <summary>
        /// Container manager
        /// 容器管理
        /// </summary>
        public ContainerManager ContainerManager
        {
            get { return _containerManager; }
        }

        /// <summary>
        /// 对类型实现解耦
        /// </summary>
        protected virtual void RegisterDependencies()
        {
            var builder = new ContainerBuilder();
            var container = builder.Build();

            //we create new instance of ContainerBuilder
            //because Build() or Update() method can only be called once on a ContainerBuilder.


            //dependencies
            var typeFinder = new WebAppTypeFinder();

            builder = new ContainerBuilder();
            //builder.RegisterInstance(config).As<NopConfig>().SingleInstance();
            builder.RegisterInstance(this).As<IApp>().SingleInstance();
            builder.RegisterInstance(typeFinder).As<ITypeFinder>().SingleInstance();
            builder.Update(container);

            //register dependencies provided by other assemblies
            builder = new ContainerBuilder();
            var drTypes = typeFinder.FindClassesOfType<IDependencyRegistrar>();
            var drInstances = new List<IDependencyRegistrar>();
            foreach (var drType in drTypes)
                drInstances.Add((IDependencyRegistrar)Activator.CreateInstance(drType));
            //sort
            drInstances = drInstances.AsQueryable().OrderBy(t => t.Order).ToList();
            foreach (var dependencyRegistrar in drInstances)
                dependencyRegistrar.Register(builder, typeFinder);

            builder.Update(container);


            this._containerManager = new ContainerManager(container);

            //set dependency resolver
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }

        public void Initialize()
        {
            RegisterDependencies();
        }

      

        /// <summary>
        /// 重新启动应用程序
        /// </summary>
        /// <param name="makeRedirect">
        /// 设置一个值是否在重启后需要重定向
        /// </param>
        /// <param name="redirectUrl">
        /// 需要重定向的URL，如果为空则重定向到当前页
        /// </param>
        public void RestartAppDomain(bool makeRedirect = false, string redirectUrl = "")
        {
            if (CommonUtils.GetTrustLevel() > AspNetHostingPermissionLevel.Medium)
            {
                //full trust
                HttpRuntime.UnloadAppDomain();

                TryWriteGlobalAsax();
            }
            else
            {
                //medium trust
                bool success = TryWriteWebConfig();
                if (!success)
                {
                    throw new Exception("application needs to be restarted due to a configuration change, but was unable to do so." + Environment.NewLine +
                        "To prevent this issue in the future, a change to the web server configuration is required:" + Environment.NewLine +
                        "- run the application in a full trust environment, or" + Environment.NewLine +
                        "- give the application write access to the 'web.config' file.");
                }

                success = TryWriteGlobalAsax();
                if (!success)
                {
                    throw new Exception("application needs to be restarted due to a configuration change, but was unable to do so." + Environment.NewLine +
                        "To prevent this issue in the future, a change to the web server configuration is required:" + Environment.NewLine +
                        "- run the application in a full trust environment, or" + Environment.NewLine +
                        "- give the application write access to the 'Global.asax' file.");
                }
            }

           var httpContext = AppContext.Current.Resolve<HttpContextBase>();
            // If setting up extensions/modules requires an AppDomain restart, it's very unlikely the
            // current request can be processed correctly.  So, we redirect to the same URL, so that the
            // new request will come to the newly started AppDomain.
            if (httpContext != null && makeRedirect)
            {
                if (String.IsNullOrEmpty(redirectUrl))
                    redirectUrl = "/";


                httpContext.Response.Redirect(redirectUrl, true /*endResponse*/);
            }
        }

        /// <summary>
        /// 是否是一个有效的请求
        /// </summary>
        /// <param name="httpContext">Http上下文</param>
        /// <returns></returns>
        protected virtual Boolean IsRequestAvailable(HttpContextBase httpContext)
        {
            if (httpContext == null)
                return false;

            try
            {
                if (httpContext.Request == null)
                    return false;
            }
            catch (HttpException ex)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// File.SetLastWriteTimeUtc(MapPath("~/web.config"), DateTime.UtcNow);
        /// 在中度信任环境中不允许UnloadAppDomain 所以，强制更新web.config来使整个应用重启
        /// </summary>
        /// <returns></returns>
        protected virtual bool TryWriteWebConfig()
        {
            try
            {
                // In medium trust, "UnloadAppDomain" is not supported. Touch web.config
                // to force an AppDomain restart.
                File.SetLastWriteTimeUtc(MapPath("~/web.config"), DateTime.UtcNow);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 更改global.asax文件，以重启应用
        /// </summary>
        /// <returns></returns>
        protected virtual bool TryWriteGlobalAsax()
        {
            try
            {
                //When a new plugin is dropped in the Plugins folder and is installed into nopCommerce, 
                //even if the plugin has registered routes for its controllers, 
                //these routes will not be working as the MVC framework couldn't 
                //find the new controller types and couldn't instantiate the requested controller. 
                //That's why you get these nasty errors 
                //i.e "Controller does not implement IController".
                //The issue is described here: http://www.nopcommerce.com/boards/t/10969/nop-20-plugin.aspx?p=4#51318
                //The solution is to touch global.asax file
                File.SetLastWriteTimeUtc(MapPath("~/global.asax"), DateTime.UtcNow);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Maps a virtual path to a physical disk path.
        /// </summary>
        /// <param name="path">The path to map. E.g. "~/bin"</param>
        /// <returns>The physical path. E.g. "c:\inetpub\wwwroot\bin"</returns>
        public virtual string MapPath(string path)
        {
            if (HostingEnvironment.IsHosted)
            {
                //hosted
                return HostingEnvironment.MapPath(path);
            }
            else
            {
                //not hosted. For example, run in unit tests
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                path = path.Replace("~/", "").TrimStart('/').Replace('/', '\\');
                return Path.Combine(baseDirectory, path);
            }
        }
    }
}
