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
	[Export, TransactionScoped]
	public class BBCodeUI
	{
		[Import]
		public ICompositionRoot Composition { get; set; }

		private BBCodeParser _parser = new BBCodeParser( new [] {
			new BBTag( "br", "<br/>", "", true, false ),
			new BBTag( "b", "<b>", "</b>" ),
			new BBTag( "i", "<i>", "</i>" ),
			new BBTag( "u", "<u>", "</u>" ),
			new BBTag( "h", "<h2>", "</h2>" ),
			new BBTag( "h1", "<h1>", "</h1>" ),
			new BBTag( "c", "<span class='${class}'>", "</span>", new BBAttribute( "class", "" ) ),
			new BBTag( "url", "<a href=\"${href}\">", "</a>", new BBAttribute( "href", "" ) ),
			
//			new BBTag( "anchor", "", "", true, false, new BBAttribute( "link", "link" ), new BBAttribute( "name", "" ) ),

			new BBTag( "list", "<ul>", "</ul>" ),
			new BBTag( "*", "<li>", "</li>", true, false ),

			new BBTag( "video", "<x>", "</x>", true, false, new BBAttribute( "url", "" ) ),
			new BBTag( "html", "<x>", "</x>" ),
			new BBTag( "img", "<img src='${src}'/>", "", true, false, new BBAttribute( "src", "" ), 
				new BBAttribute( "width", "width" ), new BBAttribute( "height", "height" ) ),
			new BBTag( "file", "<a href='${path}'/>", "", true, true, new BBAttribute( "path", "" ) )
		} );

		static readonly Dictionary<string, Func<BBParseContext, string>> _tagsMap = new Dictionary<string, Func<BBParseContext, string>> {
			{ "video", ctx => VideoEmbedUI.HtmlFromUrl( ctx.Node.AttrValue( "url" ) ) },
//			{ "html", ctx => new SequenceNode( ctx.Node.SubNodes ).ToBBCode().Replace( "\n", "<br/>" ).Replace( "\r", "" ) },
			{ "img", ctx => AttachmentUI.BB.Image( ctx ) },
			{ "file", ctx => AttachmentUI.BB.File( ctx ) },
			//{ "anchor", ctx => {
			//	var name = ctx.Node.AttrValue( "name" );
			//	var link = ctx.Node.AttrValue( "link" );
			//	if ( link.NullOrEmpty() ) { }
			//}}
		};

		public string ToHtml( string bbCode, BBParseArgs args )
		{
			var syntaxTree = _parser.ParseSyntaxTree( bbCode );
			var modifiedTree = new V( Composition, args ?? new BBParseArgs() ).Visit( syntaxTree );
			return modifiedTree.ToHtml();
		}

		class V : SyntaxTreeVisitor
		{
			private readonly BBParseArgs _args;
			private readonly ICompositionRoot _comp;
			public V( ICompositionRoot comp, BBParseArgs args ) {
				Contract.Requires( args != null );
				Contract.Requires( comp != null );
				_args = args;
				_comp = comp;
			}

			protected override SyntaxTreeNode Visit( TagNode node ) {

				//var converter = _tagsMap.ValueOrDefault( node.Tag.Name );
				//if ( converter != null ) {
				//	var text = new TextNode( "", converter( new BBParseContext { Node = node, Args = _args, Composition = _comp } ) );
				//	return node.Tag.RequiresClosingTag ? text : base.Visit( new SequenceNode( node.SubNodes.StartWith( text ) ) );
				//}

				return base.Visit( node );
			}
		}
	}

	public class BBParseContext
	{
		public TagNode Node { get; set; }
		public ICompositionRoot Composition { get; set; }
		public BBParseArgs Args { get; set; }
		}

	public class BBParseArgs
	{
		public erecruit.Mvc.MixinRouteBuilder<AttachmentUI.Mixin> AttachmentMixin { get; set; }
	}

	public static class BBTagExtensions
	{
		public static string AttrValue( this TagNode tag, string attrId ) {
			return tag.AttributeValues.Where( a => a.Key.ID == attrId ).Select( a => a.Value ).FirstOrDefault();
		}
	}
}