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

		public virtual ICollection<SpecialistProfession> Professions { get; set; }
		public string ProfessionDescription { get; set; }
	
		public virtual ICollection<SpecialistSpecialization> Specializations { get; set; }
		public string SpecializationDescription { get; set; }

		public virtual SpecialistExperienceBracket Experience { get; set; }
		public string ExperienceDescription { get; set; }
		public string FormalEducation { get; set; }
		public string MusicTherapyEducation { get; set; }

		public Specialist() {
			this.Regions = new HashSet<Region>();
			this.Professions = new HashSet<SpecialistProfession>();
			this.Specializations = new HashSet<SpecialistSpecialization>();
		}

		[Export]
		class Mapping : IModelMapping
		{
			public void Map( DbModelBuilder b ) {
				var s = b.Entity<Specialist>();
				s.HasMany( x => x.Specializations ).WithMany().Map( m => m.ToTable( "Specialist_Specialization" ).MapLeftKey( "Specialist_Id").MapRightKey( "Specialization_Id" ) );
				s.HasMany( x => x.Professions ).WithMany().Map( m => m.ToTable( "Specialist_Profession" ).MapLeftKey( "Specialist_Id" ).MapRightKey( "Profession_Id" ) );
				s.HasRequired( x => x.Experience ).WithMany();
				s.HasMany( x => x.Regions ).WithMany( x => x.Specialists );
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