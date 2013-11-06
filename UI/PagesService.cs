using System.Collections.Generic;
using System.Data.Objects.SqlClient;
using System.Linq;
using erecruit.Composition;
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

		public IQueryable<Page> GetChildPages( Page parent ) {
			var prefix = parent.Url + '/';
			return Pages.All.Where( p => 
				p.Url.StartsWith( prefix ) && SqlFunctions.CharIndex( p.Url.Substring( prefix.Length ), "/" ) <= 0
				&& ( p.Title != null || p.BbText != null || p.HtmlText != null ) );
		}

		public Page GetParentPage( Page child, bool create ) {
			var slashIndex = child.Url.LastIndexOf( '/' );
			var parentUrl = slashIndex < 0 ? "" : child.Url.Substring( 0, slashIndex );
			return GetPage( parentUrl, create );
		}
	}
}