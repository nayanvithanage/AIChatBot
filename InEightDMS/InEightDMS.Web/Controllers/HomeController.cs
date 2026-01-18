using System.Web.Mvc;

namespace InEightDMS.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Projects");
            }
            return View();
        }
    }
}
