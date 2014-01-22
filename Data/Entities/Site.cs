using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using erecruit.Composition;

namespace Mut.Data
{
	public class Site
	{
		public Guid Id { get; set; }
		public string HostName { get; set; }
		public string Theme { get; set; }

		[Export] class Mapping : IModelMapping { public void Map( DbModelBuilder b ) { b.Entity<Site>(); } }
	}
}