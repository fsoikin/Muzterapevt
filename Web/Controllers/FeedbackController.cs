using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
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
			"feedback-form", new[] { "simple" },
			args => new ClassRef {
				Class = "FormVm", Module = "BL/feedback/questionForm",
				Arguments = new { simplified = !args.ValueOrDefault( "simple" ).NullOrEmpty() }
			} );

		[HttpPost]
		public IJsonResponse<unit> Question( [JsonRequestBody] JS.FeedbackQuestion q ) {
			return (from req in q.MaybeDefined()
							from _ in Maybe.Do( () => Emails.SendEmail( new Email {
								FromName = req.Name, FromEmail = req.Email, Subject = req.Subject,
								ToEmail = "feedback@muzterapevt.ru",
								Body = string.Format( @"<h2>Вопрос с {4}</h2><hr>
									<br><b>Моё имя</b>: {0}
									<br><b>Мой e-mail</b>: {1}
									<br><b>Заголовок</b>: {2}
									<br><b>Вопрос</b>:<br>{3}",
									req.Name, req.Email, req.Subject, HttpUtility.HtmlEncode( req.Text ), Sites.CurrentSiteFriendlyName )
							} ) )
							select unit.Default
						 )
						 .LogErrors( Log.Error )
						 .AsJsonResponse();
		}
	}
}