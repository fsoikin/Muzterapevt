using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Moq;
using Mut.Data;
using Xunit;
using Xunit.Extensions;

namespace Mut.Tests
{
	public class VideoEmbedUI
	{
		public class Youtube
		{
			public class LongForm
			{
				[Theory, PropertyData("UrlPrefixes")]
				public void Should_convert_URL_with_solo_v_param( string urlPrefix ) {
					Assert.Equal( result( "abcd" ), UI.VideoEmbedUI.HtmlFromUrl( urlPrefix + "?v=abcd" ) );
				}

				[Theory, PropertyData( "UrlPrefixes" )]
				public void Should_convert_URL_with_multiple_params_with_v_param_being_first( string urlPrefix ) {
					Assert.Equal( result( "abcd" ), UI.VideoEmbedUI.HtmlFromUrl( urlPrefix + "?v=abcd&a=b&x=y" ) );
				}

				[Theory, PropertyData( "UrlPrefixes" )]
				public void Should_convert_URL_with_multiple_params_with_v_param_being_last( string urlPrefix ) {
					Assert.Equal( result( "abcd" ), UI.VideoEmbedUI.HtmlFromUrl( urlPrefix + "?a=b&x=y&v=abcd" ) );
				}

				[Theory, PropertyData( "UrlPrefixes" )]
				public void Should_convert_URL_with_multiple_params_with_v_param_being_in_the_middle( string urlPrefix ) {
					Assert.Equal( result( "abcd" ), UI.VideoEmbedUI.HtmlFromUrl( urlPrefix + "?a=b&v=abcd&x=y" ) );
				}

				public static IEnumerable<object[]> UrlPrefixes {
					get {
						return from s in new[] { "", "s" }
									 from www in new[] { "www.", "" }
									 from slash in new[] { "", "/" }
									 select new[] { string.Format( "http{0}://{1}youtube.com{2}", s, www, slash ) };
					}
				}
			}

			public class ShortForm
			{
				[Theory, PropertyData( "UrlPrefixes" )]
				public void Should_convert_URL_with_nameless_param( string urlPrefix ) {
					Assert.Equal( result( "abcd" ), UI.VideoEmbedUI.HtmlFromUrl( urlPrefix + "/abcd" ) );
				}

				[Theory, PropertyData( "UrlPrefixes" )]
				public void Should_convert_URL_with_h_param( string urlPrefix ) {
					Assert.Equal( result( "abcd" ), UI.VideoEmbedUI.HtmlFromUrl( urlPrefix + "?h=abcd" ) );
					Assert.Equal( result( "abcd" ), UI.VideoEmbedUI.HtmlFromUrl( urlPrefix + "/?h=abcd" ) );
				}

				public static IEnumerable<object[]> UrlPrefixes {
					get {
						return from s in new[] { "", "s" }
									 from www in new[] { "www.", "" }
									 select new[] { string.Format( "http{0}://{1}youtu.be", s, www ) };
					}
				}
			}

			static string result( string id ) {
				return string.Format( UI.VideoEmbedUI._youtubeTemplate, id, "560", "315" );
			}
		}
	}
}