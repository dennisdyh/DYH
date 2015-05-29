using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DYH.Core.Abstract
{
    /// <summary>
    /// 类型信息获取接口
    /// </summary>
    public interface ITypeFinder
    {
        /// <summary>
        /// 获取程序集列表
        /// </summary>
        /// <returns></returns>
        IList<Assembly> GetAssemblies();
        /// <summary>
        /// 默认获取给定的程序集中的所有的公开实体类型，因为onlyConcreteClasses=true。
        /// </summary>
        /// <param name="assignTypeFrom">被扩展的类型:父类型</param>
        /// <param name="onlyConcreteClasses">是否是只获取实体类，即该类不是抽象类或者接口</param>
        /// <returns></returns>
        IEnumerable<Type> FindClassesOfType(Type assignTypeFrom, bool onlyConcreteClasses = true);
        /// <summary>
        /// 默认获取给定的程序集中的所有的公开实体类型，因为onlyConcreteClasses=true。
        /// </summary>
        /// <param name="assignTypeFrom">被扩展的类型:父类型</param>
        /// <param name="assemblies">给定的程序集集合</param>
        /// <param name="onlyConcreteClasses">是否是只获取实体类，即该类不是抽象类或者接口</param>
        /// <returns></returns>
        IEnumerable<Type> FindClassesOfType(Type assignTypeFrom, IEnumerable<Assembly> assemblies, bool onlyConcreteClasses = true);
        /// <summary>
        ///默认获取给定的程序集中的所有的公开实体类型，因为onlyConcreteClasses=true。
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="onlyConcreteClasses">是否是只获取实体类，即该类不是抽象类或者接口</param>
        /// <returns></returns>
        IEnumerable<Type> FindClassesOfType<T>(bool onlyConcreteClasses = true);
        /// <summary>
        /// 默认获取给定的程序集中的所有的公开实体类型，因为onlyConcreteClasses=true。
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="assemblies">给定的程序集集合</param>
        /// <param name="onlyConcreteClasses">是否是只获取实体类，即该类不是抽象类或者接口</param>
        /// <returns></returns>
        IEnumerable<Type> FindClassesOfType<T>(IEnumerable<Assembly> assemblies, bool onlyConcreteClasses = true);
    }
}
