using System.Collections.Generic;

namespace Mut.Models
{
	public class LayoutModel
	{
		public bool ShowAdminMenu { get; set; }
		public TopMenuModel TopMenu { get; set; }
		public TopMenuModel SecondTopMenu { get; set; }
		public TextModel TopRight { get; set; }
		public TextModel Left { get; set; }
		public TextModel Right { get; set; }
	}
}