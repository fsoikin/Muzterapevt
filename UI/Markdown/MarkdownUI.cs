using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using erecruit.Composition;
using erecruit.JS;
using erecruit.Utils;
using Newtonsoft.Json;

namespace Mut.UI
{
	[Export, TransactionScoped]
	public class MarkdownUI
	{
		private readonly MarkdownParser<MarkdownParseArgs> _parser = new MarkdownParser<MarkdownParseArgs>();

		[Import] public VideoEmbedUI Video { get; set; }
		[Import] public AttachmentUI Attachments { get; set; }
		[Import] public IEnumerable<IMarkdownCustomModule> CustomModules { get; set; }
		[Import] public IEnumerable<IMarkdownProvider> Providers { get; set; }

		public string ToHtml( string text, MarkdownParseArgs args ) {
			return string.Join( "", _parser.Parse( text, args, Providers.SelectMany( p => p.Definitions ) ).Select( n => n.Instance.ToHtml() ) );
		}

		[Export]
		internal IMarkdownProvider StdDefinitions() {
			return MarkdownProvider.Create(
				new MarkdownNodeDefinition<MarkdownParseArgs>( @"\[\[", ( args, start, end, inners ) => new TextNode( "[" ) ),
				new MarkdownNodeDefinition<MarkdownParseArgs>( @"\]\]", ( args, start, end, inners ) => new TextNode( "]" ) ),

				_parser.Wrap( "**", "b" ),
				_parser.Wrap( "*", "i" ),
				_parser.Wrap( "_", "u" ),

				heading( @"\-\-", "h1" ),
				heading( @"\-", "h2" ),

				_parser.ComplexTag( "c", true, new[] { "" }, ( ctx, atrs ) => new Range<string> {
					Start = "<span class=\"" + atrs.ValueOrDefault( "" ) + "\">",
					End = "</span>"
				} ),

				_parser.ComplexTag( "url", true, new[] { "" }, ( ctx, atrs ) => new Range<string> {
					Start = "<a href=\"" + atrs.ValueOrDefault( "" ) + "\">",
					End = "</a>"
				} ),

				_parser.ComplexTag( "anchor", false, new[] { "" }, ( ctx, atrs, _ ) => "<a name=\"" + atrs.ValueOrDefault( "" ) + "\"></a>" ),
				_parser.ComplexTag( "jump", true, new[] { "" }, ( ctx, atrs ) => new Range<string> {
					Start = "<a href=\"#" + atrs.ValueOrDefault( "" ) + "\">", End = "</a>"
				} ),

				// List
				new MarkdownNodeDefinition<MarkdownParseArgs>( @"((?<=([\n\r]+|^)((?![\r\n])\s)*([^\s\*\r\n][^\r\n]+){0,1})(\r\n|\n\r|\r|\n)(?=((?![\r\n])\s)+\*))|(^(?=\s+\*))", @"(?<=(\r\n|\n\r|\r|\n)|$)(?!\s+\*)", ( ctx, _, __, inners ) => new WrapNode( "ul", inners ) ),

				// List item
				new MarkdownNodeDefinition<MarkdownParseArgs>( @"(?<=([\n\r]+)|^)((?![\r\n])\s)+\*(?=[^\r\n]+)", @"(?<=(([\n\r]+)|^)((?![\r\n])\s)+\*[^\r\n]+)((\r\n|\n\r|\r|\n)|$)", ( ctx, _, __, inners ) => new WrapNode( "li", inners ) ),

				Video.VideoBBTag(),
				Video.PlaylistBBTag(),
				Attachments.BBImageTag(),
				Attachments.BBFileTag(),

				_parser.ComplexTag( "html", true, new string[0], ( ctx, attrs, inners ) => {
					var f = inners.FirstOrDefault();
					var l = inners.LastOrDefault();
					return f == null ? "" : f.SourceString.Substring( f.SourceStartIndex, l.SourceEndIndex - f.SourceStartIndex );
				} ),

				new MarkdownNodeDefinition<MarkdownParseArgs>( @"&[^;\s]+;", ( ctx, regex, _, __ ) => new TextNode( regex.Value, false ) ),

				new MarkdownNodeDefinition<MarkdownParseArgs>( @"\r\n|\n\r|\n|\r", ( ctx, regex, _, __ ) => new TextNode( "<br/>", false ) ),

				// Legacy:
				_parser.SimpleTag( "b" ), _parser.SimpleTag( "i" ), _parser.SimpleTag( "u" ),
				_parser.SimpleTag( "h", "h2" ), _parser.SimpleTag( "h1" ) );
		}

