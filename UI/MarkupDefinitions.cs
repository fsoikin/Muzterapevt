using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using CodeKicker.BBCode;
using CodeKicker.BBCode.SyntaxTree;
using erecruit.Composition;
using erecruit.Utils;

namespace Mut.UI
{
	public class MarkupDefinitions
	{
		[Export]
		public IEnumerable<MarkupNodeDefinition> Defs = new[] {
				Mut.MarkupParser.Wrap( "**", "b" ),
				Mut.MarkupParser.Wrap( "*", "i" ),
				Mut.MarkupParser.Wrap( "_", "u" ),

				Mut.MarkupParser.Wrap( "--", "h1" ),
				Mut.MarkupParser.Wrap( "-", "h2" ),

				Mut.MarkupParser.ComplexTag( "c", true, new[] { "" }, atrs => new Range<string> {
					Start = "<span class=\"" + atrs.ValueOrDefault("") + "\">",
					End = "</span>"
				} ),

				Mut.MarkupParser.ComplexTag( "url", true, new[] { "" }, atrs => new Range<string> {
					Start = "<a href=\"" + atrs.ValueOrDefault("") + "\">",
					End = "</a>"
				} ),

				// List
				new MarkupNodeDefinition( @"((?<=([\n\r]+|^)(?![\r\n])\s*[^\s\*][^\r\n]+)(?=[\r\n]+\s+\*))|(^(?=\s+\*))", @"(?=[\r\n]+\s*[^\*\s])", (_,__,inners) => new WrapNode( "ul", inners ) ),

				// List item
				new MarkupNodeDefinition( @"(?<=([\n\r]+)|^)(?![\r\n])\s+\*", @"(?=[\r\n]+)", (_,__,inners) => new WrapNode( "li", inners ) ),
		};
	}
}