using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeKicker.BBCode;
using CodeKicker.BBCode.SyntaxTree;
using erecruit.Composition;
using erecruit.Utils;

namespace Mut.UI
{
	[Export, TransactionScoped]
	public class BBCodeUI
	{
		private BBCodeParser _parser = new BBCodeParser( new [] {
			new BBTag( "br", "<br/>", "", true, false ),
			new BBTag( "b", "<b>", "</b>" ),
			new BBTag( "i", "<i>", "</i>" ),
			new BBTag( "h", "<h2>", "</h2>" ),
			new BBTag( "h1", "<h1>", "</h1>" ),
			new BBTag( "c", "<span class='${class}'>", "</span>", new BBAttribute( "class", "" ) ),
			new BBTag( "url", "<a href=\"${href}\">", "</a>", new BBAttribute( "href", "" ) ),

			new BBTag( "list", "<ul>", "</ul>" ),
			new BBTag( "*", "<li>", "</li>", true, false ),

			new BBTag( "video", "<x>", "</x>", true, false, new BBAttribute( "url", "" ) ),
		} );

		static readonly Dictionary<string, Func<IDictionary<string, string>, string>> _tagsMap = new Dictionary<string, Func<IDictionary<string, string>, string>> {
			{ "video", args => VideoEmbedUI.HtmlFromUrl( args.ValueOrDefault( "url" ) ) }
		};

		public string ToHtml( string bbCode )
		{
			bbCode = bbCode.Replace( "\n", "[br]" ).Replace( "\r", "" );
			var syntaxTree = _parser.ParseSyntaxTree( bbCode );
			var modifiedTree = new V().Visit( syntaxTree );
			return modifiedTree.ToHtml();
		}

		class V : SyntaxTreeVisitor
		{
			protected override SyntaxTreeNode Visit( TagNode node ) {

				var converter = _tagsMap.ValueOrDefault( node.Tag.Name );
				if ( converter != null ) {
					return base.Visit( new SequenceNode( node.SubNodes.StartWith(
						new TextNode( "", converter( node.AttributeValues.ToDictionary( a => a.Key.ID, a => a.Value ) ) ) ) ) );
				}

				return base.Visit( node );
			}
		}
	}
}