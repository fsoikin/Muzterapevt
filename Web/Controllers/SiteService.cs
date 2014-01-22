using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using erecruit.Composition;
using Mut.Data;
using Mut.Models;
using Mut.UI;

namespace Mut.Controllers
{
	[Export, TransactionScoped]
	public class SiteService : ISiteService
	{
		[Import] public HttpContextBase HttpContext { get; set; }
		[Import] public Func<IRepository<Site>> Sites { get; set; }
		[Import] public Func<IUnitOfWork> UnitOfWork { get; set; }
		[Import] public ITransactionService Tran { get; set; }
		readonly Lazy<Site> _site;

		public Guid CurrentSiteId { get { return _site.Value.Id; } }
		public string CurrentSiteTheme { get { return _site.Value.Theme; } }

		public SiteService() {
			_site = new Lazy<Site>( () => {
				var host = HttpContext.Request.Url.Host;
				if ( host.StartsWith( "www." ) ) host = host.Substring( "www.".Length );
				var res = Sites().All.FirstOrDefault( s => s.HostName == host );
				
				if ( res == null ) 
					using ( Tran.OpenTransaction() ) {
						res = Sites().Add( new Site { HostName = host, Id = Guid.NewGuid(), Theme = "" } );
						UnitOfWork().Commit();
					}

				return res;
			} );
		}
	}
}