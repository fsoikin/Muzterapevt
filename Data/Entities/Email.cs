using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using erecruit.Composition;

namespace Mut.Data
{
	public class Email
	{
		public int Id { get; set; }
		public string Subject { get; set; }
		public string Body { get; set; }
		public string ToName { get; set; }
		public string ToEmail { get; set; }
		public string FromName { get; set; }
		public string FromEmail { get; set; }

		[Export]
		class Mapping : IModelMapping { public void Map( DbModelBuilder b ) { b.Entity<Email>(); } }
	}
}