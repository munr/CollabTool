using System.Web.Mvc;
using CollabTool.Web.Components;

namespace CollabTool.Web.Controllers
{
	[RequiresAuthentication]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}