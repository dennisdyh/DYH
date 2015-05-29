using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DYH.Core.Plugin
{
    public static class PluginFileParser
    {
        /// <summary>
        /// 解析已安装插件列表
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static IList<string> ParseInstalledPluginsFile(string filePath)
        {
            var result = new List<string>();
            if (!File.Exists(filePath))
                return result;

            var text = File.ReadAllText(filePath);
            if (string.IsNullOrEmpty(text))
                return result;

            return JsonConvert.DeserializeObject<List<string>>(text);
        }

        /// <summary>
        /// 保存已安装插件列表
        /// </summary>
        /// <param name="plugins">已安装插件</param>
        /// <param name="filePath">保存路径</param>
        public static void SaveInstalledPluginsFile(IList<string> plugins, string filePath)
        {
            var text = JsonConvert.SerializeObject(plugins);
            File.WriteAllText(filePath, text);
        }

        /// <summary>
        /// 解析插件描述文件
        /// </summary>
        /// <param name="filePath">插件描述文件路径</param>
        /// <returns></returns>
        public static PluginDescriptor ParsePluginDescriptionFile(string filePath)
        {
            var text = File.ReadAllText(filePath);
            var description = JsonConvert.DeserializeObject<PluginDescriptor>(text);

            return description;
        }

        /// <summary>
        /// 保存插件描述文件
        /// </summary>
        /// <param name="description">插件描述文件</param>
        public static void SavePluginDescriptionFile(PluginDescriptor description)
        {
            if (description == null)
                throw new ArgumentNullException("description");

            if (description.OriginalAssemblyFile == null)
                throw new Exception(string.Format("Cannot load original assembly path for {0} plugin.", description.DisplayName));
            var filePath = Path.Combine(description.OriginalAssemblyFile.Directory.FullName, "Plugin.json");
            if (!File.Exists(filePath))
                throw new Exception(string.Format("Plugin file for {0} plugin does not exist. {1}", description.DisplayName, filePath));

            var text = JsonConvert.SerializeObject(new
            {
                DisplayName = description.DisplayName,
                DisplayOrder = description.DisplayOrder,
                PluginFileName = description.PluginFileName,
                SystemName = description.SystemName,
                Version = description.Version
            });

            File.WriteAllText(filePath, text);
        }
    }
}
