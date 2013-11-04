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
		public string TagsStandIn { get; set; }
		public string ReferenceName { get; set; }
		public DateTime Created { get; set; }
		public DateTime Modified { get; set; }
		public virtual ICollection<Picture> Pictures { get; set; }

		[Export] class Mapping : IModelMapping { public void Map( DbModelBuilder b ) { b.Entity<Page>(); } }

		public Page() {
			this.Created = DateTime.Now;
			this.Modified = DateTime.Now;
		}
	}
}