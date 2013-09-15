using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using erecruit.Composition;
using Mut.Data;

namespace Mut
{
	public class MvcApplication : System.Web.HttpApplication
	{
		protected void Application_Start()
		{
			AreaRegistration.RegisterAllAreas();
			log4net.Config.XmlConfigurator.Configure();

			FilterConfig.RegisterGlobalFilters( GlobalFilters.Filters );
			RouteConfig.RegisterRoutes( RouteTable.Routes );
		}

		[Export]
		class DbConfig : IDatabaseConfig
		{
			public string ConnectionString { get { return "Mut"; } }
		}
	}
}