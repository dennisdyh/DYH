using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using DYH.Core.Abstract;
namespace DYH.Core.Basic
{
    public class TypeFinder : ITypeFinder
    {
        /// <summary>
        /// 是否忽略引用错误 当前true
        /// </summary>
        private const bool IgnoreReflectionErrors = true;

        /// <summary>
        /// 获取当前应用程序域，在当前应用程序域中查找类型
        /// </summary>
        public virtual AppDomain App
        {
            get { return AppDomain.CurrentDomain; }
        }

        private bool _loadAppDomainAssemblies = true;
        /// <summary>
        /// 是否加载应用程序域的程序集 当前ture
        /// </summary>
        public bool LoadAppDomainAssemblies {
            get { return true;}
            set { _loadAppDomainAssemblies = value; }
        }

        private IList<string> _assemblyNames = new List<string>();
        /// <summary>
        /// 获取或者设置要加载的程序集，除了应用程序域中已经加载了的。
        /// </summary>
        public IList<string> AssemblyNames {
            get { return _assemblyNames; }
            set { _assemblyNames = value; }
        }
 
        private string _assemblySkipLoadingPattern = "^System|^mscorlib|^Microsoft|^AjaxControlToolkit|^Antlr3|^Autofac|^AutoMapper|^Castle|^ComponentArt|^CppCodeProvider|^DotNetOpenAuth|^EntityFramework|^EPPlus|^FluentValidation|^ImageResizer|^itextsharp|^log4net|^MaxMind|^MbUnit|^MiniProfiler|^Mono.Math|^MvcContrib|^Newtonsoft|^NHibernate|^nunit|^Org.Mentalis|^PerlRegex|^QuickGraph|^Recaptcha|^Remotion|^RestSharp|^Rhino|^Telerik|^Iesi|^TestDriven|^TestFu|^UserAgentStringLibrary|^VJSharpCodeProvider|^WebActivator|^WebDev|^WebGrease";
        /// <summary>
        /// 获取或者设置应该忽略加载的程序集匹配符
        /// </summary>
        public string AssemblySkipLoadingPattern
        {
            get { return _assemblySkipLoadingPattern; }
            set { _assemblySkipLoadingPattern = value; }
        }

        private string _assemblyRestrictToLoadingPattern = ".*";
        /// <summary>
        /// 获取或者设置将要被加载的的DLL的匹配模式
        /// </summary>
        /// <remarks>
        /// </remarks>
        public string AssemblyRestrictToLoadingPattern {
            get { return _assemblyRestrictToLoadingPattern; }
            set { _assemblyRestrictToLoadingPattern = value; }
        }

        /// <summary>
        /// 加载所需程序集，并且返回该集合
        /// </summary>
        /// <returns>返回当前应用需要加载的程序集集合</returns>
        public virtual IList<Assembly> GetAssemblies()
        {
            //程序集显示名称集合
            var addedAssemblyNames = new List<string>();
            //程序集集合
            var assemblies = new List<Assembly>();
            //是否加载程序集
            if (LoadAppDomainAssemblies)
                AddAssembliesInAppDomain(addedAssemblyNames, assemblies);

            AddConfiguredAssemblies(addedAssemblyNames, assemblies);

            return assemblies;
        }

        /// <summary>
        ///默认获取给定的程序集中的所有的公开实体类型，因为onlyConcreteClasses=true。
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="onlyConcreteClasses">是否是只获取实体类，即该类不是抽象类或者接口</param>
        /// <returns></returns>
        public IEnumerable<Type> FindClassesOfType<T>(bool onlyConcreteClasses = true)
        {
            return FindClassesOfType(typeof(T), onlyConcreteClasses);
        }
        /// <summary>
        /// 默认获取给定的程序集中的所有的公开实体类型，因为onlyConcreteClasses=true。
        /// </summary>
        /// <param name="assignTypeFrom">被扩展的类型:父类型</param>
        /// <param name="onlyConcreteClasses">是否是只获取实体类，即该类不是抽象类或者接口</param>
        /// <returns></returns>
        public IEnumerable<Type> FindClassesOfType(Type assignTypeFrom, bool onlyConcreteClasses = true)
        {
            return FindClassesOfType(assignTypeFrom, GetAssemblies(), onlyConcreteClasses);
        }

        /// <summary>
        /// 默认获取给定的程序集中的所有的公开实体类型，因为onlyConcreteClasses=true。
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="assemblies">给定的程序集集合</param>
        /// <param name="onlyConcreteClasses">是否是只获取实体类，即该类不是抽象类或者接口</param>
        /// <returns></returns>
        public IEnumerable<Type> FindClassesOfType<T>(IEnumerable<Assembly> assemblies, bool onlyConcreteClasses = true)
        {
            return FindClassesOfType(typeof(T), assemblies, onlyConcreteClasses);
        }

