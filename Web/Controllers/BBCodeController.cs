using System.Web.Mvc;
using erecruit.Composition;
using log4net;
using Mut.UI;
using Mut.Web;

namespace Mut.Controllers
{
	public class BBCodeController : Controller
	{
		[Import] public BBCodeUI UI { get; set; }

		public IJsonResponse<string> ToHtml( string bbText )
		{
			return JsonResponse.Catch( () => 
				UI.ToHtml( bbText ), Log );
		}
	}
}