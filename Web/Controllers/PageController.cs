using System;
using System.Linq;
using System.Web.Mvc;
using erecruit.Composition;
using Mut.Data;
using Mut.Models;
using Mut.UI;

namespace Mut.Controllers
{
	public class PageController : Controller
	{
		[Import] public IRepository<Page> Pages { get; set; }
		[Import] public IAuthService Auth { get; set; }

		public ActionResult Page( string url )
		{
			var p = Pages.All.FirstOrDefault( x => x.Url == url );
			if ( p == null ) return HttpNotFound();

			return View( "~/Views/Page.cshtml", new PageModel { Page = p, AllowEdit = true
				//Auth.CurrentActor.IsAdmin 
			} );
		}
	}
}