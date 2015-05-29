using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Routing;

namespace DYH.Core.Abstract
{
    /// <summary>
    /// 注册路由接口
    /// </summary>
    public interface IRouteRegistrar
    {
        /// <summary>
        /// 注册路由
        /// </summary>
        /// <param name="routeCollection"></param>
        void RegisterRoutes(RouteCollection routeCollection);
    }
}
