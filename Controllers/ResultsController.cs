using Microsoft.AspNetCore.Mvc;
using SEMS.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using SEMS.Models.Counting;
using Microsoft.IdentityModel.Tokens;
using System.Reflection.Metadata;


namespace SEMS.Controllers
{
    public class ResultsController : Controller
    {

        string qry;
        //static string constr = "";
        DataSet ds = new DataSet();
        SqlConnection con = new SqlConnection();
        private readonly ElectionDBContext myDbContext;
        private readonly ILogger<ResultsController> _logger;
        private readonly IConfiguration configuration;
        RevisionModel md = new RevisionModel();
        DataManager dm = new DataManager(HomeController.constr);
        public ResultsController(ILogger<ResultsController> logger, ElectionDBContext myDbContext, IConfiguration configuration)
        {
            _logger = logger;
            this.myDbContext = myDbContext;
        }
        #region VIEW RESULTS
        public IActionResult DisplayResults()
        {
            VotesModel md = new VotesModel();
            if (HttpContext.Session.GetString("electionType").IsNullOrEmpty())
                return RedirectToAction("Login", "Home");
            md.panMun = HttpContext.Session.GetString("electionType");
            md.mode = "A";
            qry = "SELECT TYPE_CODE,TYPE_NAME FROM CONST_TYPE_MASTER WHERE PAN_MUN='" + md.panMun + "'";
            ds = dm.create_dataset(qry);
            ViewBag.constTypes = ds;
            md.constType = ds.Tables[0].Rows[0][0].ToString();
            md.constType = ds.Tables[0].Rows[0]["TYPE_CODE"].ToString();
            qry = "SELECT DIST_CODE,DIST_NAME FROM DISTRICT ORDER BY DIST_CODE";
            ds = dm.create_dataset(qry);
            ViewBag.districts = ds;
            qry = "SELECT TCODE,TNAME FROM TEHSIL WHERE E_ROLL=1 ORDER BY TNAME";
            ds = dm.create_dataset(qry);
            ViewBag.tehsils = ds;
            md.tehsil = ds.Tables[0].Rows[0][0].ToString();
            qry = "SELECT PNO,PAN_NAME FROM PANCHAYAT WHERE TCODE=" + md.tehsil + " ORDER BY PAN_NAME";
            ds = dm.create_dataset(qry);
            ViewBag.panchayats = ds;
            md.panchayat = ds.Tables[0].Rows[0][0].ToString();
            if (md.panMun == "P")
            {
                qry = "SELECT CONST_CODE,CONST_NAME FROM CONSTITUENCY WHERE TYPE_CODE LIKE " + md.constType;
                qry += " AND PCODE=" + md.panchayat + " ORDER BY CONST_NAME";
            }
            else
            {
                qry = "SELECT CONST_CODE,CONST_NAME FROM CONSTITUENCY WHERE TYPE_CODE LIKE " + md.constType;
                qry += " AND PAN_MUN='M' ORDER BY CONST_NAME";
            }

            ds = dm.create_dataset(qry);
            ViewBag.constituencies = ds;
            md.constName = "1";
            qry = "SELECT PSCODE,PS_NAME FROM POLLING_STATION AS PS JOIN CONSTITUENCY AS C ON PS.CONST_NO=C.CONST_NO ";
            qry += "AND PS.PAN_MUN=C.PAN_MUN AND C.PAN_MUN='" + md.panMun + "' WHERE C.CONST_NO=(SELECT CONST_NO FROM CONSTITUENCY WHERE ";
            qry += "CONST_CODE=" + md.constName + ")";
            ds = dm.create_dataset(qry);
            ViewBag.pollingStations = ds;
            qry = "SELECT COUNT(*) FROM POLLING_STATION WHERE PAN_MUN='" + md.panMun + "'";
            ViewBag.psCnt = dm.create_scalar(qry);
            return View(md);

        }

