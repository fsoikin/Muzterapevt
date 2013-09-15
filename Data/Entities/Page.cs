using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using erecruit.Composition;

namespace Mut.Data
{
	public class Page
	{
		public int Id { get; set; }
		public string Url { get; set; }
		public string Title { get; set; }
		public string BbText { get; set; }
		public string HtmlText { get; set; }
		public DateTime Created { get; set; }
		public DateTime Modified { get; set; }

		[Export] class Mapping : IModelMapping { public void Map( DbModelBuilder b ) { b.Entity<Page>(); } }
	}
}