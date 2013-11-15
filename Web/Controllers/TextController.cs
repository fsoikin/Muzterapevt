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
		[Import] public BBCodeUI BbCode { get; set; }
		[Import] public AttachmentUI Attachments { get; set; }

		public JsonResponse<JS.TextEditor> Load( string id ) {
			return JsonResponse.Catch( () => {
				var p = Texts.Find( id );
				return JsonResponse.Create( new JS.TextEditor { Id = id, Text = p == null ? "" : p.BbText } );
			}, Log );
		}

		public JsonResponse<JS.TextSaveResult> Update( [JsonRequestBody] JS.TextEditor text ) {
			return JsonResponse.Catch( () => {
				var p = Texts.Find( text.Id );
				if ( p == null ) p = Texts.Add( new Data.Text { Id = text.Id } );

				p.BbText = text.Text;
				p.HtmlText = BbCode.ToHtml( p.BbText );
				UnitOfWork.Commit();

				return JsonResponse.Create( new JS.TextSaveResult { Html = p.HtmlText } );
			}, Log );
		}

		[Mixin]
		public AttachmentUI.Mixin Attachment( int textId ) {
			return Attachments.AsMixin( TextService.AttachmentDomain( textId ) );
		}
	}
}