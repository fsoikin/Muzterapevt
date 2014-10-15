using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using DotNetOpenAuth.OAuth2;
using DotNetOpenAuth.OpenId;
using DotNetOpenAuth.OpenId.RelyingParty;
using erecruit.Composition;
using erecruit.Utils;
using Mut.Data;
using Mut.Models;
using Mut.Web;
using Newtonsoft.Json.Linq;

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
			var res =
				from _ in id.MaybeDefined().OrFail( "Empty identifier" )
				from i in Identifier.Parse( id )
				from _1 in Session[_sessionReturnUrlKey] = returnUrl
				from t_url in Login.GetRedirectUrlForOpenId( id, Url.Action( ( LoginController c ) => c.Landing() ) )
				from url in t_url.Result
				select url.ToString();

			return res.LogErrors( Log.Error ).AsJsonResponse();
		}

		public IJsonResponse<string> GetRedirectUrl( string method, string returnUrl ) {
			var t = new List<CookieHeaderValue>();
			var res = from m in Login.GetMethods()
									.FirstOrDefault( x => x.Id == method ).MaybeDefined()
									.OrFail( "Unknown method" )
								from _ in Session[_sessionReturnUrlKey] = returnUrl
								from _1 in Session[_sessionLoginMethodKey] = method
								from t_url in Login.GetRedirectUrlFor( m, Url.Action( ( LoginController c ) => c.Landing() ) )
								from url in t_url.Result
								select url.ToString();

			return res.LogErrors( Log.Error ).AsJsonResponse();
		}

		public async Task<ActionResult> Landing() {
			try {
				var methodId = Session[_sessionLoginMethodKey] as string;
				var method = Login.GetMethods().First( x => x.Id == methodId );
				Auth.SetCurrentActor( await Login.GetAuthenticatedUser( method ), true );
			}
			catch ( Exception ex ) {
				Log.Error( ex );
			}

			var returnUrl = (Session[_sessionReturnUrlKey] as string) ?? "~/";
			return Redirect( returnUrl );
		}

		const string _sessionReturnUrlKey = "{018542BB-81B1-4F96-8F36-A3648E4E8BE0}";
		const string _sessionLoginMethodKey = "{546DD728-0EC7-4D9B-B1FC-57146AEFA2CA}";

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
		Task<HttpResponseMessage> GetRedirectResponse( Uri returnUrl );
		Task<string> GetAuthenticatedUserUniqueId();
	}

	public class LoginService
	{
		readonly OpenIdRelyingParty _relyingParty = new OpenIdRelyingParty( OpenIdRelyingParty.GetHttpApplicationStore() );
		readonly ILoginMethod[] _methods;
		[Import] public Func<HttpContextBase> HttpContext { get; set; }
		[Import] public Func<IRepository<User>> Users { get; set; }
		[Import] public Func<IUnitOfWork> UnitOfWork { get; set; }
		[Import] public ITransactionService Tran { get; set; }

		public LoginService() {
			_methods = new ILoginMethod[] {
				new OAuthMethod( "Google", "google", this,
					"https://accounts.google.com/o/oauth2/auth", "https://accounts.google.com/o/oauth2/token",
					"363920244600-vcmc0qeoo1mnsp0p4edkc589mqpp3em7.apps.googleusercontent.com", 
					Properties.Settings.Default.GoogleClientSecret, "email", 
					"https://www.googleapis.com/plus/v1/people/me?access_token={0}", 
					j => {
						var emails = j["emails"] as JArray;
						if ( emails != null && emails.Any() ) {
							var e = emails.First()["value"];
							if ( e != null ) return e.ToString();
						}

						var url = j["url"];
						if ( url != null ) return url.ToString();

						return null;
					} ),

				new OAuthMethod( "Facebook", "facebook", this, 
					"https://www.facebook.com/dialog/oauth", "https://graph.facebook.com/oauth/access_token",
					"520877634665092", Properties.Settings.Default.FacebookClientSecret, "email", 
					"https://graph.facebook.com/me?access_token={0}",
					j => {
						var emails = j["email"];
						if ( emails != null ) return emails.ToString();

						var url = j["link"];
						if ( url != null ) return url.ToString();

						return null;
					})
			};
		}

		public Task<Uri> GetRedirectUrlForOpenId( string id, string returnUrl ) {
			return GetRedirectUrlFor( new OpenIdMethod( null, null, this, id ), returnUrl );
		}

		public async Task<Uri> GetRedirectUrlFor( ILoginMethod method, string returnUrl ) {
			var r = await method.GetRedirectResponse( new Uri( new Uri( HttpContext().Request.Url.GetLeftPart( UriPartial.Authority ) ), returnUrl ) );
			var ctx = HttpContext();

			( from h in r.Headers
				where h.Key == "Set-Cookie"
				from v in h.Value
				select new { h = h.Key, v } )
				.ForEach( x => ctx.Response.AppendHeader( x.h, x.v + "; path=" + ctx.Request.ApplicationPath ) );

			return r.Headers.Location;
		}

		public async Task<User> GetAuthenticatedUser( ILoginMethod method ) {
			var userId = await method.GetAuthenticatedUserUniqueId();
			using ( Tran.OpenTransaction() ) {
				var u = Users().All.FirstOrDefault( x => x.UniqueId == userId )
					?? Users().Add( new User { UniqueId = userId } );
				UnitOfWork().Commit();
				return u;
			}
		}

		public IEnumerable<ILoginMethod> GetMethods() { return _methods; }

		Uri Realm() {
			var url = HttpContext().Request.Url;
			var host = url.Host;
			//if ( host.StartsWith( "www.", StringComparison.InvariantCultureIgnoreCase ) ) host = host.Substring( "www.".Length );
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

			public async Task<HttpResponseMessage> GetRedirectResponse( Uri returnUrl ) {
				var r = await _srv._relyingParty.CreateRequestAsync( _id, new Realm( _srv.Realm() ), returnUrl );
				return await r.GetRedirectingResponseAsync();
			}

			public async Task<string> GetAuthenticatedUserUniqueId() {
				var rsp = await _srv._relyingParty.GetResponseAsync();
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
			readonly Func<JToken, string> _uniqueIDFromPerson;
			public OAuthMethod( string title, string logoClass,
				LoginService srv, string authEndpoint, string tokenEndpoint, string clientId, string clientSecret,
				string emailScope, string requestUrl, Func<JToken, string> uniqueIDFromPerson ) {
				_client = new WebServerClient( new AuthorizationServerDescription {
					AuthorizationEndpoint = new Uri( authEndpoint ),
					TokenEndpoint = new Uri( tokenEndpoint )
				}, clientId, ClientCredentialApplicator.PostParameter( clientSecret ) );
				Id = title;
				_srv = srv;
				_requestUrl = requestUrl;
				_emailScope = emailScope;
				_uniqueIDFromPerson = uniqueIDFromPerson;
				Title = title;
				LogoClass = logoClass;
			}

			public string Id { get; private set; }
			public string Title { get; private set; }
			public string LogoClass { get; private set; }

			public Task<HttpResponseMessage> GetRedirectResponse( Uri returnUrl ) {
				return _client.PrepareRequestUserAuthorizationAsync( new[] { _emailScope }, returnUrl );
			}

			public async Task<string> GetAuthenticatedUserUniqueId() {
				var st = await _client.ProcessUserAuthorizationAsync( _srv.HttpContext().Request );
				if ( st == null || st.AccessToken.NullOrEmpty() ) return null;

				var res = await new WebClient().DownloadStringTaskAsync( new Uri( string.Format( _requestUrl, st.AccessToken ) ) );
				var j = JToken.Parse( res );
				return _uniqueIDFromPerson( j );
			}
		}
	}
}