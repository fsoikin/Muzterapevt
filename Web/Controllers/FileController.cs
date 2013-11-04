using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using erecruit.Composition;
using Mut.Data;
using Mut.Models;
using Mut.UI;
using Name.Files;

namespace Mut.Controllers
{
	public class FileController : Controller
	{
		[Import] public IFileUI Files { get; set; }

		public ActionResult Serve( string path ) { return Files.ServeFile( path ); }

		[EditPermission]
		public ActionResult Upload( string pathPrefix, HttpPostedFileBase[] files ) { 
			Files.UploadFiles( pathPrefix, files ).LastOrDefault();
			UnitOfWork.Commit();
			return new EmptyResult();
		}
	}
}