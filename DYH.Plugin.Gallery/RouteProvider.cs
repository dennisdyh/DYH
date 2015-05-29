using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using DYH.Core.Abstract;

namespace DYH.Plugin.Gallery
{
    public class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("DYH.Plugin.Gallery.Index",
                "Plugins/Gallery/Index",
                new { controller = "Gallery", action = "Index" },
                new[] { "DYH.Plugin.Gallery.Controllers" }
           );
        }

        public int Priority
        {
            get { return 1; }
        }
    }
}
