using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Routing;
using DYH.Core.Abstract;

using DYH.Core.Plugin;


namespace MvcApp
{
    public class RouteRegistrar : IRouteRegistrar
    {
        private readonly ITypeFinder _typeFinder;
        public RouteRegistrar(ITypeFinder typeFinder)
        {
            _typeFinder = typeFinder;
        }

        protected virtual PluginDescriptor FindPlugin(Type providerType)
        {
            if (providerType == null)
                throw new ArgumentNullException("providerType");

            foreach (var plugin in PluginManager.ReferencedPlugins)
            {
                if (plugin.ReferencedAssembly == null)
                    continue;

                if (plugin.ReferencedAssembly.FullName == providerType.Assembly.FullName)
                    return plugin;
            }

            return null;
        }

        public void RegisterRoutes(RouteCollection routes)
        {
            var routeProviderTypes = _typeFinder.FindClassesOfType<IRouteProvider>();

            var routeProviders = new List<IRouteProvider>();
            foreach (var providerType in routeProviderTypes)
            {
                //Ignore not installed plugins
                var plugin = FindPlugin(providerType);
                if (plugin != null && !plugin.Installed)
                    continue;

                var provider = Activator.CreateInstance(providerType) as IRouteProvider;
                routeProviders.Add(provider);
            }

            routeProviders = routeProviders.OrderByDescending(rp => rp.Priority).ToList();
            routeProviders.ForEach(rp => rp.RegisterRoutes(routes));
        }
    }
}
