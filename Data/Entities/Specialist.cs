using System.Collections.Generic;
using System.Data.Entity;
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

		public string ContactEmail { get; set; }
		public string PublicEmail { get; set; }

		public string ContactPhone { get; set; }
		public string PublicPhone { get; set; }

		public string Resume { get; set; }
		public string Url { get; set; }
		public File Photo { get; set; }

		public virtual Organization Organization { get; set; }

		public string City { get; set; }
		public virtual ICollection<Region> Regions { get; set; }

		public virtual SpecialistProfession Profession { get; set; }
		public string ProfessionDescription { get; set; }
	
		public virtual SpecialistSpecialization Specialization { get; set; }
		public string SpecializationDescription { get; set; }

		public virtual SpecialistExperienceBracket Experience { get; set; }
		public string ExperienceDescription { get; set; }
		public string FormalEducation { get; set; }
		public string MusicTherapyEducation { get; set; }

		public Specialist() {
			this.Regions = new HashSet<Region>();
		}

		[Export]
		class Mapping : IModelMapping
		{
			public void Map( DbModelBuilder b ) {
				var s = b.Entity<Specialist>();
				s.HasRequired( x => x.Specialization ).WithMany();
				s.HasRequired( x => x.Profession ).WithMany();
				s.HasRequired( x => x.Experience ).WithMany();
				s.HasMany( x => x.Regions ).WithMany();
				s.HasOptional( x => x.Organization ).WithMany();
			}
		}
	}

	public class SpecialistProfession
	{
		public int Id { get; set; }
		public bool IsNull { get; set; }
		public int Order { get; set; }
		public string Name { get; set; }
	}

	public class SpecialistSpecialization
	{
		public int Id { get; set; }
		public bool IsNull { get; set; }
		public int Order { get; set; }
		public string Name { get; set; }
	}

	public class SpecialistExperienceBracket
	{
		public int Id { get; set; }
		public bool IsNull { get; set; }
		public int Order { get; set; }
		public string Name { get; set; }
	}
}