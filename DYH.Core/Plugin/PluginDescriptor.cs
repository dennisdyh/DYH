using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DYH.Core.Basic;

namespace DYH.Core.Plugin
{
    [Serializable]
    public class PluginDescriptor
    {
        /// <summary>
        /// 插件名称
        /// </summary>
        public string DisplayName { get; set; }
        /// <summary>
        /// 插件系统名称,该名称是唯一的
        /// </summary>
        public string SystemName { get; set; }
        /// <summary>
        /// 插件文件名称:dll名称
        /// </summary>
        public string PluginFileName { get; set; }
        /// <summary>
        /// 插件类型
        /// </summary>
        public Type PluginType { get; set; }
        /// <summary>
        /// 插件版本
        /// </summary>
        public string Version { get; set; }
        /// <summary>
        /// 插件序列
        /// </summary>
        public int DisplayOrder { get; set; }
        /// <summary>
        /// 是否已经安装
        /// </summary>
        public bool Installed { get; set; }
        /// <summary>
        /// 一个组成卷影拷贝的原生程序集文件
        /// <para>插件自己本身的文件信息</para>
        /// </summary>
        public FileInfo OriginalAssemblyFile { get; internal set; }
        /// <summary>        
        /// 已经部署到当前应用程序的插件程序集信息
        /// </summary>
        public Assembly ReferencedAssembly { get; internal set; }

        /// <summary>
        /// 实例化插件
        /// </summary>
        /// <typeparam name="T">类型占位符</typeparam>
        /// <returns></returns>
        public virtual T Instance<T>() where T : class, IPlugin
        {
            object instance;
            if (!AppContext.Current.ContainerManager.TryResolve(PluginType, null, out instance))
            {
                //not resolved
                instance = AppContext.Current.ContainerManager.ResolveUnregistered(PluginType);
            }
            var typedInstance = instance as T;
            if (typedInstance != null)
                typedInstance.PluginDescriptor = this;

            return typedInstance;
        }

        /// <summary>
        /// 实例化插件
        /// </summary>
        /// <returns></returns>
        public IPlugin Instance()
        {
            return Instance<IPlugin>();
        }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}
