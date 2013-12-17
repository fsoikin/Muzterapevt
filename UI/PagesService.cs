using System.Collections.Generic;
using System.Data.Objects.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using erecruit.Composition;
using erecruit.Utils;
using Mut.Data;

namespace Mut
{
	[Export, TransactionScoped]
	public class PagesService
	{
		[Import] public IRepository<Page> Pages { get; set; }
		[Import] public IUnitOfWork UnitOfWork { get; set; }

		public static string AttachmentDomain( int pageId ) { return "PageAttachment." + pageId; }

		public Page GetPage( string url, bool create ) {
			url = url ?? "";
			var p = Pages.All.FirstOrDefault( x => x.Url == url );
			if ( p == null && create ) {
				p = Pages.Add( new Page { Url = url } );
				UnitOfWork.Commit();
			}

			return p;
		}

		public IQueryable<Page> GetChildPages( IEnumerable<Page> parents ) {
			return Pages.All
				.Where( p => p.Title != null || p.BbText != null || p.HtmlText != null || p.ReferenceName != null )
				.Where( parents
					.Select( p => p.Url + '/' )
					.Select( pr => {
						var len = pr.Length;
						return Expr.Create( ( Page p ) => p.Url.StartsWith( pr ) && SqlFunctions.CharIndex( "/", p.Url.Substring( len ) ) <= 0 );
					} )
					.Fold( Expression.OrElse )
				);
		}

		public Page GetParentPage( Page child, bool create ) {
			var slashIndex = child.Url.LastIndexOf( '/' );
			var parentUrl = slashIndex < 0 ? "" : child.Url.Substring( 0, slashIndex );
			return GetPage( parentUrl, create );
		}

		public IQueryable<Page> GetParentPages( Page child ) {
			return from p in Pages.All
						 where child.Url.StartsWith( p.Url )
						 select p;
		}
	}
}