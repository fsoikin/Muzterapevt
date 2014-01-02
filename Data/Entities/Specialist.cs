using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using erecruit.Composition;
using Name.Files;

namespace Mut.Data
{
	public class Specialist
	{
		public int Id { get; set; }
		public bool Approved { get; set; }
		public bool Ignored { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string PatronymicName { get; set; }
		public string City { get; set; }
		public string Resume { get; set; }
		public string Email { get; set; }
		public bool IsEmailPublic { get; set; }
		public string Phone { get; set; }
		public bool IsPhonePublic { get; set; }
		public string Url { get; set; }
		public File Photo { get; set; }

		public virtual Organization Organization { get; set; }
		public virtual ICollection<Country> Countries { get; set; }

		public virtual SpecialistProfession Profession { get; set; }
		public string ProfessionDescription { get; set; }
	
		public virtual SpecialistSpecialization Specialization { get; set; }
		public string SpecializationDescription { get; set; }

		public Specialist() {
			this.Countries = new HashSet<Country>();
		}

		[Export]
		class Mapping : IModelMapping
		{
			public void Map( DbModelBuilder b ) {
				var s = b.Entity<Specialist>();
				s.HasRequired( x => x.Specialization ).WithMany();
				s.HasRequired( x => x.Profession ).WithMany();
				s.HasMany( x => x.Countries ).WithMany();
				s.HasOptional( x => x.Organization ).WithMany();
			}
		}
	}

	public class SpecialistProfession
	{
		public int Id { get; set; }
		public int Order { get; set; }
		public string Name { get; set; }
	}

	public class SpecialistSpecialization
	{
		public int Id { get; set; }
		public int Order { get; set; }
		public string Name { get; set; }
	}
}