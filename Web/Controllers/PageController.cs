using System;
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
		[Import] public PageUI PageUI { get; set; }
		[Import] public MarkdownUI Markup { get; set; }
		[Import] public AttachmentUI Attachments { get; set; }

		public ActionResult Page( string url )
		{
			return PageUI.PageModel( url )
				.Select( model => View( "~/Views/Page.cshtml", model ) as ActionResult )
				.Or( () => Redirect( Url.Action( ( PageController c ) => c.Page( "" ) ) ) )
				.LogErrors( Log.Error )
				.Value;
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
				p.HtmlText = Markup.ToHtml( p.BbText ?? "", new MarkdownParseArgs { AttachmentMixin = Url.Mixin( (PageController c) => c.Attachment( page.Id ) ) } );
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