using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DYH.Core.Plugin
{
    public abstract class BasePlugin : IPlugin
    {
        /// <summary>
        /// 获取或者设置插件信息
        /// </summary>
        public virtual PluginDescriptor PluginDescriptor
        {
            get;
            set;
        }
        /// <summary>
        /// 安装插件
        /// </summary>
        public virtual void Install()
        {
            PluginManager.MarkPluginAsInstalled(this.PluginDescriptor.SystemName);
        }
        /// <summary>
        /// 卸载插件
        /// </summary>
        public virtual void Uninstall()
        {
            PluginManager.MarkPluginAsUninstalled(this.PluginDescriptor.SystemName);
        }
    }
}
