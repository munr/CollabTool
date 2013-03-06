using System.Web.Mvc;
using CollabTool.Web.Components;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace CollabTool.Web.Controllers
{
	[RequiresAuthentication]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {            
            return View();
        }

        public ActionResult CollabTool()
        {
            return View();
        }
    }
}