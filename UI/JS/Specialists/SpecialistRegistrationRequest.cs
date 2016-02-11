namespace Mut.JS
{
	public class SpecialistRegistrationRequest
	{
		public string firstName { get; set; }
		public string lastName { get; set; }
		public string patronymicName { get; set; }

		public string contactEmail { get; set; }
		public string publicEmail { get; set; }
		public string publicPhone { get; set; }

		public string url { get; set; }
		public int photo { get; set; }
		public string organization { get; set; }
		public string resume { get; set; }

		public string city { get; set; }
		public int[] regions { get; set; }

		public int[] professions { get; set; }
		public string professionDescription { get; set; }

		public int[] specializations { get; set; }
		public string specializationDescription { get; set; }

		public int experience { get; set; }
		public string experienceDescription { get; set; }
		public string formalEducation { get; set; }
		public string musicTherapyEducation { get; set; }
  }
}