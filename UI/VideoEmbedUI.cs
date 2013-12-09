using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using CodeKicker.BBCode;
using erecruit.Composition;
using erecruit.Utils;

namespace Mut.UI
{
	public class VideoEmbedUI
	{
		public MarkupNodeDefinition<MarkupParseArgs> VideoBBTag() {
			return new MarkupParser<MarkupParseArgs>().ComplexTag( "video", false, new[] { "" },
				( ctx, attrs, inners ) => HtmlFromUrl( attrs.ValueOrDefault( "" ) ) );
		}

		public MarkupNodeDefinition<MarkupParseArgs> PlaylistBBTag() {
			return new MarkupParser<MarkupParseArgs>().ComplexTag( "playlist", false, new[] { "" },
				( ctx, attrs, inners ) => {
					var m = _youtubePlaylist.Match( attrs.ValueOrDefault( "" ) ?? "" );
					if ( m.Success ) return "<div class='autobind' data-controller='BL/Widgets/youtubePlaylist' data-args='{ id: \"" + m.Groups["id"] + "\" }'></div>";
					return null;
				} );
		}

		public static string HtmlFromUrl( string url ) {
			if ( url.NullOrEmpty() ) return null;

			var toTry = new[] {
				new { regex = _youtube, tpl = _youtubeTemplate },
				new { regex = _youtubeShort, tpl = _youtubeTemplate },
//				new { regex = _facebook, tpl = _facebookTemplate },
			};
			foreach ( var x in toTry ) {
				var match = x.regex.Match( url );
				var id = match.Success ? match.Groups["id"].Value : null;
				if ( id != null ) return string.Format( x.tpl, id, 560, 315 ); // TODO: configurable dimensions
			}

			return "";
		}

		static string domainRegex( string domain, string firstLevel = null ) { return @"^(https{0,1}\:\/\/){0,1}(www\.){0,1}" + domain + @"\." + (firstLevel ?? "com"); }
		static string paramRegex( string param ) { return @"(\/[^&\?]*){0,1}\?([^&]+&)*((?<=&|\?)" + param + @"=(?'id'[^&]+))"; }

		internal static readonly Regex _youtube = new Regex( domainRegex( "youtube" ) + paramRegex( "v" ) );
		internal static readonly Regex _youtubeShort = new Regex( domainRegex( "youtu", "be" ) + "((" + paramRegex( "h" ) + @")|(\/(?'id'.+)))" );
		internal static readonly Regex _youtubePlaylist = new Regex( domainRegex( "youtube" ) + paramRegex( "list" ) );
		internal static readonly string _youtubeTemplate = @"<iframe width=""{1}"" height=""{2}"" src=""//www.youtube.com/embed/{0}"" frameborder=""0"" allowfullscreen></iframe>";

		//		static readonly Regex _facebook = new Regex( domainRegex( "facebook" ) + paramRegex( "v" ) );
		//		static readonly string _facebookTemplate = @"<div class=""fb-post"" data-href=""https://www.facebook.com/photo.php?v={0}"">";
	}
}