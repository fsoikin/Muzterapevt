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
			routes.MapRoute( "Default", "-/{controller}/{action}", new { controller = "BackOffice", action = "Index" } );

			routes.MapRoute( "PageAttachment", "--/page/{pageId}/{action}/{*path}", new { controller = "Page" }, new { action = "^Attachment.*" } );
			routes.MapRoute( "TextAttachment", "--/text/{textId}/{action}/{*path}", new { controller = "Text" }, new { action = "^Attachment.*" } );

			routes.MapRoute( "Page", "{*url}", new { controller = "Page", action = "Page", url = UrlParameter.Optional } );
		}
	}
}