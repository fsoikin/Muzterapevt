using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using CodeKicker.BBCode.SyntaxTree;
using erecruit.Composition;
using erecruit.Mvc;
using erecruit.Mvc.Mixins;
using log4net;
using Mut.Data;
using Mut.Models;
using Mut.UI;
using Mut.Web;
using Name.Files;
using erecruit.Utils;

namespace Mut
{
	[Export, TransactionScoped]
	public class AttachmentUI
	{
		[Import] public IFileUI Files { get; set; }
		[Import] public ILog Log { get; set; }
		[Import] public IUnitOfWork UnitOfWork { get; set; }
		[Import] public IPictureUI Pictures { get; set; }

		public Mixin AsMixin( string domain ) {
			return new Mixin( this, domain );
		}

		public class Mixin : IMvcMixin
		{
			readonly AttachmentUI _ui;
			readonly string _domain;
			public Mixin( AttachmentUI ui, string domain ) {
				Contract.Requires( ui != null );
				_ui = ui;
				_domain = domain;
			}

			[DefaultMixinAction]
			public ActionResult Serve( string path ) { return _ui.Files.ServeFile( _domain, path, false ); }
			public ActionResult Download( string path ) { return _ui.Files.ServeFile( _domain, path, true ); }
			public ActionResult Img( string path ) { return _ui.Files.ServeFile( _domain, path, false ); }
			public ActionResult Crop( string path, int width, int height ) { return _ui.Pictures.ServeCropped( _domain, path, width, height ); }
			public ActionResult Stretch( string path, int width, int height ) { return _ui.Pictures.ServeStretched( _domain, path, width, height ); }
			public ActionResult ScaleW( string path, int width ) { return _ui.Pictures.ServeScaled( _domain, path, width, null ); }
			public ActionResult ScaleH( string path, int height ) { return _ui.Pictures.ServeScaled( _domain, path, null, height ); }

			public IJsonResponse<IEnumerable<object>> GetAll() {
				return JsonResponse.Catch( () => _ui
					.Files.GetAll( _domain )
					.Select( f => new { f.Id, f.FilePath, f.Data.ContentType } )
					.AsEnumerable()
					.Select( f => AttachmentJson( f.Id, f.FilePath, f.ContentType ) ), _ui.Log );
			}

			[EditPermission, HttpPost]
			public IJsonResponse<IEnumerable<object>> Upload( string pathPrefix, HttpPostedFileBase[] file ) {
				return JsonResponse.Catch( () => {
					var res = _ui.Files.UploadFiles( _domain, pathPrefix, file ).ToList();
					_ui.UnitOfWork.Commit();
					return res.Select( f => AttachmentJson( f.Id, f.FilePath, f.Data.ContentType ) );
				}, _ui.Log );
			}

			private object AttachmentJson( int fileId, string path, string contentType ) {
				var pic = IsPicture( contentType );

				return new {
					Module = "BL/attachment",
					Class = pic ? "Picture" : "File",
					Arguments = pic
						? (object)new {
							FileId = fileId,
							Path = path,
							Download = Url( c => c.Serve( path ) ),
							SmallThumb = Url( c => c.Crop( path, 50, 50 ) ),
							BigThumb = Url( c => c.Crop( path, 150, 150 ) )
						}
						: (object)new {
							FileId = fileId,
							Path = path,
							Download = Url( c => c.Download( path ) )
						}
				};
			}

			bool IsPicture( string contentType ) { return contentType != null && contentType.StartsWith( "image/" ); }

			IMvcMixinContext _context;
			public void SetContext( IMvcMixinContext ctx ) { _context = ctx; }
			string Url( Expression<Func<Mixin, object>> action ) {
				return _context.GetMyActionUrl( action );
			}
		}

		public MarkupNodeDefinition<MarkupParseArgs> BBImageTag() {
			return new MarkupParser<MarkupParseArgs>().ComplexTag( "img", false, new[] { "", "width", "height" },
				( ctx, attrs, inners ) => {
					var width = attrs.ValueOrDefault( "width" ).ToIntOrNull();
					var height = attrs.ValueOrDefault( "height" ).ToIntOrNull();
					var path = attrs.ValueOrDefault( "" );
					var url =
						width == null && height == null ? ctx.AttachmentMixin.Action( m => m.Img( path ) ) :
						width != null ? ctx.AttachmentMixin.Action( m => m.ScaleW( path, width.Value ) ) :
						/* height != null */ ctx.AttachmentMixin.Action( m => m.ScaleH( path, height.Value ) );

					return string.Format( "<img src='{0}' />", url );
				} );
		}

		static readonly ISet<string> _yes = new HashSet<string>( StringComparer.InvariantCultureIgnoreCase ) {
			"yes", "1", "true", "да"
		};

		public MarkupNodeDefinition<MarkupParseArgs> BBFileTag() {
			return new MarkupParser<MarkupParseArgs>().ComplexTag( "file", true, new[] { "", "download", "window", "tab" },
				( ctx, attrs ) => {
					var file = attrs.ValueOrDefault( "" );
					var download = _yes.Contains( attrs.ValueOrDefault( "download" ) ?? "" );
					var newWindow = 
						string.Equals( attrs.ValueOrDefault( "window" ), "new", StringComparison.InvariantCultureIgnoreCase ) ||
						string.Equals( attrs.ValueOrDefault( "tab" ), "new", StringComparison.InvariantCultureIgnoreCase );

					return new Range<string> {
						
						Start = string.Format( "<a class='file-download-link' href='{0}'{1}>",
							download 
								? ctx.AttachmentMixin.Action( m => m.Download( file ) )
								: ctx.AttachmentMixin.Action( m => m.Serve( file ) ),
							newWindow ? " target='_new'" : "" ),

						End = "</a>"
					
					};
				} );
		}
	}
}