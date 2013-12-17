using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using erecruit.Composition;
using erecruit.Mvc;
using erecruit.Utils;
using Mut.Data;
using Mut.Models;
using Mut.UI;
using Mut.Web;
using Name.Files;

namespace Mut.Controllers
{
	public class PageController : Controller
	{
		[Import] public IRepository<Page> Pages { get; set; }
		[Import] public PagesService PagesService { get; set; }
		[Import] public IAuthService Auth { get; set; }
		[Import] public MarkupUI Markup { get; set; }
		[Import] public AttachmentUI Attachments { get; set; }

		public ActionResult Page( string url )
		{
			var p = PagesService.GetPage( url, Auth.CurrentActor.IsAdmin );
			if ( p == null ) return RedirectToAction( "Page", new { url = "" } );
			else return View( "~/Views/Page.cshtml", new PageModel { 
				Page = p, 
				AllowEdit = Auth.CurrentActor.IsAdmin, 
				ChildPages = GetChildren( p )
			} );
		}

		private IEnumerable<PageModel> GetChildren( Data.Page p ) {
			var firstLevel = new[] { p }.ToList();
			var levels = EnumerableEx.Generate( new { pages = firstLevel, depth = 4 }, x => x.depth >= 0 && x.pages.Any(), 
				x => new { pages = PagesService.GetChildPages( x.pages ).ToList(), depth = x.depth - 1 },
				x => x.pages )
				.ToList();
			return MergeChildPages( p, levels.Skip( 1 ) );
		}

		private IEnumerable<PageModel> MergeChildPages( Data.Page parent, IEnumerable<List<Data.Page>> levels ) {
			var prefix = parent.Url + "/";
			return from c in levels.FirstOrDefault().EmptyIfNull()
						 where c.Url.StartsWith( prefix )
						 select new PageModel {
							 Page = c,
							 ChildPages = MergeChildPages( c, levels.Skip( 1 ) )
						 };
		}

		[EditPermission]
		public JsonResponse<JS.PageEditor> Load( int id ) {
			return JsonResponse.Catch( () => {
				var p = Pages.Find( id );
				if ( p == null ) return JsonResponse<JS.PageEditor>.NotFound;

				return JsonResponse.Create( new JS.PageEditor {
					Id = id, Path = p.Url, Text = p.BbText ?? "", Title = p.Title ?? "",
					TagsStandIn = p.TagsStandIn, ReferenceName = p.ReferenceName ?? ""
				} );
			}, Log );
		}

		[EditPermission, HttpPost]
		public JsonResponse<JS.PageSaveResult> Update( [JsonRequestBody] JS.PageEditor page ) {
			return JsonResponse.Catch( () => {
				var p = Pages.Find( page.Id );
				if ( p == null ) return JsonResponse<JS.PageSaveResult>.NotFound;

				p.Title = page.Title ?? "";
				p.BbText = page.Text;
				p.TagsStandIn = page.TagsStandIn;
				p.ReferenceName = page.ReferenceName;
				p.HtmlText = Markup.ToHtml( p.BbText ?? "", new MarkupParseArgs { AttachmentMixin = Url.Mixin( (PageController c) => c.Attachment( page.Id ) ) } );
				UnitOfWork.Commit();

				return JsonResponse.Create( new JS.PageSaveResult { Title = p.Title, Html = p.HtmlText } );
			}, Log );
		}

		[Mixin]
		public AttachmentUI.Mixin Attachment( int pageId ) {
			return Attachments.AsMixin( PagesService.AttachmentDomain( pageId ) );
		}
	}
}