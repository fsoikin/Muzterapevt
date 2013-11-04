using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using erecruit.Composition;

namespace Mut.Data
{
	public class Picture
	{
		public int Id { get; set; }
		public string FileName { get; set; }
		public virtual Page Page { get; set; }

		[Export] class Mapping : IModelMapping { public void Map( DbModelBuilder b ) { b.Entity<Picture>(); } }
	}
}