        /// <summary>
        /// 默认获取给定的程序集中的所有的公开实体类型，因为onlyConcreteClasses=true。
        /// </summary>
        /// <param name="assignTypeFrom">被扩展的类型:父类型</param>
        /// <param name="assemblies">给定的程序集集合</param>
        /// <param name="onlyConcreteClasses">是否是只获取实体类，即该类不是抽象类或者接口</param>
        /// <returns></returns>
        public IEnumerable<Type> FindClassesOfType(Type assignTypeFrom, IEnumerable<Assembly> assemblies, bool onlyConcreteClasses = true)
        {
            //返回实体类型
            var result = new List<Type>();
            try
            {
                //遍历给定的程序集集合
                foreach (var a in assemblies)
                {
                    Type[] types = null;
                    try
                    {
                        //获取给定程序集里面的所有的公开类型
                        types = a.GetTypes();
                    }
                    catch
                    {
                        //Entity Framework 6 doesn't allow getting types (throws an exception)
                        if (!IgnoreReflectionErrors)
                        {
                            throw;
                        }
                    }
                    if (types != null)
                    {
                        //遍历每一个类型
                        foreach (var t in types)
                        {
                            //遍历中的类型是否继承自给定的父类
                            if (assignTypeFrom.IsAssignableFrom(t) ||
                                (assignTypeFrom.IsGenericTypeDefinition &&
                                DoesTypeImplementOpenGeneric(t, assignTypeFrom)))
                            {
                                //如果遍历的类型不是接口
                                if (!t.IsInterface)
                                {
                                    //如果该类型是一个具体的类
                                    if (onlyConcreteClasses)
                                    {
                                        //t 是一个类，并且t 不是抽象类
                                        if (t.IsClass && !t.IsAbstract)
                                        {
                                            result.Add(t);
                                        }
                                    }
                                    else
                                    {
                                        result.Add(t);
                                    }
                                }
                            }
                        }
                    }
                }
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

            return result;
        }
      

        /// <summary>
        /// 匹配指定的程序集名称
        /// </summary>
        /// <param name="assemblyFullName">
        /// 需要检查的dll名称
        /// </param>
        /// <returns>
        /// 如果是真则我们需要加载，假就是不需要加载
        /// </returns>
        public virtual bool Matches(string assemblyFullName)
        {
            return
                //当前程序集名称 和 不需要加载的程序集的正则表达式 进行匹配；例如 System, ^System 匹配返回true ！true = false 不加载因为指定不加载了。
                !Matches(assemblyFullName, AssemblySkipLoadingPattern)
                   &&
                //当前程序集名称 和 一定需要加载的正则表达式； 例如： DYH.Core , DYH.* 匹配返回真 
                Matches(assemblyFullName, AssemblyRestrictToLoadingPattern);
        }

        /// <summary>
        /// 当前程序集名称是否与正则表达式匹配
        /// </summary>
        /// <param name="assemblyFullName">
        /// 需要比较的程序集名称
        /// </param>
        /// <param name="pattern">
        /// 程序集名称正则表达式
        /// </param>
        /// <returns>
        /// 如果匹配返回真，否则假
        /// </returns>
        protected virtual bool Matches(string assemblyFullName, string pattern)
        {
            return Regex.IsMatch(assemblyFullName, pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        /// <summary>
        /// 确认指定的类型是否扩展自给定的泛型模板例如List&lt;T&gt;
        /// </summary>
        /// <param name="type">给定的类型</param>
        /// <param name="openGeneric">开放式的泛型， 例如：List&lt;&gt; 尖括号中没有给定类型</param>
        /// <returns></returns>
        protected virtual bool DoesTypeImplementOpenGeneric(Type type, Type openGeneric)
        {
            try
            {
                var genericTypeDefinition = openGeneric.GetGenericTypeDefinition();
                foreach (var implementedInterface in type.FindInterfaces((objType, objCriteria) => true, null))
                {
                    if (!implementedInterface.IsGenericType)
                        continue;

                    var isMatch = genericTypeDefinition.IsAssignableFrom(implementedInterface.GetGenericTypeDefinition());
                    return isMatch;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 加载指定目录的匹配程序集
        /// </summary>
        /// <param name="directoryPath">
        /// 包含程序集的指定目录
        /// </param>
        protected virtual void LoadMatchingAssemblies(string directoryPath)
        {
            var loadedAssemblyNames = new List<string>();
            //遍历所有需要加载的程序集
            foreach (Assembly a in GetAssemblies())
            {
                loadedAssemblyNames.Add(a.FullName);
            }

            if (!Directory.Exists(directoryPath))
            {
                return;
            }
            //遍历指定目录的所有dll
            foreach (string dllPath in Directory.GetFiles(directoryPath, "*.dll"))
            {
                try
                {
                    var an = AssemblyName.GetAssemblyName(dllPath);
                    if (Matches(an.FullName) && !loadedAssemblyNames.Contains(an.FullName))
                    {
                        App.Load(an);
                    }
                }
                catch (BadImageFormatException ex)
                {
                    Trace.TraceError(ex.ToString());
                }
            }
        }

        /// <summary>
        /// 在当前应用程序域中迭代所有的程序集,仅添加需要的程序集(添加匹配模式的程序集)
        /// </summary>
        /// <param name="addedAssemblyNames">表示存储已经添加的程序集的显示名称集合</param>
        /// <param name="assemblies">表示存储已经添加了的程序集</param>
        private void AddAssembliesInAppDomain(List<string> addedAssemblyNames, List<Assembly> assemblies)
        {

            //遍历当前应用程序域中的所有程序集
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                //判断当前程序集是否应该加载
                if (Matches(assembly.FullName))
                {
                    //判断已经加载的列表中是否存在该程序集名称
                    if (!addedAssemblyNames.Contains(assembly.FullName))
                    {
                        assemblies.Add(assembly);
                        addedAssemblyNames.Add(assembly.FullName);
                    }
                }
            }
        }

        /// <summary>
        /// 添加指定配置的程序集，该方法目前没有用，除非在派生类重写
        /// </summary>
        /// <param name="addedAssemblyNames">表示存储已经添加的程序集的显示名称集合</param>
        /// <param name="assemblies"></param>
        protected virtual void AddConfiguredAssemblies(List<string> addedAssemblyNames, List<Assembly> assemblies)
        {
            foreach (string assemblyName in AssemblyNames)
            {
                Assembly assembly = Assembly.Load(assemblyName);
                if (!addedAssemblyNames.Contains(assembly.FullName))
                {
                    assemblies.Add(assembly);
                    addedAssemblyNames.Add(assembly.FullName);
                }
            }
        }
      
    }
}
