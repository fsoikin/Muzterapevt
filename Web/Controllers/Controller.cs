using System;
using System.Linq;
using System.Web.Mvc;
using erecruit.Composition;
using erecruit.Mvc.Mixins;
using log4net;
using Mut.Data;
using Mut.Models;
using Mut.UI;

namespace Mut.Controllers
{
	public class Controller : System.Web.Mvc.Controller
	{
		[Import]
		public IAuthService Auth { get; set; }
		[Import]
		public ILog Log { get; set; }
		[Import]
		public ITopMenuUI TopMenu { get; set; }
		[Import]
		public TextUI Text { get; set; }
		[Import]
		public IUnitOfWork UnitOfWork { get; set; }

		protected override void OnException( ExceptionContext filterContext ) {
			this.Log.Error( filterContext.Exception );
			base.OnException( filterContext );
		}

		protected override void OnActionExecuting( ActionExecutingContext filterContext ) {
			Log.DebugFormat( "START: {0}", filterContext.ActionDescriptor.ActionName );

			ViewBag.LayoutModel = new LayoutModel {
				ShowAdminMenu = Auth.CurrentActor.IsAdmin,
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


		protected override IActionInvoker CreateActionInvoker() {
			return new MixinControllerActionInvoker();
		}

		public class MixinControllerActionInvoker : ControllerActionInvoker
		{
			protected override ControllerDescriptor GetControllerDescriptor( ControllerContext controllerContext ) {
				return new AggregateControllerDescriptor( this.GetType(),
						MixinControllerDescriptor.GetFromMethods( controllerContext.Controller.GetType() ).Concat(
						new[] { base.GetControllerDescriptor( controllerContext ) }
					) );
			}
		}
	}
}