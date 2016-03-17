using System.Collections.Generic;
using System.Data.Entity;
using erecruit.Composition;

namespace Mut.Data
{
	public class Region
	{
		public int ID { get; set; }
		public virtual Region Parent { get; set; }
		public string ShortName { get; set; }
		public string Name { get; set; }
		public virtual ICollection<Specialist> Specialists { get; set; }

		public Region() {
			this.Specialists = new HashSet<Specialist>();
		}

		[Export]
		class Mapping : IModelMapping
		{
			public void Map( DbModelBuilder b ) {
				var r = b.Entity<Region>();
				r.HasOptional( x => x.Parent ).WithMany().Map( m => m.MapKey( "ParentID" ) );
			}
		}
	}
}