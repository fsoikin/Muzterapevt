using System.Linq;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using erecruit.Composition;
using Mut.Data;
using Mut.Models;
using Mut.UI;
using Mut.Web;
using Name.Files;
using erecruit.Mvc;

namespace Mut.Controllers
{
	[EditPermission]
	public class TextController : Controller
	{
		[Import] public IRepository<Text> Texts { get; set; }
		[Import] public IAuthService Auth { get; set; }
		[Import] public MarkupUI Markup { get; set; }
		[Import] public AttachmentUI Attachments { get; set; }

		public JsonResponse<JS.TextEditor> Load( string id ) {
			return JsonResponse.Catch( () => {
				var p = Texts.Find( id );
				return JsonResponse.Create( new JS.TextEditor { Id = id, Text = p == null ? "" : p.BbText } );
			}, Log );
		}

		public JsonResponse<JS.TextView> LoadHtml( string id ) {
			return JsonResponse.Catch( () => {
				var p = Texts.Find( id );
				return new JS.TextView { Text = p == null ? "" : p.HtmlText, AllowEdit = Auth.CurrentActor.IsAdmin };
			}, Log );
		}

		[HttpPost]
		public JsonResponse<JS.TextSaveResult> Update( [JsonRequestBody] JS.TextEditor text ) {
			return JsonResponse.Catch( () => {
				var p = Texts.Find( text.Id );
				if ( p == null ) p = Texts.Add( new Data.Text { Id = text.Id } );

				p.BbText = text.Text;
				p.HtmlText = Markup.ToHtml( p.BbText ?? "", new MarkupParseArgs { AttachmentMixin = Url.Mixin( ( TextController c ) => c.Attachment( text.Id ) ) } );
				UnitOfWork.Commit();

				return JsonResponse.Create( new JS.TextSaveResult { Html = p.HtmlText } );
			}, Log );
		}

		[Mixin]
		public AttachmentUI.Mixin Attachment( string textId ) {
			return Attachments.AsMixin( TextService.AttachmentDomain( textId ) );
		}
	}
}