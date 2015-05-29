using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using DYH.Core.Abstract;
using DYH.Core.Basic;


namespace MvcApp
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            var routePublisher = AppContext.Current.Resolve<IRouteRegistrar>();
            routePublisher.RegisterRoutes(routes);

            routes.MapRoute(
                 "Default",
                 "{controller}/{action}/{id}",
                 new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                 new[] { "MvcApp.Controllers" }
            );
        }
    }
}