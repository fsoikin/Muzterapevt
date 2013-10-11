using System;
using System.Linq;
using System.Reactive;
using System.Web.Mvc;
using erecruit.Composition;
using Mut.Data;
using Mut.Models;
using Mut.UI;
using Mut.Web;

namespace Mut.Controllers
{
	public class PageController : Controller
	{
		[Import] public IRepository<Page> Pages { get; set; }
		[Import] public PagesService PagesService { get; set; }
		[Import] public IAuthService Auth { get; set; }
		[Import] public BBCodeUI BbCode { get; set; }
		[Import] public IUnitOfWork UnitOfWork { get; set; }
		[Import] public TopMenuUI TopMenu { get; set; }

		public ActionResult Page( string url )
		{
			var p = PagesService.GetPage( url, Auth.CurrentActor.IsAdmin );
			if ( p == null ) return RedirectToAction( "Page", new { url = "" } );
			else return View( "~/Views/Page.cshtml", new PageModel { 
				Page = p, 
				AllowEdit = Auth.CurrentActor.IsAdmin, 
				TopMenu = TopMenu.GetTopMenu(),
				ChildPages = PagesService.GetChildPages( p )
			} );
		}

		public JsonResponse<JS.PageEditor> Load( int id ) {
			return JsonResponse.Catch( () => {
				var p = Pages.Find( id );
				if ( p == null ) return JsonResponse<JS.PageEditor>.NotFound;

				return JsonResponse.Create( new JS.PageEditor {
					Id = id, Path = p.Url, Text = p.BbText, Title = p.Title
				} );
			}, Log );
		}

		public JsonResponse<string> Update( JS.PageEditor page ) {
			return JsonResponse.Catch( () => {
				var p = Pages.Find( page.Id );
				if ( p == null ) return JsonResponse<string>.NotFound;

				p.Title = page.Title;
				p.BbText = page.Text;
				p.HtmlText = BbCode.ToHtml( p.BbText );
				UnitOfWork.Commit();

				return JsonResponse.Create( p.HtmlText );
			}, Log );
		}
	}
}