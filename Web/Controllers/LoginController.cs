using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OAuth2;
using DotNetOpenAuth.OpenId;
using DotNetOpenAuth.OpenId.RelyingParty;
using erecruit.Composition;
using erecruit.Utils;
using Mut.Data;
using Mut.Models;
using Mut.UI;
using Mut.Web;

namespace Mut.Controllers
{
	public class LoginController : Controller
	{
		[Import] public LoginService Login { get; set; }

		public IJsonResponse<IEnumerable<LoginMethodModel>> GetMethods()
		{
			return Maybe.Eval( () => Login.GetMethods().Select( i => new LoginMethodModel {
				Id = i.Id.ToString(),
				Title = i.Title, LogoClass = i.LogoClass
			} ) )
			.LogErrors( Log.Error )
			.AsJsonResponse();
		}

		public IJsonResponse<string> GetRedirectUrlForOpenId( string id, string returnUrl ) {
			var res = from _ in id.MaybeDefined().OrFail( "Empty identifier" )
								from i in Identifier.Parse( id )
								from url in Login.GetRedirectUrlForOpenId( id, Url.Action( ( LoginController c ) => c.Landing( returnUrl ) ) )
								select url;
			return res.LogErrors( Log.Error ).AsJsonResponse();
		}

		public IJsonResponse<string> GetRedirectUrl( string method, string returnUrl ) {
			var res = from m in Login.GetMethods()
									.FirstOrDefault( x => x.Id == method ).MaybeDefined()
									.OrFail( "Unknown method" )
								from url in Login.GetRedirectUrlFor( m, Url.Action( ( LoginController c ) => c.Landing( returnUrl ) ) )
								select url;
			return res.LogErrors( Log.Error ).AsJsonResponse();
		}

		public RedirectResult Landing( string returnUrl ) {
			try {
				Auth.SetCurrentActor( Login.GetAuthenticatedUser().Value, true );
			}
			catch ( Exception ex ) {
				Log.Error( ex );
			}

			return Redirect( returnUrl );
		}

		public RedirectResult Logout( string returnUrl ) {
			Auth.SetCurrentActor( null, true );
			return Redirect( returnUrl );
		}
	}

	public interface ILoginMethod
	{
		string Id { get; }
		string Title { get; }
		string LogoClass { get; }
		OutgoingWebResponse GetRedirectResponse( string returnUrl );
		string GetAuthenticatedUserUniqueId();
	}

	public class LoginService
	{
		readonly OpenIdRelyingParty _relyingParty = new OpenIdRelyingParty( OpenIdRelyingParty.HttpApplicationStore );
		readonly ILoginMethod[] _methods;
		[Import] public Func<HttpContextBase> HttpContext { get; set; }
		[Import] public Func<IRepository<User>> Users { get; set; }
		[Import] public Func<IUnitOfWork> UnitOfWork { get; set; }
		[Import] public ITransactionService Tran { get; set; }

		public LoginService() {
			_methods = new ILoginMethod[] {
				new OpenIdMethod( "Google", "google", this, "http://www.google.com/accounts/o8/id" ),
				new OAuthMethod( "Facebook", "facebook", this, 
					"https://www.facebook.com/dialog/oauth", "https://graph.facebook.com/oauth/access_token", 
					"520877634665092", "3f6dcede52ed12a70aaebbdba38d9d2a", "email", "https://graph.facebook.com/me?access_token={0}" )
			};
		}

		public Maybe<string> GetRedirectUrlForOpenId( string id, string returnUrl ) {
			return GetRedirectUrlFor( new OpenIdMethod( null, null, this, id ), returnUrl );
		}

		public Maybe<string> GetRedirectUrlFor( ILoginMethod method, string returnUrl ) {
			return from resp in method.GetRedirectResponse( returnUrl ).MaybeDefined()
						 from url in resp.Headers[System.Net.HttpResponseHeader.Location]
						 select url;
		}

		public Maybe<User> GetAuthenticatedUser() {
			return from userId in _methods.Select( m => m.GetAuthenticatedUserUniqueId() ).Where( id => !id.NullOrEmpty() ).FirstOrDefault().MaybeDefined()
						 from user in Maybe.Eval( () => {
							 using ( Tran.OpenTransaction() ) {
								 var u = Users().All.FirstOrDefault( x => x.UniqueId == userId )
									 ?? Users().Add( new User { UniqueId = userId } );
								 UnitOfWork().Commit();
								 return u;
							 }
						 } )
						 select user;
		}

		public IEnumerable<ILoginMethod> GetMethods() { return _methods; }

		Uri Realm() {
			var url = HttpContext().Request.Url;
			var host = url.Host;
			if ( host.StartsWith( "www.", StringComparison.InvariantCultureIgnoreCase ) ) host = host.Substring( "www.".Length );
			return new Uri( url.Scheme + "://" + host );
		}

		class OpenIdMethod : ILoginMethod
		{
			readonly Identifier _id;
			readonly LoginService _srv;
			public OpenIdMethod( string title, string logoClass, LoginService srv, string id ) {
				_id = Identifier.Parse( id );
				Id = title;
				Title = title;
				LogoClass = logoClass;
				_srv = srv;
			}

			public string Id { get; private set; }
			public string Title { get; private set; }
			public string LogoClass { get; private set; }

			public OutgoingWebResponse GetRedirectResponse( string returnUrl ) {
				return _srv._relyingParty
				.CreateRequest( _id, new Realm( _srv.Realm() ), new Uri( new Uri( _srv.HttpContext().Request.Url.GetLeftPart(UriPartial.Authority) ), returnUrl ) )
				.RedirectingResponse;
			}

			public string GetAuthenticatedUserUniqueId() {
				var rsp = _srv._relyingParty.GetResponse();
				if ( rsp != null && rsp.Status == AuthenticationStatus.Authenticated ) {
					return rsp.ClaimedIdentifier.ToString();
				}
				return null;
			}
		}

		class OAuthMethod : ILoginMethod
		{
			readonly WebServerClient _client;
			readonly string _emailScope;
			readonly string _requestUrl;
			readonly LoginService _srv;
			public OAuthMethod( string title, string logoClass,
				LoginService srv, string authEndpoint, string tokenEndpoint, string clientId, string clientSecret,
				string emailScope, string requestUrl ) {
				_client = new WebServerClient( new AuthorizationServerDescription {
					AuthorizationEndpoint = new Uri( authEndpoint ),
					TokenEndpoint = new Uri( tokenEndpoint )
				}, clientId, clientSecret );
				Id = title;
				_srv = srv;
				_requestUrl = requestUrl;
				_emailScope = emailScope;
				Title = title;
				LogoClass = logoClass;
			}

			public string Id { get; private set; }
			public string Title { get; private set; }
			public string LogoClass { get; private set; }

			public OutgoingWebResponse GetRedirectResponse( string returnUrl ) {
				return _client.PrepareRequestUserAuthorization( new[] { _emailScope }, new Uri( returnUrl ) );
			}

			public string GetAuthenticatedUserUniqueId() {
				var st = _client.ProcessUserAuthorization( _srv.HttpContext().Request );
				if ( st == null || st.AccessToken.NullOrEmpty() ) return null;

				var res = new WebClient().DownloadString( string.Format( _requestUrl, st.AccessToken ) );
				return null;
			}
		}
	}
}