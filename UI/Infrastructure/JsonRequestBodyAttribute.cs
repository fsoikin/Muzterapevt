using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace Mut.UI
{
	[AttributeUsage( AttributeTargets.Parameter )]
	public class JsonRequestBodyAttribute : CustomModelBinderAttribute, IModelBinder
	{
		public object BindModel( ControllerContext cctx, ModelBindingContext bctx )
		{
			if ( cctx == null ) throw new ArgumentNullException( "controllerContext" );
			var req = cctx.HttpContext.Request;

			string content;
			if ( string.Equals( req.HttpMethod, "POST", StringComparison.InvariantCultureIgnoreCase ) )
			{
				if ( !req.ContentType.StartsWith( "application/json", StringComparison.OrdinalIgnoreCase ) ) return null;

				var str = cctx.HttpContext.Request.InputStream;
				str.Seek( 0, SeekOrigin.Begin );
				content = new StreamReader( str ).ReadToEnd();
			}
			else
			{
				content = HttpUtility.UrlDecode( req.QueryString.ToString() );
			}

			if ( String.IsNullOrEmpty( content ) ) return null;
			return JsonConvert.DeserializeObject( content, bctx.ModelType );
		}

		public override IModelBinder GetBinder()
		{
			return this;
		}
	}
}