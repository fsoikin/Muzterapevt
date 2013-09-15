using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Web.Mvc;
using erecruit.Composition;
using Mut.Data;
using Mut.Models;
using Mut.UI;
using Mut.Web;

namespace Mut.Controllers.BackOffice
{
	public class PagesController : Controller
	{
		[Import]
		public IRepository<Page> Pages { get; set; }

		[Import]
		public IUnitOfWork UnitOfWork { get; set; }

		public JsonResponse<IEnumerable<JS.Page>> All()
		{
			return JsonResponse.Catch( () => Pages.All.AsEnumerable().Select( p => new JS.Page {
				Id = p.Id, Path = p.Url
			}), Log );
		}

		public JsonResponse<Unit> Update( [JsonRequestBodyAttribute] JS.Page page ) {
			return JsonResponse.Catch( () => {
				var p = Pages.Find( page.Id );
				if ( p == null ) return JsonResponse<Unit>.NotFound;
				p.Url = page.Path;
				p.Modified = DateTime.Now;
				UnitOfWork.Commit();
				return JsonResponse<Unit>.Void;
			}, Log );
		}
		public JsonResponse<JS.Page> Create( [JsonRequestBodyAttribute] JS.Page page ) {
			return JsonResponse.Catch( () => {
				var p = Pages.Add( new Page { Url = page.Path } );
				UnitOfWork.Commit();
				return new JS.Page { Id = p.Id, Path = p.Url };
			}, Log );
		}
	}
}