using System;
using System.Web;
using System.Web.Mvc;
using erecruit.Composition;
using log4net;

namespace Mut.UI
{
	public class EditPermissionAttribute : ActionFilterAttribute
	{
		public override void OnActionExecuting( ActionExecutingContext filterContext ) {
			if ( filterContext.HttpContext.Composition().Get<IAuthService>().CurrentActor.IsAdmin == false ) {
				filterContext.Result = new RedirectResult( "/" );
			}
			else {
				base.OnActionExecuting( filterContext );
			}
		}
	}
}