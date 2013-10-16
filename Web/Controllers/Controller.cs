using System;
using System.Linq;
using System.Web.Mvc;
using erecruit.Composition;
using log4net;
using Mut.Data;
using Mut.Models;
using Mut.UI;

namespace Mut.Controllers
{
	public class Controller : System.Web.Mvc.Controller
	{
		[Import] public ILog Log { get; set; }
		[Import] public ITopMenuUI TopMenu { get; set; }
		[Import] public TextUI Text { get; set; }

		protected override void OnException( ExceptionContext filterContext ) {
			this.Log.Error( filterContext.Exception );
			base.OnException( filterContext );
		}

		protected override void OnActionExecuting( ActionExecutingContext filterContext ) {
			Log.DebugFormat( "START: {0}", filterContext.ActionDescriptor.ActionName );
			
			ViewBag.LayoutModel = new LayoutModel {
				TopMenu = TopMenu.GetTopMenu( "TopMenu.First" ),
				SecondTopMenu = TopMenu.GetTopMenu( "TopMenu.Second" ),
				Left = Text.TextModel( "Layout.Left" ),
				Right = Text.TextModel( "Layout.Right" ),
				TopRight = Text.TextModel( "Layout.TopRight" )
			};

			base.OnActionExecuting( filterContext );
		}

		protected override void OnActionExecuted( ActionExecutedContext filterContext ) {
			Log.DebugFormat( "END:   {0}, result = {1}", filterContext.ActionDescriptor.ActionName, filterContext.Result );
			base.OnActionExecuted( filterContext );
		}
	}
}