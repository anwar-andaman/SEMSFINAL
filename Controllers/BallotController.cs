using Microsoft.AspNetCore.Mvc;
using SEMS.Models;
using System.Data;
using Microsoft.Data.SqlClient;
using SEMS.Models.Ballot;
namespace SEMS.Controllers
{
    public class BallotController : Controller
    {
        string qry;
        //static string constr = "";
        DataSet ds = new DataSet();
        SqlConnection con = new SqlConnection();
        private readonly ElectionDBContext myDbContext;
        private readonly ILogger<BallotController> _logger;
        private readonly IConfiguration configuration;
        DataManager dm = new DataManager(HomeController.constr);
        public BallotController(ILogger<BallotController> logger, ElectionDBContext myDbContext, IConfiguration configuration)
        {
            _logger = logger;
            this.myDbContext = myDbContext;

        }
        public IActionResult ManageParties()
        {
            int cnt;
            PolPartyModel md = new PolPartyModel();
            qry = "SELECT COUNT(*) FROM PARTY";
            cnt = dm.create_scalar(qry);
            ViewBag.partyCount = cnt;
            return View(md);
        }
        [HttpPost]
        public IActionResult ManageParties(PolPartyModel md)
        {
            int cnt;
            qry = "SELECT COUNT(*) FROM PARTY";
            cnt = dm.create_scalar(qry);
            ViewBag.partyCount = cnt;
            return View(md);
        }
    }
}
