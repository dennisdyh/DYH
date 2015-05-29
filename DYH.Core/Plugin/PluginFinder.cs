using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DYH.Core.Plugin
{
    public class PluginFinder : IPluginFinder
    {
        /// <summary>
        /// 已经加载到当前应用程序域中的插件的集合
        /// </summary>
        private IList<PluginDescriptor> _plugins;

        private bool _arePluginsLoaded = false;

        /// <summary>
        /// 确认插件已经被加载
        /// </summary>
        protected virtual void EnsurePluginsAreLoaded()
        {
            if (!_arePluginsLoaded)
            {
                //获取一个已经加载到当前应用程序域中的插件的集合
                var foundPlugins = PluginManager.ReferencedPlugins.ToList();
                foundPlugins.Sort();
                _plugins = foundPlugins.ToList();
                _arePluginsLoaded = true;
            }
        }


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
        public IEnumerable<T> GetPlugins<T>(bool installedOnly = true) where T : class, IPlugin
        {
            EnsurePluginsAreLoaded();
            //遍历当前已经加载的插件集合
            foreach (var plugin in _plugins)
                //判断遍历的插件类型是否是扩展自需要取得的插件类型
                if (typeof(T).IsAssignableFrom(plugin.PluginType))
                    if (!installedOnly || plugin.Installed)
                        yield return plugin.Instance<T>();
        }

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
        public IEnumerable<PluginDescriptor> GetPluginDescriptors<T>(bool installedOnly = true) where T : class, IPlugin
        {
            EnsurePluginsAreLoaded();

            foreach (var plugin in _plugins)
                if (typeof(T).IsAssignableFrom(plugin.PluginType))
                    if (!installedOnly || plugin.Installed)
                        yield return plugin;
        }

        /// <summary>
        /// 通过插件唯一的系统名称获取插件描述信息
        /// </summary>
        /// <typeparam name="T">插件类型</typeparam>
        /// <param name="systemName">插件唯一系统名称</param>
        /// <param name="installedOnly">设置一个值是否只加载已经安装的插件</param>
        /// <returns>Plugin descriptor</returns>
        public PluginDescriptor GetPluginDescriptorBySystemName<T>(string systemName, bool installedOnly = true) where T : class, IPlugin
        {
            return GetPluginDescriptors<T>(installedOnly)
               .SingleOrDefault(p => p.SystemName.Equals(systemName, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// 重新加载插件
        /// </summary>
        public void ReloadPlugins()
        {
            _arePluginsLoaded = false;
            EnsurePluginsAreLoaded();
        }
    }
}
