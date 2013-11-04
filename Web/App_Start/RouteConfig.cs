using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Mut
{
	public class RouteConfig
	{
		public static void RegisterRoutes( RouteCollection routes )
		{
			routes.IgnoreRoute( "{resource}.axd/{*pathInfo}" );

			routes.MapRoute(
					name: "Default",
					url: "-/{controller}/{action}",
					defaults: new { controller = "BackOffice", action = "Index" }
			);

			routes.MapRoute(
					name: "File",
					url: "file/{*path}",
					defaults: new { controller = "File", action = "Serve", path = UrlParameter.Optional }
			);

			routes.MapRoute( "Picture", "img/{action}/{width}/{height}/{*path}", new { controller = "Picture" }, new {  action = "Stretch|Crop"} );
			routes.MapRoute( "PictureScaleW", "img/scalew/{width}/{*path}", new { controller = "Picture", action = "ScaleW" } );
			routes.MapRoute( "PictureScaleH", "img/scaleh/{height}/{*path}", new { controller = "Picture", action = "ScaleH" } );

			routes.MapRoute(
					name: "Page",
					url: "{*url}",
					defaults: new { controller = "Page", action = "Page", url = UrlParameter.Optional }
			);
		}
	}
}