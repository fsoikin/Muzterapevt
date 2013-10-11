using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using erecruit.Composition;

namespace Mut.Data
{
	public class NavigationItem
	{
		public int Id { get; set; }
		public string Text { get; set; }
		public string Link { get; set; }
		public int Order { get; set; }
		public virtual NavigationItem Parent { get; set; }
		public virtual ICollection<NavigationItem> Children { get; set; }

		public NavigationItem()
		{
			this.Children = new HashSet<NavigationItem>();
		}

		[Export]
		class Mapping : IModelMapping { public void Map( DbModelBuilder b ) { b.Entity<NavigationItem>(); } }
	}
}