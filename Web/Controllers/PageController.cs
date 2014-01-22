using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
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
		[Import] public PageUI PageUI { get; set; }
		[Import] public PagesService PageService { get; set; }
		[Import] public MarkdownUI Markup { get; set; }
		[Import] public AttachmentUI Attachments { get; set; }
		[Import] public ISiteService Sites { get; set; }

		public ActionResult Page( string url )
		{
			return PageUI.PageModel( url )
				.Select( model => View( "~/Views/Page.cshtml", model ) as ActionResult )
				.Or( () => url.NullOrEmpty() ? EmptyView() : Redirect( "~/" ) )
				.LogErrors( Log.Error )
				.Value;
		}

		private ActionResult EmptyView() {
			return View( "~/Views/Page.cshtml", new PageModel { AllowEdit = false, Page = new Page(), TopParent = new Page(), ChildPages = new PageModel[0] } );
		}

		[EditPermission]
		public JsonResponse<JS.PageEditor> Load( int id ) {
			return JsonResponse.Catch( () => {
				var p = Pages.All.FirstOrDefault( x => x.Id == id && x.SiteId == Sites.CurrentSiteId );
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
				var p = Pages.All.FirstOrDefault( x => x.Id == page.Id && x.SiteId == Sites.CurrentSiteId );
				if ( p == null ) return JsonResponse<JS.PageSaveResult>.NotFound;

				p.Title = page.Title ?? "";
				p.BbText = page.Text;
				p.TagsStandIn = page.TagsStandIn;
				p.ReferenceName = page.ReferenceName;
				p.HtmlText = Markup.ToHtml( p.BbText ?? "", new MarkdownParseArgs { AttachmentMixin = Url.Mixin( (PageController c) => c.Attachment( page.Id ) ) } );
				UnitOfWork.Commit();

				return JsonResponse.Create( new JS.PageSaveResult { Title = p.Title, Html = p.HtmlText } );
			}, Log );
		}

		[EditPermission, HttpPost]
		public IJsonResponse<unit> Reorder( [JsonRequestBody] JS.PageReorderRequest req ) {
			return (from r in req.MaybeDefined()
							from parent in Pages.All.FirstOrDefault( x => x.Id == r.ParentId && x.SiteId == Sites.CurrentSiteId ).MaybeDefined()
							from children in PageService.GetChildPages( new[] { parent } ).ToList()

							from reordered in
								from c in children
								join o in req.Children.Select( ( id, idx ) => new { id, idx } ) on c.Id equals o.id
								orderby o.idx
								select c

							from result in reordered.Concat( children.Except( req.Children, p => p.Id ) )
								 .Select( ( p, idx ) => p.SortOrder = idx )
								 .LastOrDefault()

							from _ in Maybe.Do( UnitOfWork.Commit )

							select unit.Default
						 )
						 .LogErrors( Log.Error )
						 .AsJsonResponse();
		}

		[Mixin]
		public AttachmentUI.Mixin Attachment( int pageId ) {
			return Attachments.AsMixin( PagesService.AttachmentDomain( pageId ) );
		}
	}
}