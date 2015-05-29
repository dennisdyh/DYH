using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Hosting;
using Autofac;
using Autofac.Integration.Mvc;
using DYH.Core.Abstract;
using DYH.Core.Basic;

using DYH.Core.Data;
using DYH.Core.Plugin;

using DYH.Framework.Data;
using DYH.Framework.Models;

namespace MvcApp
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public void Register(ContainerBuilder builder, ITypeFinder typeFinder)
        {
            builder.Register(c => new HttpContextWrapper(HttpContext.Current) as HttpContextBase)
              .As<HttpContextBase>()
              .InstancePerLifetimeScope();

            builder.RegisterType<AppRuntime>().As<IApp>().InstancePerLifetimeScope();
            builder.RegisterType<PluginFinder>().As<IPluginFinder>().InstancePerLifetimeScope();

            builder.RegisterType<RouteRegistrar>().As<IRouteRegistrar>().SingleInstance(); ;
      
            var assemblies = new DirectoryInfo(HostingEnvironment.MapPath("~/bin/")).GetFiles("*.dll")
               .Select(r => Assembly.LoadFrom(r.FullName)).ToArray();
            //var list = AppDomain.CurrentDomain.GetAssemblies();

            builder.RegisterControllers(assemblies);
        }

        public int Order
        {
            get { return 0; }
        }
    }
}
