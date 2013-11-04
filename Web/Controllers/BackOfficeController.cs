using System;
using System.Linq;
using System.Web.Mvc;
using erecruit.Composition;
using Mut.Data;
using Mut.Models;
using Mut.UI;

namespace Mut.Controllers
{
	[EditPermission]
	public class BackOfficeController : Controller
	{
		public ActionResult Index()
		{
			return View();
		}
	}
}