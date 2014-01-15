using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Security;
using erecruit.Composition;
using Mut.Data;

namespace Mut
{
	public interface IAuthService
	{
		ISecurityActor CurrentActor { get; }
		void SetCurrentActor( User u, bool permanent );
	}

	public interface ISecurityActor
	{
		string Name { get; }
		bool IsAdmin { get; }
		bool IsAuthenticated { get; }
	}

	public interface IUserActor : ISecurityActor
	{
		User User { get; }
	}


	[Export, TransactionScoped]
	class AspNetAuthService : IAuthService
	{
		[Import] public IRepository<User> Users { get; set; }

		ISecurityActor _currentActor;
		public ISecurityActor CurrentActor
		{
			get
			{
				if ( _currentActor != null ) return _currentActor;

				var p = HttpContext.Current.User as Principal;
				if ( p != null ) return _currentActor = p;

				var i = HttpContext.Current.User == null ? null : HttpContext.Current.User.Identity;
				var u = i == null || !i.IsAuthenticated || !i.Name.All( char.IsDigit ) ? null : Users.Find( int.Parse( i.Name ) );
				if ( u == null ) return (_currentActor = new Guest());

				HttpContext.Current.User = p = new Principal( u );
				_currentActor = p;
				return p;
			}
		}

		public void SetCurrentActor( User u, bool permanent )
		{
			if ( u == null )
			{
				_currentActor = new Guest();
				FormsAuthentication.SignOut();
				return;
			}

			_currentActor = new Principal( u );
			FormsAuthentication.SetAuthCookie( u.Id.ToString(), permanent );
		}

		class Principal : IPrincipal, IUserActor
		{
			public User User { get; set; }
			public IIdentity Identity { get; set; }
			public bool IsInRole( string role ) { return false; }

			public string Name { get { return User.Name; } }
			public bool IsAdmin { get { return User.IsAdmin; } }
			public bool IsAuthenticated { get { return true; } }

			public Principal ( User u )
			{
				User = u;
				Identity = new GenericIdentity( u.Name ?? "" );
			}
		}

		class Guest : ISecurityActor
		{
			public string Name { get { return "Guest"; } }
			public bool IsAdmin { get { return false; } }
			public bool IsAuthenticated { get { return false; } }
		}
	}
}