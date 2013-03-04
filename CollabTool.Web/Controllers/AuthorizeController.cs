using System.Web.Mvc;

namespace CollabTool.Web.Controllers
{
    public class AuthorizeController : Controller
    {
        public ActionResult Index(string code)
        {
            return RedirectToAction("Index", "Home");
        }
    }
}