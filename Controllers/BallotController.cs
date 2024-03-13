using Microsoft.AspNetCore.Mvc;

namespace SEMS.Controllers
{
    public class BallotController : Controller
    {
        public IActionResult ManageParties()
        {
            return View();
        }
    }
}
