namespace Mut.JS
{
	public class SpecialistView
	{
		public int id { get; set; }
		public string firstName { get; set; }
		public string lastName { get; set; }
		public string patronymicName { get; set; }

		public string profession { get; set; }
		public string professionDescription { get; set; }
		public string specialization { get; set; }
		public string specializationDescription { get; set; }
		public string experience { get; set; }
		public string experienceDescription { get; set; }
		public string formalEducation { get; set; }
		public string musicTherapyEducation { get; set; }

		public string resume { get; set; }

		public string contactEmail { get; set; }
		public string contactPhone { get; set; }
		public string publicEmail { get; set; }
		public string publicPhone { get; set; }

		public string url { get; set; }
		public string photoUrl { get; set; }
		public string organization { get; set; }

		public string city { get; set; }
		public string[] regions { get; set; }
	}
}