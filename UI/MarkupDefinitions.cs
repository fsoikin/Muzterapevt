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
	public class MarkupParseArgs
	{
		public erecruit.Mvc.MixinRouteBuilder<AttachmentUI.Mixin> AttachmentMixin { get; set; }
	}

	public class MarkupUI
	{
		private readonly MarkupParser<MarkupParseArgs> _parser = new MarkupParser<MarkupParseArgs>();

		[Import] public VideoEmbedUI Video { get; set; }
		[Import] public AttachmentUI Attachments { get; set; }

		public string ToHtml( string text, MarkupParseArgs args ) {
			return string.Join( "", _parser.Parse( text, args, Defs ).Select( n => n.Instance.ToHtml() ) );
		}

		[Export]
		public IEnumerable<MarkupNodeDefinition<MarkupParseArgs>> Defs {
			get {
				return new[] {
					_parser.Wrap( "**", "b" ),
					_parser.Wrap( "*", "i" ),
					_parser.Wrap( "_", "u" ),

					heading( @"\-", "h2" ),
					heading( @"\-\-", "h1" ),

					_parser.ComplexTag( "c", true, new[] { "" }, (ctx,atrs) => new Range<string> {
						Start = "<span class=\"" + atrs.ValueOrDefault("") + "\">",
						End = "</span>"
					} ),

					_parser.ComplexTag( "url", true, new[] { "" }, (ctx,atrs) => new Range<string> {
						Start = "<a href=\"" + atrs.ValueOrDefault("") + "\">",
						End = "</a>"
					} ),

					// List
					new MarkupNodeDefinition<MarkupParseArgs>( @"((?<=([\n\r]+|^)(?![\r\n])\s*[^\s\*][^\r\n]+)(?=[\r\n]+\s+\*))|(^(?=\s+\*))", @"(?=[\r\n]+\s*[^\*\s])", (ctx,_,__,inners) => new WrapNode( "ul", inners ) ),

					// List item
					new MarkupNodeDefinition<MarkupParseArgs>( @"(?<=([\n\r]+)|^)(?![\r\n])\s+\*", @"(?=[\r\n]+)", (ctx,_,__,inners) => new WrapNode( "li", inners ) ),

					Video.BBTag(),
					Attachments.BBImageTag(),
					Attachments.BBFileTag(),

					_parser.ComplexTag( "html", true, new string[0], (ctx, attrs, inners) => {
						var f = inners.FirstOrDefault();
						var l = inners.LastOrDefault();
						return f == null ? "" : f.SourceString.Substring( f.SourceStartIndex, l.SourceEndIndex - f.SourceStartIndex );
					} ),

					new MarkupNodeDefinition<MarkupParseArgs>( @"&[^;\s]+;", (ctx, regex, _, __) => new TextNode( regex.Value, false ) ),

					new MarkupNodeDefinition<MarkupParseArgs>( @"[\r\n]{2}", (ctx, regex, _, __) => new TextNode( "<br/>", false ) ),

					// Legacy:
					_parser.SimpleTag("b"), _parser.SimpleTag("i"), _parser.SimpleTag("u"),
					_parser.SimpleTag("h", "h2"), _parser.SimpleTag("h1")
				};
			}
		}

		MarkupNodeDefinition<MarkupParseArgs> heading( string markup, string htmlTag ) {
			return new MarkupNodeDefinition<MarkupParseArgs>(
				@"(?<=([\r\n]+)|^)((?![\r\n])\s)*" + markup + @"(?=[^\-]+" + markup + @"\s*([\r\n]|$))",
				@"(?<=(([\r\n]+)|^)((?![\r\n])\s)*" + markup + @"[^\-]+)" + markup + @"((?![\r\n])\s)*(?=([\r\n]|$))",
				( ctx, _, __, inners ) => new WrapNode( htmlTag, inners ) );
		}
	}
}