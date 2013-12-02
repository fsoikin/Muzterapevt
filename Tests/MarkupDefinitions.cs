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
	public class MarkupDefinitions
	{
		[Fact]
		public void Bold_italic_underline() {
			t( "123 **abc** xyz", "123 <b>abc</b> xyz" );
			t( "123 *abc* xyz", "123 <i>abc</i> xyz" );
			t( "123 _abc_ xyz", "123 <u>abc</u> xyz" );
		}

		[Fact]
		public void Headings() {
			t( "123 -abc- xyz", "123 <h2>abc</h2> xyz" );
			t( "123 --abc-- xyz", "123 <h1>abc</h1> xyz" );
		}

		[Fact]
		public void List() {
			t( @"abc
						* one
						* two
						* three
					xyz",
					@"abc<ul>
<li> one</li>
<li> two</li>
<li> three</li></ul>
					xyz");
		}

		[Fact]
		public void List_starting_at_first_line() {
			t( @" * one
						* two
						* three
					xyz",
					@"<ul><li> one</li>
<li> two</li>
<li> three</li></ul>
					xyz" );
		}

		[Fact]
		public void List_starting_at_last_line() {
			t( @" abc
	* one
						* two
						* three",
					@" abc<ul>
<li> one</li>
<li> two</li>
<li> three</li></ul>" );
		}
		void t( string source, string result = null ) {
			var res = new Mut.MarkupParser<Mut.UI.BBParseArgs>().Parse( source, null, new Mut.UI.MarkupDefinitions().Defs );
			var actual = string.Join( "", res.Select( r => r.Instance.ToHtml() ) );
			Assert.Equal( result, actual );
		}
	}
}