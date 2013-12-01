using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Moq;
using Mut.Data;
using Xunit;
using erecruit.Utils;

namespace Mut.Tests
{
	public class MarkupParser
	{
		[Fact]
		public void Should_parse_single_node() {
			t( "[b]abc[/b]" );
		}

		[Fact]
		public void Should_parse_top_level_text_nodes() {
			t( "abc[b]d[/b]xyz" );
		}

		[Fact]
		public void Should_parse_single_top_level_text_nodes() {
			t( "xyz" );
		}

		[Fact]
		public void Should_parse_nested_nodes() {
			t( "abc[b]d[i]e[/i][/b]xyz" );
		}

		[Fact]
		public void Should_force_close_unclosed_nested_nodes() {
			t( "abc[b]d[i]e[/b]xyz", "abc<b>d<i>e</i></b>xyz" );
		}

		[Fact]
		public void Should_force_close_unclosed_trailing_nodes() {
			t( "abc[b]d[i]e[/i]xyz", "abc<b>d<i>e</i>xyz</b>" );
		}

		[Fact]
		public void Should_force_close_multiple_unclosed_trailing_nodes() {
			t( "abc[b]d[i]exyz", "abc<b>d<i>exyz</i></b>" );
		}

		[Fact]
		public void Should_tolerate_undefined_end_regex() {
			t( "abc[img]d[i]exyz", "abc<img>d<i>exyz</i>", new[] {
				Mut.MarkupParser.SimpleTag( "i" ),
				new MarkupNodeDefinition( "\\[img\\]", (a,b,c) => new TextNode( "<img>", false ) )
			});
		}

		[Fact]
		public void Should_tolerate_overlapping_start_definitions() {
			t( "abc**d**ex*y*z", "abc<b>d</b>ex<i>y</i>z", new[] {
				Mut.MarkupParser.Wrap( "**", "b" ),
				Mut.MarkupParser.Wrap( "*", "i" )
			} );
		}

		[Fact]
		public void Should_parse_default_attribute_of_complex_tag() {
			t( "abc [img=whatever.jpg] xyz", "abc <img src=\"whatever.jpg\" /> xyz", new[] { _imgTag } );
		}

		[Fact]
		public void Should_parse_nondefault_attributes_of_complex_tag() {
			t( "abc [img=whatever.jpg width=123] xyz", "abc <img src=\"whatever.jpg\" width=\"123\" /> xyz", new[] { _imgTag } );
		}

		[Fact]
		public void Should_parse_multiple_nondefault_attributes_of_complex_tag() {
			t( "abc [img=whatever.jpg width=123 height=456] xyz", 
				"abc <img src=\"whatever.jpg\" width=\"123\" height=\"456\" /> xyz", new[] { _imgTag } );
		}

		[Fact]
		public void Should_ignore_order_of_attributes_of_complex_tag() {
			t( "abc [img=whatever.jpg height=456 width=123] xyz",
				"abc <img src=\"whatever.jpg\" width=\"123\" height=\"456\" /> xyz", new[] { _imgTag } );
		}

		[Fact]
		public void Should_tolerate_multiple_occurrences_of_same_attribute_in_complex_tag() {
			t( "abc [img=whatever.jpg width=123 height=456 width=abcd] xyz",
				"abc <img src=\"whatever.jpg\" width=\"abcd\" height=\"456\" /> xyz", new[] { _imgTag } );
		}

		[Fact]
		public void Should_tolerate_absence_of_default_attribute_in_complex_tag() {
			t( "abc [img width=123 height=456 width=abcd] xyz",
				"abc <img src=\"\" width=\"abcd\" height=\"456\" /> xyz", new[] { _imgTag } );
		}

		[Fact]
		public void Should_tolerate_total_absence_of_attributes_in_complex_tag() {
			t( "abc [img] xyz", "abc <img src=\"\" /> xyz", new[] { _imgTag } );
		}

		[Fact]
		public void Should_parse_complex_tag_without_default_attribute_defined() {
			t( "abc [link url=abcd] xyz", "abc <a href=\"abcd\"> xyz", new[] { _unclosedLinkTag } );
		}

		[Fact]
		public void Should_fail_to_parse_complex_tag_without_default_attribute_defined_when_default_attribute_is_specified() {
			t( "abc [link=abcd] xyz", "abc [link=abcd] xyz", new[] { _unclosedLinkTag } );
		}

		[Fact]
		public void Should_parse_attribute_values_with_spaces_in_complex_tag() {
			t( "abc [link url=\"ab cd\"] xyz", "abc <a href=\"ab cd\"> xyz", new[] { _unclosedLinkTag } );
		}

		[Fact]
		public void Should_parse_in_complex_tag_with_content() {
			t( "abc [link url=\"ab cd\"]text text[/link] xyz", "abc <a href=\"ab cd\">text text</a> xyz", new[] { _linkTag } );
		}

		static MarkupNodeDefinition _imgTag = Mut.MarkupParser.ComplexTag( "img", false, 
			new[] { "", "width", "height" },
			( atrs, _ ) => {
				var width = atrs.ValueOrDefault( "width" );
				var height = atrs.ValueOrDefault( "height" );
				width = width.NullOrEmpty() ? "" : (" width=\"" + width + "\"");
				height = height.NullOrEmpty() ? "" : (" height=\"" + height + "\"");
				return string.Format( "<img src=\"{0}\"{1}{2} />", atrs.ValueOrDefault( "" ), width, height );
			} );

		static MarkupNodeDefinition _unclosedLinkTag = Mut.MarkupParser.ComplexTag( "link", false,
			new[] { "url" }, ( atrs, _ ) => "<a href=\"" + atrs.ValueOrDefault( "url" ) + "\">" );

		static MarkupNodeDefinition _linkTag = Mut.MarkupParser.ComplexTag( "link", true,
			new[] { "url" }, atrs => new Range<string> {
				Start = "<a href=\"" + atrs.ValueOrDefault( "url" ) + "\">",
				End = "</a>"
			} );

		void t( string source, string result = null, IEnumerable<MarkupNodeDefinition> defs = null ) {
			if ( result == null ) result = source.Replace( '[', '<' ).Replace( ']', '>' );
			var res = Mut.MarkupParser.Parse( source, defs ?? StdDefs );
			Assert.Equal( result, string.Join( "", res.Select( r => r.Instance.ToHtml() ) ) );
		}

		static readonly MarkupNodeDefinition[] StdDefs = new[] {
			Mut.MarkupParser.SimpleTag( "b" ),
			Mut.MarkupParser.SimpleTag( "i" ),
			Mut.MarkupParser.SimpleTag( "u" )
		};
	}
}