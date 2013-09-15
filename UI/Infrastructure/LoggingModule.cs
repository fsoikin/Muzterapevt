using System.Web;
using erecruit.Composition;
using log4net;

namespace Mut.UI
{
	[Export]
	public class LoggingModule : IHttpModule
	{
		[Import] public ILog Log { get; set; }

		public void Init( HttpApplication context )
		{
			context.Error += ( _, __ ) => Log.Error( context.Server.GetLastError() );
			context.BeginRequest += ( _, __ ) => Log.DebugFormat( "HTTP {0} start: {1}", context.Request.HttpMethod, context.Request.Url );
			context.EndRequest += ( _, __ ) => Log.DebugFormat( "HTTP {0} end: {1}", context.Request.HttpMethod, context.Request.Url );
		}

		public void Dispose() { }
	}
}