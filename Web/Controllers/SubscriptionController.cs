using System;
using System.Linq;
using System.Reactive;
using System.Web.Mvc;
using erecruit.Composition;
using Mut.Data;
using Mut.Models;
using Mut.UI;
using Mut.Web;
using erecruit.Utils;
using erecruit.JS;

namespace Mut.Controllers
{
	public class SubscriptionController : Controller
	{
		[Import] public IRepository<Subscription> Subscriptions { get; set; }
		[Import] public EmailService EmailService { get; set; }
		[Import] public ISiteService Sites { get; set; }

		[Export]
		private static readonly IMarkdownCustomModule MarkdownTag_Link = MarkdownCustomModule.Create(
			"subscribe-link", new[] { "text" }, args => new ClassRef { Class = "Link", Module = "BL/widgets/subscription", Arguments = args.ValueOrDefault( "text" ) } );
		[Export]
		private static readonly IMarkdownCustomModule MarkdownTag_Page = MarkdownCustomModule.Create(
			"subscribe-form", new ClassRef { Class = "Page", Module = "BL/widgets/subscription" } );

		public IJsonResponse<Unit> Submit( [JsonRequestBody] SubscriptionSubmitModel model )
		{
			return (
				from r in model.MaybeDefined()
				
				from sb in
					Subscriptions.All.FirstOrDefault( s => s.Email == model.Email ) ??
					Subscriptions.Add( new Subscription { Email = model.Email, Verified = false } )
				where sb.Verified == false
				from setToken in sb.VerificationToken = Guid.NewGuid()

				from commit in Maybe.Do( UnitOfWork.Commit )
				
				from send in Maybe.Do( () => EmailService.SendEmail( new Email {
					ToEmail = model.Email,
					Subject = Sites.CurrentSiteFriendlyName + ": почтовая рассылка",
					Body = string.Format( _emailBody, Sites.CurrentSiteFriendlyName, new Uri( new Uri( Request.Url.AbsoluteUri ), Url.Action( ( SubscriptionController c ) => c.Verify( sb.VerificationToken.ToString() ) ) ) )
				} ) )

				select Unit.Default
				)
				.LogErrors( Log.Error )
				.AsJsonResponse();
		}

		public ActionResult Verify( string id ) {
			try {
				var guid = new Guid( id );
				var sb = Subscriptions.All.FirstOrDefault( s => s.VerificationToken == guid );
				if ( sb != null && sb.Verified == false ) {
					sb.Verified = true;
					UnitOfWork.Commit();
					return View();
				}
			}
			catch ( Exception ex ) {
				Log.Error( ex );
			}

			return Redirect( Request.ApplicationPath );
		}

		const string _emailBody = @"
			<h2>Спасибо, что подписались на почтовую рассылку сайта {0}.</h2>
			Для того, чтобы Ваша рассылка вступила в силу, мы должны удостовериться, что этот электронный адрес действительно принадлежит Вам.
			Чтобы это сделать, пожалуйста нажмите на эту ссылку:<br/><br/>
			&nbsp;&nbsp;&nbsp;&nbsp;<a href=""{1}"">{1}</a><br/><br/>
			Если Ваш почтовый клиент не позволяет нажимать на ссылки в тексте письма, просто скопируйте эту ссылку и вставьте её в адресную строку Вашего браузера.<br/><br/>
      С уважением, команда сайта {0}.
		";
	}
}