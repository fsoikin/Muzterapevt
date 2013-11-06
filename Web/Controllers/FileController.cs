using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using erecruit.Composition;
using Mut.Models;
using Mut.UI;
using Mut.Web;
using Name.Files;

namespace Mut.Controllers
{
	public class FileController : Controller
	{
		[Import] public IFileUI Files { get; set; }

		public ActionResult PageAttachment( int pageId, string path ) { return Files.ServeFile( PagesService.AttachmentDomain( pageId ), path ); }
		public ActionResult TextAttachment( int textId, string path ) { return Files.ServeFile( TextService.AttachmentDomain( textId ), path ); }
		public ActionResult Serve( string path ) { return Files.ServeFile( FileDomain.General, path ); }

		[EditPermission, HttpPost]
		public IJsonResponse<IEnumerable<string>> Upload( string pathPrefix, HttpPostedFileBase[] files ) {
			return UploadImpl( FileDomain.General, pathPrefix, files, f => Url.Action( (FileController c) => c.Serve( f.FilePath ) ) );
		}

		[EditPermission, HttpPost]
		public IJsonResponse<IEnumerable<string>> UploadPageAttachment( int pageId, string pathPrefix, HttpPostedFileBase[] files ) {
			return UploadImpl( PagesService.AttachmentDomain( pageId ), pathPrefix, files, 
				f => Url.Action( ( FileController c ) => c.PageAttachment( pageId, f.FilePath ) ) );
		}

		[EditPermission, HttpPost]
		public IJsonResponse<IEnumerable<string>> UploadTextAttachment( int textId, string pathPrefix, HttpPostedFileBase[] files ) {
			return UploadImpl( TextService.AttachmentDomain( textId ), pathPrefix, files, 
				f => Url.Action( ( FileController c ) => c.TextAttachment( textId, f.FilePath ) ) );
		}

		private IJsonResponse<IEnumerable<string>> UploadImpl( string domain, string pathPrefix, HttpPostedFileBase[] files, Func<File,string> getPath ) {
			return JsonResponse.Catch( () => {
				var res = Files.UploadFiles( domain, pathPrefix, files ).ToList();
				UnitOfWork.Commit();
				return res.Select( f => f.FilePath );
			},
				 Log );
		}
	}
}