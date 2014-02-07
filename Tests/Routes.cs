using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;
using log4net;
using Moq;
using Mut.Data;
using Xunit;
using Xunit.Extensions;

namespace Mut.Tests
{
	public class Routes
	{
		[Theory]
		[InlineData( "a" )]
		[InlineData( "a/b" )]
		[InlineData( "a/b/c" )]
		public void ThemeFile( string url ) {
			var rt = new RouteCollection();
			RouteConfig.RegisterRoutes( rt );

			var d = rt.GetRouteData( MockCtx( "Content/" + url ) );
			Assert.Equal( "Theme", d.Values["controller"] );
			Assert.Equal( "File", d.Values["action"] );
		}

		const string AppPath = "http://a.b.com";

		HttpContextBase MockCtx( string url ) {
			var ctx = new Mock<HttpContextBase> { CallBase = true };
			var req = new Mock<HttpRequestBase> { CallBase = true };
			req.SetupGet( r => r.Url ).Returns( new Uri( AppPath + "/" + url ) );
			req.SetupGet( r => r.ApplicationPath ).Returns( AppPath );
			req.SetupGet( r => r.AppRelativeCurrentExecutionFilePath ).Returns( "~/" + url );
			req.SetupGet( r => r.PhysicalApplicationPath ).Returns( "c:\\" );
			req.SetupGet( r => r.PathInfo ).Returns( "" );
			ctx.SetupGet( r => r.Request ).Returns( req.Object );

			return ctx.Object;
		}
	}
}