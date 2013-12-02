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
		private readonly MarkupParser<BBParseArgs> _parser = new MarkupParser<BBParseArgs>();

		[Export]
		public IEnumerable<MarkupNodeDefinition<BBParseArgs>> Defs {
			get {
				return new[] {
					_parser.Wrap( "**", "b" ),
					_parser.Wrap( "*", "i" ),
					_parser.Wrap( "_", "u" ),

					_parser.Wrap( "--", "h1" ),
					_parser.Wrap( "-", "h2" ),

					_parser.ComplexTag( "c", true, new[] { "" }, (ctx,atrs) => new Range<string> {
						Start = "<span class=\"" + atrs.ValueOrDefault("") + "\">",
						End = "</span>"
					} ),

					_parser.ComplexTag( "url", true, new[] { "" }, (ctx,atrs) => new Range<string> {
						Start = "<a href=\"" + atrs.ValueOrDefault("") + "\">",
						End = "</a>"
					} ),

					// List
					new MarkupNodeDefinition<BBParseArgs>( @"((?<=([\n\r]+|^)(?![\r\n])\s*[^\s\*][^\r\n]+)(?=[\r\n]+\s+\*))|(^(?=\s+\*))", @"(?=[\r\n]+\s*[^\*\s])", (ctx,_,__,inners) => new WrapNode( "ul", inners ) ),

					// List item
					new MarkupNodeDefinition<BBParseArgs>( @"(?<=([\n\r]+)|^)(?![\r\n])\s+\*", @"(?=[\r\n]+)", (ctx,_,__,inners) => new WrapNode( "li", inners ) ),
				};
			}
		}
	}
}