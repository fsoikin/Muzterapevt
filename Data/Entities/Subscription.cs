using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using erecruit.Composition;

namespace Mut.Data
{
	public class Subscription
	{
		public int Id { get; set; }
		public bool Verified { get; set; }
		public string Email { get; set; }
		public Guid VerificationToken { get; set; }

		[Export]
		class Mapping : IModelMapping { public void Map( DbModelBuilder b ) { b.Entity<Subscription>(); } }
	}
}