using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using erecruit.Composition;
using erecruit.Mvc;
using erecruit.Mvc.Mixins;
using log4net;
using Mut.Data;
using Mut.Models;
using Mut.UI;
using Mut.Web;
using Name.Files;

namespace Mut.Controllers
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
			public ActionResult Serve( string path ) { return _ui.Files.ServeFile( _domain, path ); }
			public ActionResult Crop( string path, int width, int height ) { return _ui.Pictures.ServeCropped( _domain, path, width, height ); }
			public ActionResult Stretch( string path, int width, int height ) { return _ui.Pictures.ServeStretched( _domain, path, width, height ); }
			public ActionResult ScaleW( string path, int width ) { return _ui.Pictures.ServeScaled( _domain, path, width, null ); }
			public ActionResult ScaleH( string path, int height ) { return _ui.Pictures.ServeScaled( _domain, path, null, height ); }


			[EditPermission, HttpPost]
			public IJsonResponse<IEnumerable<object>> Upload( string pathPrefix, HttpPostedFileBase[] files ) {
				return JsonResponse.Catch( () => {
					var res = _ui.Files.UploadFiles( _domain, pathPrefix, files ).ToList();
					_ui.UnitOfWork.Commit();
					return res.Select( f => new {
						Module = "app/BL/attachment",
						Class = IsPicture( f.Data.ContentType ) ? "Picture" : "File",
						Arguments = IsPicture( f.Data.ContentType )
							? (object)new {
								Path = f.FilePath,
								Download = Url( c => c.Serve( f.FilePath ) ),
								SmallThumb = Url( c => c.Crop( f.FilePath, 50, 50 ) ),
								BigThumb = Url( c => c.Crop( f.FilePath, 150, 150 ) )
							}
							: (object)new {
								Path = f.FilePath,
								Download = Url( c => c.Serve( f.FilePath ) )
							}
					} );
				}, _ui.Log );
			}

			bool IsPicture( string contentType ) { return contentType != null && contentType.StartsWith( "image/" ); }

			IMvcMixinContext _context;
			public void SetContext( IMvcMixinContext ctx ) { _context = ctx; }
			string Url( Expression<Func<Mixin, object>> action ) {
				return _context.GetMyActionUrl( action );
			}
		}
	}
}