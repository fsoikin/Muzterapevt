using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using erecruit.Composition;
using erecruit.Utils;

namespace Mut.Controllers
{
	public class ThemeController : Controller
	{
		[Import] public ISiteService Sites { get; set; }

		public ActionResult File( string url )
		{
			return
				new[] { "~/__Themes/" + (Sites.CurrentSiteTheme.NullOrEmpty() ? "_" : Sites.CurrentSiteTheme), "~/Content" }
				.Select( Server.MapPath )
				.Select( p => Path.Combine( p, url ) )
				.Where( System.IO.File.Exists )
				.Select( path => new Result( path ) as ActionResult )
				.DefaultIfEmpty( () => HttpNotFound() )
				.First();
		}

		static string GetContentType( string path ) {
			var ext = System.IO.Path.GetExtension( path );
			return _knownContentTypes.ValueOrDefault( ext ) ??
				_contentTypes.GetOrAdd( ext, e => {
					using ( var r = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey( ext ) ) {
						if ( r != null ) return Convert.ToString( r.GetValue( "Content Type" ) );
					}
					return "application/octet-stream";
				} );
		}

		static IDictionary<string, string> _knownContentTypes = new Dictionary<string, string> {
			{ ".css", "text/css" },
			{ ".js", "text/javascript" },
			{ ".html", "text/html" },
			{ ".png", "image/png" },
			{ ".jpg", "image/jpeg" },
			{ ".gif", "image/gif" },
		};
		static ConcurrentDictionary<string, string> _contentTypes = new ConcurrentDictionary<string, string>();

		class Result : ActionResult
		{
			readonly string _fileName;
			public Result( string fileName ) {
				_fileName = fileName;
			}

			public override void ExecuteResult( ControllerContext context ) {
				var resp = context.HttpContext.Response;
				resp.ContentType = GetContentType( Path.GetExtension( _fileName ) );
				resp.Cache.SetLastModified( System.IO.File.GetLastWriteTime( _fileName ) );
				resp.Cache.SetMaxAge( TimeSpan.FromSeconds( Properties.Settings.Default.StaticContentMaxAgeSeconds ) );
				using ( var s = System.IO.File.OpenRead( _fileName ) ) {
					s.CopyTo( resp.OutputStream );
				}
			}
		}
	}
}