using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using erecruit.Composition;

namespace Mut.JS
{
	// Defined in js/BL/page.ts
	public class PageReorderRequest
	{
		public int ParentId { get; set; }
		public int[] Children { get; set; }
	}
}
