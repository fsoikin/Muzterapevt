using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using erecruit.Composition;

namespace Mut.JS.Menu
{
	// Defined in js/BackOffice/menu.ts
	public class Item
	{
		public int Id { get; set; }
		public string Text { get; set; }
		public string Link { get; set; }
		public int Order { get; set; }
		public IEnumerable<Item> SubItems { get; set; }
	}
}