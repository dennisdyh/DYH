using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DYH.Core.Basic;

namespace DYH.Core.Abstract
{
    public interface IApp
    {
        /// <summary>
        /// <para>容器管理者</para>
        /// </summary>
        ContainerManager ContainerManager { get; }

        /// <summary>
        /// 利用配置文件对插件和组件进行初始化
        /// </summary>
        void Initialize();

        /// <summary>
        /// <para>从当前上下文中返回指定类型的实例</para>
        /// </summary>
        /// <typeparam name="T">T 类型占位符</typeparam>
        /// <returns></returns>
        T Resolve<T>() where T : class;

        /// <summary>
        /// <para>从当前上下文中返回指定类型的实例</para>
        /// </summary>
        /// <param name="type">指定类型</param>
        /// <returns></returns>
        object Resolve(Type type);

        /// <summary>
        /// <para>从当前上下文中返回指定类型的实例</para>
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <returns></returns>
        T[] ResolveAll<T>();

        /// <summary>
        /// 重新启动应用程序
        /// </summary>
        /// <param name="makeRedirect">
        /// 设置一个值是否在重启后需要重定向
        /// </param>
        /// <param name="redirectUrl">
        /// 需要重定向的URL，如果为空则重定向到当前页
        /// </param>
        void RestartAppDomain(bool makeRedirect = false, string redirectUrl = "");
    }
}
