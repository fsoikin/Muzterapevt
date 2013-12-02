using System.Web.Mvc;
using erecruit.Composition;
using log4net;
using Mut.UI;
using Mut.Web;

namespace Mut.Controllers
{
	[EditPermission]
	public class BBCodeController : Controller
	{
		[Import] public MarkupUI UI { get; set; }

		public IJsonResponse<string> ToHtml( string bbText )
		{
			return JsonResponse.Catch( () => 
				UI.ToHtml( bbText, new MarkupParseArgs() ), Log );
		}
	}
}