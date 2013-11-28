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
			public ActionResult Serve( string path ) { return _ui.Files.ServeFile( _domain, path, true ); }
			public ActionResult Img( string path ) { return _ui.Files.ServeFile( _domain, path, false ); }
			public ActionResult Crop( string path, int width, int height ) { return _ui.Pictures.ServeCropped( _domain, path, width, height ); }
			public ActionResult Stretch( string path, int width, int height ) { return _ui.Pictures.ServeStretched( _domain, path, width, height ); }
			public ActionResult ScaleW( string path, int width ) { return _ui.Pictures.ServeScaled( _domain, path, width, null ); }
			public ActionResult ScaleH( string path, int height ) { return _ui.Pictures.ServeScaled( _domain, path, null, height ); }

			public IJsonResponse<IEnumerable<object>> GetAll() {
				return JsonResponse.Catch( () => _ui
					.Files.GetAll( _domain )
					.Select( f => new { f.FilePath, f.Data.ContentType } )
					.AsEnumerable()
					.Select( f => AttachmentJson( f.FilePath, f.ContentType ) ), _ui.Log );
			}

			[EditPermission, HttpPost]
			public IJsonResponse<IEnumerable<object>> Upload( string pathPrefix, HttpPostedFileBase[] file ) {
				return JsonResponse.Catch( () => {
					var res = _ui.Files.UploadFiles( _domain, pathPrefix, file ).ToList();
					_ui.UnitOfWork.Commit();
					return res.Select( f => AttachmentJson( f.FilePath, f.Data.ContentType ) );
				}, _ui.Log );
			}

			private object AttachmentJson( string path, string contentType ) {
				var pic = IsPicture( contentType );

				return new {
					Module = "BL/attachment",
					Class = pic ? "Picture" : "File",
					Arguments = pic
						? (object)new {
							Path = path,
							Download = Url( c => c.Serve( path ) ),
							SmallThumb = Url( c => c.Crop( path, 50, 50 ) ),
							BigThumb = Url( c => c.Crop( path, 150, 150 ) )
						}
						: (object)new {
							Path = path,
							Download = Url( c => c.Serve( path ) )
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

		public static class BB
		{
			public static string Image( BBParseContext ctx ) {
				var width = ctx.Node.AttrValue( "width" ).ToIntOrNull();
				var height = ctx.Node.AttrValue( "height" ).ToIntOrNull();
				var path = ctx.Node.AttrValue( "src" );
				var url =
					width == null && height == null ? ctx.Args.AttachmentMixin.Action( m => m.Img( path ) ) :
					width != null ? ctx.Args.AttachmentMixin.Action( m => m.ScaleW( path, width.Value ) ) :
					/* height != null */ ctx.Args.AttachmentMixin.Action( m => m.ScaleH( path, height.Value ) );

				return string.Format( "<img src='{0}' />", url, innerText( ctx.Node ) );
			}

			public static string File( BBParseContext ctx ) {
				return string.Format( "<a class='file-download-link' href='{0}'>{1}</a>", path( ctx, "path" ), innerText( ctx.Node ) );
			}

			static string innerText( TagNode node ) {
				return HttpUtility.HtmlEncode( string.Join( "", node.SubNodes.Select( n => n.ToBBCode() ) ) );
			}

			static string path( BBParseContext ctx, string pathAttr ) {
				return ctx.Args.AttachmentMixin.Action( m => m.Serve( ctx.Node.AttrValue( pathAttr ) ) );
			}
		}
	}
}