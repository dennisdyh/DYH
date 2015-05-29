using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;

namespace DYH.Core.Basic
{
    public class WebAppTypeFinder : TypeFinder
    {
        /// <summary>
        /// 确认bin目录下的程序集是否被加载完成
        /// </summary>
        private bool _binFolderAssembliesLoaded = false;

        /// <summary>
        /// <para>获取bin目录的物理路径</para>
        /// </summary>
        /// <returns>E.g. "c:\inetpub\wwwroot\bin"</returns>
        public virtual string GetBinDirectory()
        {
            if (HostingEnvironment.IsHosted)
            {
                //hosted
                return HttpRuntime.BinDirectory;
            }

            //not hosted. For example, run either in unit tests
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        /// <summary>
        /// 获取所有的需要加载的程序集
        /// </summary>
        /// <returns></returns>
        public override IList<Assembly> GetAssemblies()
        {
            if (!_binFolderAssembliesLoaded)
            {
                //导入成功
                _binFolderAssembliesLoaded = true;
                string binPath = GetBinDirectory();
                LoadMatchingAssemblies(binPath);
            }
            return base.GetAssemblies();
        }
    }
}
