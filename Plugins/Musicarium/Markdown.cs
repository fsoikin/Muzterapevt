using System.Linq;
using erecruit.Composition;
using Mut.UI;

namespace Mut.Musicarium
{
	class Markdown
	{
		[Export]
		static IMarkdownProvider MusicariumColorful = MarkdownProvider.Create( new MarkdownNodeDefinition<MarkdownParseArgs>(
			"(?i)!(?'word'Musicarium|Музыкариум)", (_1,rgx,_2,_3) => new TextNode( 
				"<span class='musicarium-text-logo'>" + string.Join( "", 
					rgx.Groups["word"].Value.Select( (chr,idx) => string.Format( "<span class='l{1}'>{0}</span>", chr, idx ) ) )
					+ "</span>",
					encode: false ) ) );
	}
}