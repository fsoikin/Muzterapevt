namespace Mut.JS
{
	public class Region
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public Region[] children { get; set; }
		public int totalSpecialists { get; set; }
	}
}