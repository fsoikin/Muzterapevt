using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using erecruit.Composition;

namespace Mut.Data
{
	public class Organization
	{
		public int Id { get; set; }
		public string Name { get; set; }

		[Export]
		class Mapping : IModelMapping { public void Map( DbModelBuilder b ) { b.Entity<Organization>(); } }
	}
}