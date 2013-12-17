using System.Collections.Generic;

namespace Mut.Models
{
	public class PageModel
	{
		public bool AllowEdit { get; set; }
		public Data.Page Page { get; set; }
		public IEnumerable<PageModel> ChildPages { get; set; }
	}
}