		[Export]
		internal IMarkdownProvider AllowedHtml() {
			return MarkdownProvider.Create(
				new[] { "div", "span", "img" }.Select( t => _parser.HtmlTag( t, "style", "id", "class" ) ) );
		}

		[Export]
		internal IMarkdownProvider CustomModulesProvider() {
			return MarkdownProvider.Create(
				from m in CustomModules
				select _parser.ComplexTag( m.Name, false, m.Parameters.ToArray(), ( _, args, inners ) => {
					var classRef = m.GetClassRef( args );
					return string.Format( "<div class='autobind' data-controller='{0}, {1}' data-args='{2}'></div>", classRef.Class, classRef.Module,
						 HttpUtility.HtmlAttributeEncode( JsonConvert.SerializeObject( classRef.Arguments ) ) );
				} ) );
		}

		// TODO: this should be defined in the Web project, because it's coupled with the client-side code
		[Export]
		internal IMarkdownProvider GetTriviaTags() {
			var responseMarker = new MarkdownNodeDefinition<MarkdownParseArgs>( "::RESPONSE:", (_1,_2,_3,_4) => new TextNode("") );
			var triviaTag = _parser.ComplexTag( "trivia", true, new[] { "answers" }, ( args, attrs, inners ) => {
				var question = inners.TakeWhile( x => x.Def != responseMarker );
				var response = inners.SkipWhile( x => x.Def != responseMarker );
				return string.Format( @"
					<div class='autobind trivia' data-controller='Vm, BL/Widgets/trivia' data-args=""{0}"">
						<div class='question'>{1}</div>
						<div class='response' style='display: none'>{2}</div>
					</div>",
					HttpUtility.HtmlAttributeEncode( JsonConvert.SerializeObject( new {
						answers = from s in (attrs.ValueOrDefault( "answers" ) ?? "").Split( ',' )
											let t = s.Trim()
											where !t.NullOrEmpty()
											select t
					} ) ),
					string.Join( "", question.Select( n => n.Instance.ToHtml() ) ),
					string.Join( "", response.Select( n => n.Instance.ToHtml() ) ) );
			} );

			return MarkdownProvider.Create( responseMarker, triviaTag );
		}

		[Export]
		internal IMarkdownProvider HtmlTableTags() {
			var attrs = new[] { "style", "class", "id" };
			var td = _parser.HtmlTag( "td", attrs.Concat( new[] { "rowspan", "colspan" } ).ToArray() );
			var tr = _parser.HtmlTag( "tr", ii => ii.Where( i => i.Def == td ), attrs );
			var table = _parser.HtmlTag( "table", ii => ii.Where( i => i.Def == tr ), attrs );
			return MarkdownProvider.Create( td, tr, table );
		}

		MarkdownNodeDefinition<MarkdownParseArgs> heading( string markup, string htmlTag ) {
			return new MarkdownNodeDefinition<MarkdownParseArgs>(
				@"(?<=([\r\n]+)|^)((?![\r\n])\s)*" + markup + @"(?=[^\r\n]+" + markup + @"\s*([\r\n]|$))",
				@"(?<=(([\r\n]+)|^)((?![\r\n])\s)*" + markup + @"[^\r\n]+)" + markup + @"((?![\r\n])\s)*((\r\n|\n\r|\r|\n)|$)",
				( ctx, _, __, inners ) => new WrapNode( htmlTag, inners ) );
		}
	}
}