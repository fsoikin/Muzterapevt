using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using erecruit.Composition;

namespace Mut.Data
{
	public class Text
	{
		public string Id { get; set; }
		public Guid SiteId { get; set; }
		public string BbText { get; set; }
		public string HtmlText { get; set; }

		[Export]
		class Mapping : IModelMapping
		{
			public void Map( DbModelBuilder b ) {
				b.Entity<Text>().HasKey( x => new { x.Id, x.SiteId } );
			}
		}
	}
}