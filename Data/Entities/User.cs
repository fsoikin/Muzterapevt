using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using erecruit.Composition;

namespace Mut.Data
{
	public class User
	{
		public int Id { get; set; }
		public bool IsAdmin { get; set; }
		public string Login { get; set; }
		public string Name { get; set; }
		public string PasswordHash { get; set; }
		public string Email { get; set; }

		[Export] class Mapping : IModelMapping { public void Map( DbModelBuilder b ) { b.Entity<User>(); } }
	}
}