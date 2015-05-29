using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DYH.Core.Plugin
{
    /// <summary>
    /// 插件接口
    /// </summary>
    public interface IPlugin
    {
        /// <summary>
        /// 获取或者设置插件的信息
        /// </summary>
        PluginDescriptor PluginDescriptor { get; set; }
        /// <summary>
        /// 安装插件
        /// </summary>
        /// <returns></returns>
        void Install();
        /// <summary>
        /// 卸载插件
        /// </summary>
        void Uninstall();
    }
}
