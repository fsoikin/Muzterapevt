using System;
using System.Linq;
using System.Web.Mvc;
using erecruit.Composition;
using Mut.Data;
using Mut.Models;
using Mut.UI;
using Name.Files;

namespace Mut.Controllers
{
	public class PictureController : Controller
	{
		[Import] public IPictureUI Pictures { get; set; }

		public ActionResult Crop( string path, int width, int height ) { return Pictures.ServeCropped( path, width, height ); }
		public ActionResult Stretch( string path, int width, int height ) { return Pictures.ServeStretched( path, width, height ); }
		public ActionResult ScaleW( string path, int width ) { return Pictures.ServeScaled( path, width, null ); }
		public ActionResult ScaleH( string path, int height ) { return Pictures.ServeScaled( path, null, height ); }
	}
}