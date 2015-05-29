using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DYH.Core.Abstract;

using DYH.Core.Plugin;
using DYH.Framework.Data;
using DYH.Framework.Models;


namespace MvcApp.Controllers
{
    public class HomeController : Controller
    {

        private readonly IPluginFinder _pluginFinder;
        private readonly IApp _app;

        public HomeController(IPluginFinder pluginFinder, IApp app)
        {

            _app = app;
            _pluginFinder = pluginFinder;
        }

        //
        // GET: /Home/
        public ActionResult Index()
        {
            var uof = new UnitOfWork("DbConnStr");

            var repository = new Repository<UserEntity, int>(uof);

            var list = repository.FindAll(x => true).ToList();
            return View(list);

        }

        [HttpPost]
        public ActionResult Install(string SystemName)
        {
            var pluginDescriptor = _pluginFinder.GetPluginDescriptors<BasePlugin>(false)
                    .FirstOrDefault(x => x.SystemName.Equals(SystemName, StringComparison.InvariantCultureIgnoreCase));
            if (pluginDescriptor != null && !pluginDescriptor.Installed)
            {
                pluginDescriptor.Instance().Install();

                _app.RestartAppDomain();
            }

            return Redirect("~/Home/Index");
        }

        [HttpPost]
        public ActionResult Uninstall(string SystemName)
        {
            var pluginDescriptor = _pluginFinder.GetPluginDescriptors<BasePlugin>(false)
                    .FirstOrDefault(x => x.SystemName.Equals(SystemName, StringComparison.InvariantCultureIgnoreCase));

            if (pluginDescriptor != null && pluginDescriptor.Installed)
            {
                pluginDescriptor.Instance().Uninstall();
                _app.RestartAppDomain();
            }

            return Redirect("~/Home/Index");
        }
    }
}
