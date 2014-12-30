using System;
using System.Linq;
using System.Web.Mvc;
using erecruit.Composition;
using erecruit.Utils;
using Mut.Data;
using Mut.Models;
using Mut.UI;
using Newtonsoft.Json;

namespace Mut.Controllers
{
	public class PlaylistMarkdown
	{
		[Import]
		public MarkdownParser<MarkdownParseArgs> Parser { get; set; }

		[Export]
		IMarkdownProvider Tag() {
			return MarkdownProvider.Create(
				Parser.ComplexTag( "audio-playlist", true, new string[0], ( args, attrs, inners ) => {
					var f = inners.FirstOrDefault();
					var l = inners.LastOrDefault();
					var insideText = f == null ? "" : f.SourceString.Substring( f.SourceStartIndex, l.SourceEndIndex - f.SourceStartIndex );

					string lines = null;
					if ( !insideText.NullOrEmpty() ) {
						lines = JsonConvert.SerializeObject( new {
							songs = JsonConvert
								.DeserializeAnonymousType( "[" + insideText + "]", new[] { new { href = "", name = "", index = "", length = "" } } )
								.EmptyIfNull()
								.Select( ln => new {
									href = args.AttachmentMixin.Action( m => m.Download( ln.href ) ),
									ln.name, ln.index, ln.length
								} )
						} );
					}

					return string.Format(
						@"<div class='autobind' data-controller='Vm, BL/Widgets/playlist' data-args='{0}'></div>",
						(lines??"").Replace("'", "&lsquo;") );
				} ) );
		}
	}
}