        [HttpPost]
        public IActionResult DisplayResults(VotesModel md)
        {
            if (HttpContext.Session.GetString("electionType").IsNullOrEmpty())
                return RedirectToAction("Login", "Home");
            qry = "SELECT TYPE_CODE,TYPE_NAME FROM CONST_TYPE_MASTER WHERE PAN_MUN='" + md.panMun + "' ORDER BY TYPE_NAME";
            ds = dm.create_dataset(qry);
            ViewBag.constTypes = ds;
            qry = "SELECT DIST_CODE,DIST_NAME FROM DISTRICT ORDER BY DIST_CODE";
            ds = dm.create_dataset(qry);
            ViewBag.districts = ds;
            qry = "SELECT TCODE,TNAME FROM TEHSIL WHERE E_ROLL=1 ORDER BY TNAME";
            ds = dm.create_dataset(qry);
            ViewBag.tehsils = ds;
            qry = "SELECT PNO,PAN_NAME FROM PANCHAYAT WHERE TCODE=" + md.tehsil + " ORDER BY PAN_NAME";
            ds = dm.create_dataset(qry);
            ViewBag.panchayats = ds;
            if (md.panMun == "P")
            {
                if (md.constType == "1")
                {
                    qry = "SELECT CONST_CODE,CONST_NAME FROM CONSTITUENCY WHERE TYPE_CODE LIKE " + md.constType;
                    qry += " AND PCODE=" + md.panchayat + " ORDER BY CONST_NAME";
                }
                else if (md.constType == "2")
                {
                    qry = "SELECT CONST_CODE,CONST_NAME FROM CONSTITUENCY WHERE TYPE_CODE LIKE " + md.constType;
                    qry += " AND TCODE=" + md.tehsil + " ORDER BY CONST_NAME";
                }
                else if (md.constType == "3")
                {
                    qry = "SELECT CONST_CODE,CONST_NAME FROM CONSTITUENCY WHERE TYPE_CODE LIKE " + md.constType;
                    qry += " AND TCODE=" + md.tehsil + " ORDER BY CONST_NAME";
                }
                else if (md.constType == "4")
                {
                    qry = "SELECT CONST_CODE,CONST_NAME FROM CONSTITUENCY WHERE TYPE_CODE LIKE " + md.constType;
                    qry += " AND ZILLA_CODE=" + md.district + " ORDER BY CONST_NAME";
                }
                ds = dm.create_dataset(qry);
                ViewBag.constituencies = ds;
                if (md.constType == "4")
                {
                    qry = "SELECT PSCODE,PS_NAME FROM POLLING_STATION AS PS JOIN CONSTITUENCY AS C ON PS.CONST_NO=C.CONST_NO ";
                    qry += "AND PS.PAN_MUN=C.PAN_MUN WHERE C.PAN_MUN='" + md.panMun + "' AND ZILLA_CODE=(SELECT CONST_NO FROM ";
                    qry += "CONSTITUENCY WHERE CONST_CODE=" + md.constName + ")";
                }
                else if (md.constType == "1")
                {
                    qry = "SELECT PSCODE,PS_NAME FROM POLLING_STATION AS PS JOIN CONSTITUENCY AS C ON PS.CONST_NO=C.CONST_NO ";
                    qry += "AND PS.PAN_MUN=C.PAN_MUN AND C.PAN_MUN='" + md.panMun + "' AND PCODE=" + md.panchayat;
                }
                else
                {
                    qry = "SELECT PSCODE,PS_NAME FROM POLLING_STATION AS PS JOIN CONSTITUENCY AS C ON PS.CONST_NO=C.CONST_NO ";
                    qry += "AND PS.PAN_MUN=C.PAN_MUN AND C.PAN_MUN='" + md.panMun + "' AND PCODE=(SELECT CONST_NO FROM CONSTITUENCY WHERE CONST_CODE=" + md.constName + ")";
                }
                ds = dm.create_dataset(qry);
                ViewBag.pollingStations = ds;
            }
            else if (md.panMun == "M")
            {
                qry = "SELECT CONST_CODE,CONST_NAME FROM CONSTITUENCY WHERE TYPE_CODE LIKE " + md.constType;
                qry += " ORDER BY CONST_NO";
                ds = dm.create_dataset(qry);
                ViewBag.constituencies = ds;
                qry = "SELECT PSCODE,PS_NAME FROM POLLING_STATION AS PS JOIN CONSTITUENCY AS C ON PS.CONST_NO=C.CONST_NO ";
                qry += "AND PS.PAN_MUN=C.PAN_MUN AND C.PAN_MUN='" + md.panMun + "' WHERE C.CONST_NO=(SELECT CONST_NO FROM CONSTITUENCY WHERE ";
                qry += "CONST_CODE=" + md.constName + ")";
                ds = dm.create_dataset(qry);
                ViewBag.pollingStations = ds;
            }
            else
            {
                return RedirectToAction("Login", "Home");
            }
            qry = "SELECT COUNT(*) FROM POLLING_STATION WHERE PAN_MUN='" + md.panMun + "'";
            ViewBag.psCnt = dm.create_scalar(qry);
            if (md.postCause == "btnFetch")
            {
                int psNo, constCode = 0;
                if (md.mode == "A")
                {
                    psNo = md.psNo;
                    qry = "SELECT CAST(PSCODE AS INT) FROM POLLING_STATION WHERE PAN_MUN='" + md.panMun + "' AND PS_NO=" + md.psNo;
                    md.pollingStation = dm.create_scalar(qry).ToString();
                    if (md.panMun == "P")
                    {
                        if (md.constType == "1")
                        {
                            qry = "SELECT CAST(CONST_CODE AS INT) FROM CONSTITUENCY WHERE TYPE_CODE=" + md.constType + " AND CONST_NO=(SELECT ";
                            qry += "CONST_NO FROM POLLING_STATION WHERE PAN_MUN='P' AND PS_NO=" + psNo + ")";
                        }
                        else if (md.constType == "2" || md.constType == "3")
                        {
                            qry = "SELECT CAST(CONST_CODE AS INT) FROM CONSTITUENCY WHERE TYPE_CODE=" + md.constType + " AND CONST_NO=(SELECT ";
                            qry += "PCODE FROM CONSTITUENCY WHERE TYPE_CODE=1 AND CONST_NO=" + psNo + ")";
                        }
                        else if (md.constType == "4")
                        {
                            qry = "SELECT CAST(CONST_CODE AS INT) FROM CONSTITUENCY WHERE TYPE_CODE=" + md.constType + " AND CONST_NO=(SELECT ";
                            qry += "ZILLA_CODE FROM CONSTITUENCY WHERE TYPE_CODE=1 AND CONST_NO=" + psNo + ")";
                        }


                    }
                    else if (md.panMun == "M")
                    {
                        qry = "SELECT CAST(CONST_CODE AS INT) FROM CONSTITUENCY WHERE TYPE_CODE=" + md.constType + " AND CONST_NO=(SELECT ";
                        qry += "CONST_NO FROM POLLING_STATION WHERE PAN_MUN='M' AND PS_NO=" + psNo + ")";

                    }
                    else
                    {
                        return RedirectToAction("Login", "Home");
                    }
                    md.constName = dm.create_scalar(qry).ToString();
                }
                else
                {
                    qry = "SELECT CAST(PS_NO AS INT) FROM POLLING_STATION WHERE PSCODE=" + md.pollingStation;
                    psNo = dm.create_scalar(qry);
                    if (md.constType == "1")
                    {

                        qry = "SELECT CAST(CONST_CODE AS INT) FROM CONSTITUENCY WHERE TYPE_CODE=" + md.constType + " AND CONST_NO=(SELECT ";
                        qry += "CONST_NO FROM POLLING_STATION WHERE PAN_MUN='P' AND PS_NO=" + psNo + ")";
                        md.constName = dm.create_scalar(qry).ToString();
                    }

                }
               
            }
            return View(md);
        }
        #endregion

