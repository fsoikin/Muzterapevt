using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeKicker.BBCode;
using erecruit.Composition;

namespace Mut.UI
{
	[Export, TransactionScoped]
	public class BBCodeUI
	{
		private BBCodeParser _parser = new BBCodeParser( new [] {
			new BBTag( "**", "<b>", "</b>", true, false ),
			new BBTag( "b", "<b>", "</b>" ),
			new BBTag( "i", "<i>", "</i>" ),
			new BBTag( "url", "<a href=\"${href}\">", "</a>", new BBAttribute( "href", "" ) ),

			new BBTag( "list", "<ul>", "</ul>" ),
			new BBTag( "*", "<li>", "</li>", true, false ),
		} );

		public string ToHtml( string bbCode )
		{
			return _parser.ToHtml( bbCode );
		}
	}
}
