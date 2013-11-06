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

			routes.MapRoute( "File", "--/file/{*path}", new { controller = "File", action = "Serve" } );
			routes.MapRoute( "PageAttachment", "--/page/{pageId}/{*path}", new { controller = "File", action = "PageAttachment" } );
			routes.MapRoute( "TextAttachment", "--/text/{textId}/{*path}", new { controller = "File", action = "TextAttachment" } );

			routes.MapRoute( "Picture", "--/img/{action}/{width}/{height}/{*path}", new { controller = "Picture" }, new {  action = "Stretch|Crop"} );
			routes.MapRoute( "PictureScaleW", "--/img/scalew/{width}/{*path}", new { controller = "Picture", action = "ScaleW" } );
			routes.MapRoute( "PictureScaleH", "--/img/scaleh/{height}/{*path}", new { controller = "Picture", action = "ScaleH" } );

			routes.MapRoute( "Page", "{*url}", new { controller = "Page", action = "Page", url = UrlParameter.Optional } );
		}
	}
}