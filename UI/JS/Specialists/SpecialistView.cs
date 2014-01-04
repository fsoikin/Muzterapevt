using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using erecruit.Composition;
using Mut.UI;

namespace Mut.JS
{
	public class SpecialistView
	{
		public int Id { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string PatronymicName { get; set; }
		public string City { get; set; }
		public string Profession { get; set; }
		public string ProfessionDescription { get; set; }
		public string Specialization { get; set; }
		public string SpecializationDescription { get; set; }
		public string Resume { get; set; }
		public string Email { get; set; }
		public bool IsEmailPublic { get; set; }
		public string Phone { get; set; }
		public bool IsPhonePublic { get; set; }
		public string Url { get; set; }
		public string PhotoUrl { get; set; }
		public string Organization { get; set; }
		public string[] Countries { get; set; }
	}
}