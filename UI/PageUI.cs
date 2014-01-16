using System;
using System.Linq;
using System.Collections.Generic;
using erecruit.Composition;
using erecruit.Utils;
using Mut.Data;

namespace Mut
{
	[Export, TransactionScoped]
	public class PageUI
	{
		[Import]
		public IRepository<Page> Pages { get; set; }
		[Import]
		public PagesService PagesService { get; set; }
		[Import]
		public IAuthService Auth { get; set; }


		public Maybe<PageModel> PageModel( string url ) {
			return from p in PagesService.GetPage( url, Auth.CurrentActor.IsAdmin ).MaybeDefined()
						 from parents in PagesService.GetParentPages( p )
						 from topParent in parents.Where( pr => pr.Url != null && pr.Url != "" ).OrderBy( pr => pr.Url.Length ).FirstOrDefault() ?? p
						 select new PageModel {
							 Page = p,
							 TopParent = topParent,
							 AllowEdit = Auth.CurrentActor.IsAdmin,
							 ChildPages = GetChildren( topParent, parents.Count() )
						 };
		}

		private IEnumerable<PageModel> GetChildren( Data.Page p, int depth ) {
			var firstLevel = new[] { p }.ToList();
			var levels = EnumerableEx.Generate( new { pages = firstLevel, depth = Math.Max( 4, depth ) }, x => x.depth >= 0 && x.pages.Any(),
				x => new { pages = PagesService.GetChildPages( x.pages ).ToList(), depth = x.depth - 1 },
				x => x.pages )
				.ToList();
			return MergeChildPages( p, levels.Skip( 1 ) );
		}

		private IEnumerable<PageModel> MergeChildPages( Data.Page parent, IEnumerable<List<Data.Page>> levels ) {
			var prefix = parent.Url + "/";
			return from c in levels.FirstOrDefault().EmptyIfNull()
						 where c.Url.StartsWith( prefix )
						 orderby c.SortOrder
						 select new PageModel {
							 Page = c,
							 ChildPages = MergeChildPages( c, levels.Skip( 1 ) )
						 };
		}
	}
	public class PageModel
	{
		public bool AllowEdit { get; set; }
		public Data.Page TopParent { get; set; }
		public Data.Page Page { get; set; }
		public IEnumerable<PageModel> ChildPages { get; set; }
	}
}