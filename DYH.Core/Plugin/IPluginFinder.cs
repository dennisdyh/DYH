using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DYH.Core.Plugin
{
    public interface IPluginFinder
    {
        /// <summary>
        /// 获取插件集合
        /// </summary>
        /// <typeparam name="T">
        /// 插件类型
        /// </typeparam>
        /// <param name="installedOnly">
        /// 设置一个值是否只加载已经安装的插件
        /// </param>
        /// <returns>Plugins</returns>
        IEnumerable<T> GetPlugins<T>(bool installedOnly = true) where T : class, IPlugin;

        /// <summary>
        /// 获取插件描述信息集合
        /// </summary>
        /// <typeparam name="T">
        /// 插件类型
        /// </typeparam>
        /// <param name="installedOnly">
        /// 设置一个值是否只加载已经安装的插件
        /// </param>
        /// <returns>Plugin descriptors</returns>
        IEnumerable<PluginDescriptor> GetPluginDescriptors<T>(bool installedOnly = true) where T : class, IPlugin;

        /// <summary>
        /// 通过插件唯一的系统名称获取插件描述信息
        /// </summary>
        /// <typeparam name="T">插件类型</typeparam>
        /// <param name="systemName">插件唯一系统名称</param>
        /// <param name="installedOnly">设置一个值是否只加载已经安装的插件</param>
        /// <returns>Plugin descriptor</returns>
        PluginDescriptor GetPluginDescriptorBySystemName<T>(string systemName, bool installedOnly = true) where T : class, IPlugin;

        /// <summary>
        /// 重新加载插件
        /// </summary>
        void ReloadPlugins();
    }
}