        #region PROGRESS REPORT
        public IActionResult ProgressReport()
        {
            VotesModel md = new VotesModel();
            if (HttpContext.Session.GetString("electionType").IsNullOrEmpty())
                return RedirectToAction("Login", "Home");
            md.panMun = HttpContext.Session.GetString("electionType");
            qry = "SELECT TYPE_CODE,TYPE_NAME FROM CONST_TYPE_MASTER WHERE PAN_MUN='" + md.panMun + "' AND TYPE_CODE<>1";
            ds = dm.create_dataset(qry);
            ViewBag.constTypes = ds;
            md.constType = ds.Tables[0].Rows[0][0].ToString();
            qry = "SELECT TCODE,TNAME FROM TEHSIL WHERE E_ROLL=1 ORDER BY TNAME";
            ds = dm.create_dataset(qry);
            ViewBag.tehsils = ds;
            md.tehsil = ds.Tables[0].Rows[0][0].ToString();
            qry = "SELECT CONST_CODE,CONST_NAME FROM CONSTITUENCY WHERE TYPE_CODE LIKE " + md.constType;
            qry += " AND TCODE=" + md.tehsil + " ORDER BY CONST_NAME";
            ds = dm.create_dataset(qry);
            ViewBag.constituencies = ds;
            return View(md);
            
        }
        [HttpPost]
        public IActionResult ProgressReport(VotesModel md)
        {
            
            if (HttpContext.Session.GetString("electionType").IsNullOrEmpty())
                return RedirectToAction("Login", "Home");
            md.panMun = HttpContext.Session.GetString("electionType");
            qry = "SELECT TYPE_CODE,TYPE_NAME FROM CONST_TYPE_MASTER WHERE PAN_MUN='" + md.panMun + "' AND TYPE_CODE<>1";
            ds = dm.create_dataset(qry);
            ViewBag.constTypes = ds;
           
            qry = "SELECT TCODE,TNAME FROM TEHSIL WHERE E_ROLL=1 ORDER BY TNAME";
            ds = dm.create_dataset(qry);
            ViewBag.tehsils = ds;
            
            qry = "SELECT CONST_CODE,CONST_NAME FROM CONSTITUENCY WHERE TYPE_CODE LIKE " + md.constType;
            qry += " AND TCODE=" + md.tehsil + " ORDER BY CONST_NAME";
            ds = dm.create_dataset(qry);
            ViewBag.constituencies = ds;
            return View(md);

        }

