using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeKicker.BBCode;
using erecruit.Composition;

namespace Mut.JS
{
	// Defined in js/page.ts
	public class PageEditor
	{
		public int Id { get; set; }
		public string Path { get; set; }
		public string Text { get; set; }
		public string Title { get; set; }
	}
}
