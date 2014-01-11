using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using erecruit.Composition;
using erecruit.Mvc;
using erecruit.Utils;
using Mut.Data;
using Mut.Models;
using Mut.UI;
using Mut.Web;

namespace Mut.Controllers
{
	public class FeedbackController : Controller
	{
		[Import] public EmailService Emails { get; set; }
	}
}