using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeKicker.BBCode;
using erecruit.Composition;

namespace Mut.JS.Menu
{
	// Defined in js/BackOffice/menu.ts
	public class SubItemsSaveRequest
	{
		public string MenuId { get; set; }
		public IEnumerable<Item> Items { get; set; }
	}
}
