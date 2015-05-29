using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using DYH.Core.Abstract;

namespace DYH.Plugin.News
{
    public class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("DYH.Plugin.News.Index",
                "Plugins/{controller}/{action}",
                new {controller = "Gallery", action = "Index"},
                new[] {"DYH.Plugin.News.Controllers"}
                );
        }

        public int Priority
        {
            get { return 1; }
        }
    }
}