        public IActionResult Form20()
        {
            VotesModel md = new VotesModel();
            if (HttpContext.Session.GetString("electionType").IsNullOrEmpty())
                return RedirectToAction("Login", "Home");
            md.panMun = HttpContext.Session.GetString("electionType");
            qry = "SELECT TYPE_CODE,TYPE_NAME FROM CONST_TYPE_MASTER WHERE PAN_MUN='" + md.panMun + "' ORDER BY TYPE_NAME";
            ds = dm.create_dataset(qry);
            ViewBag.constTypes = ds;
            md.constType = ds.Tables[0].Rows[0][0].ToString();
            qry = "SELECT TCODE,TNAME FROM TEHSIL WHERE E_ROLL=1 ORDER BY TNAME";
            ds = dm.create_dataset(qry);
            ViewBag.tehsils = ds;
            md.tehsil = ds.Tables[0].Rows[0][0].ToString();
            qry = "SELECT CONST_CODE,CONST_NAME FROM CONSTITUENCY WHERE TYPE_CODE LIKE " + md.constType;
            qry += " AND TCODE=" + md.tehsil + " ORDER BY CONST_NAME";
            ds = dm.create_dataset(qry);
            ViewBag.constituencies = ds;
            qry = "SELECT PNO,PAN_NAME FROM PANCHAYAT WHERE TCODE=" + md.tehsil + " ORDER BY PAN_NAME";
            ds = dm.create_dataset(qry);
            ViewBag.panchayats = ds;
            return View(md);
        }
        [HttpPost]
        public IActionResult Form20(VotesModel md)
        {
            
            if (HttpContext.Session.GetString("electionType").IsNullOrEmpty())
                return RedirectToAction("Login", "Home");
            md.panMun = HttpContext.Session.GetString("electionType");
            qry = "SELECT TYPE_CODE,TYPE_NAME FROM CONST_TYPE_MASTER WHERE PAN_MUN='" + md.panMun + "' ORDER BY TYPE_NAME";
            ds = dm.create_dataset(qry);
            ViewBag.constTypes = ds;
            
            qry = "SELECT TCODE,TNAME FROM TEHSIL WHERE E_ROLL=1 ORDER BY TNAME";
            ds = dm.create_dataset(qry);
            ViewBag.tehsils = ds;
           
            qry = "SELECT CONST_CODE,CONST_NAME FROM CONSTITUENCY WHERE TYPE_CODE LIKE " + md.constType;
            qry += " AND TCODE=" + md.tehsil + " ORDER BY CONST_NAME";
            ds = dm.create_dataset(qry);
            ViewBag.constituencies = ds;
            qry = "SELECT PNO,PAN_NAME FROM PANCHAYAT WHERE TCODE=" + md.tehsil + " ORDER BY PAN_NAME";
            ds = dm.create_dataset(qry);
            ViewBag.panchayats = ds;
            return View(md);
        }

        #endregion
    }
}

