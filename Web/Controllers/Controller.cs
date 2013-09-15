using System;
using System.Linq;
using System.Web.Mvc;
using erecruit.Composition;
using log4net;
using Mut.Data;
using Mut.Models;
using Mut.UI;

namespace Mut.Controllers
{
	public class Controller : System.Web.Mvc.Controller
	{
		[Import]
		public ILog Log { get; set; }
	}
}