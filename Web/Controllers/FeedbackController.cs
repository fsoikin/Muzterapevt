using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using erecruit.Composition;
using erecruit.JS;
using erecruit.Mvc;
using erecruit.Utils;
using Mut.Data;
using Mut.Models;
using Mut.UI;
using Mut.Web;

namespace Mut.Controllers
{
	public class FeedbackController : Controller
	{
		[Import] public EmailService Emails { get; set; }
		[Import] public ISiteService Sites { get; set; }

		[Export]
		private static readonly IMarkdownCustomModule MarkdownTag = MarkdownCustomModule.Create(
			"feedback-form", new[] { "simple", "lang", "toEmail" },
				args => new ClassRef {
					Class = "FormVm", Module = "BL/feedback/questionForm",
					Arguments = new { 
						simplified = !args.ValueOrDefault( "simple" ).NullOrEmpty(), 
						lang = args.ValueOrDefault( "lang" ),
						toEmail = ProtectToEmail( args.ValueOrDefault( "toEmail" ) ) }
				} );

		static string ProtectToEmail( string value ) {
			if ( value.NullOrEmpty() ) return null;
			return Convert.ToBase64String( MachineKey.Protect( Encoding.UTF8.GetBytes( value ) ) );
		}

		static string UnprotectToEmail( string value ) {
			if ( value.NullOrEmpty() ) return null;
			return Encoding.UTF8.GetString( MachineKey.Unprotect( Convert.FromBase64String( value ) ) );
		}

		[HttpPost]
		public IJsonResponse<unit> Question( [JsonRequestBody] JS.FeedbackQuestion q ) {
			return (
				from req in q.MaybeDefined()
				
				from toEmail in 
					UnprotectToEmail( q.toEmail )
					.MaybeDefined()
					.Or( () => "feedback@muzterapevt.ru" )

				from _ in Maybe.Do( () => Emails.SendEmail( new Email {
					FromName = req.name, FromEmail = req.email, Subject = req.subject,
					ToEmail = toEmail,
					Body = string.Format( @"<h2>Вопрос с {4}</h2><hr>
						<br><b>Моё имя</b>: {0}
						<br><b>Мой e-mail</b>: {1}
						<br><b>Заголовок</b>: {2}
						<br><b>Вопрос</b>:<br>{3}",
						req.name, req.email, req.subject, HttpUtility.HtmlEncode( req.text ), Sites.CurrentSiteFriendlyName )
				} ) )
				select unit.Default
				)
				.LogErrors( Log.Error )
				.AsJsonResponse();
		}
	}
}