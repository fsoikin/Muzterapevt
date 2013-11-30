using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Moq;
using Mut.Data;
using Xunit;

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
				Mut.MarkupParser.BbNode( "i" ),
				new MarkupNodeDefinition( "\\[img\\]", (a,b,c) => new TextNode( "<img>", false ) )
			});
		}

		[Fact]
		public void Should_tolerate_overlapping_start_definitions() {
			t( "abc**d**ex*y*z", "abc<b>d</b>ex<i>y</i>z", new[] {
				new MarkupNodeDefinition( "\\*\\*", "\\*\\*", (a,b,c) => new WrapNode( "b", c ) ),
				new MarkupNodeDefinition( "\\*", "\\*", (a,b,c) => new WrapNode( "i", c ) )
			} );
		}

		void t( string source, string result = null, IEnumerable<MarkupNodeDefinition> defs = null ) {
			if ( result == null ) result = source.Replace( '[', '<' ).Replace( ']', '>' );
			var res = Mut.MarkupParser.Parse( source, defs ?? Mut.MarkupParser.StdDefs );
			Assert.Equal( result, string.Join( "", res.Select( r => r.Instance.ToHtml() ) ) );
		}
	}
}