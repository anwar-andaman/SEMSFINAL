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
        #region CHECK AUTHORIZATION
        public bool checkAuthorization(int fid)
        {
            bool allOK = true;
            string qry = "";
            byte[] logn;
            if (HttpContext.Session.TryGetValue("logUserType", out logn))
            {
                string logUserType = HttpContext.Session.GetString("logUserType");
                qry = "SELECT STATUS FROM USER_FUNCTION_MAPPING WHERE FUNCTION_ID=" + fid + " AND USER_TYPE_ID=";
                qry += "(SELECT TYPE_ID FROM USER_TYPE WHERE USER_TYPE='" + logUserType + "')";
                ds = dm.create_dataset(qry);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    if (!(bool)ds.Tables[0].Rows[0][0])
                        allOK = false;
                }
                else
                {
                    allOK = false;
                }
            }
            else
            {
                allOK = false;
            }
            return allOK;
        }
        #endregion

        

        #region PROGRESS REPORT
        public IActionResult ProgressReport()
        {
            bool allOK = checkAuthorization(22);
            if (!allOK)
            {
                HttpContext.Session.SetString("errorMessage", "You are not authorized to access this page. Please contact Administrator.......");
                return RedirectToAction("AuthorizationError", "Home");
            }
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
            bool allOK = checkAuthorization(22);
            if (!allOK)
            {
                HttpContext.Session.SetString("errorMessage", "You are not authorized to access this page. Please contact Administrator.......");
                return RedirectToAction("AuthorizationError", "Home");
            }
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
            bool allOK = checkAuthorization(23);
            if (!allOK)
            {
                HttpContext.Session.SetString("errorMessage", "You are not authorized to access this page. Please contact Administrator.......");
                return RedirectToAction("AuthorizationError", "Home");
            }
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
            bool allOK = checkAuthorization(23);
            if (!allOK)
            {
                HttpContext.Session.SetString("errorMessage", "You are not authorized to access this page. Please contact Administrator.......");
                return RedirectToAction("AuthorizationError", "Home");
            }
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

        #region LIST OF CONTESTING CANDIDATES
        public IActionResult ContestingCandidates()
        {
            bool allOK = checkAuthorization(24);
            if (!allOK)
            {
                HttpContext.Session.SetString("errorMessage", "You are not authorized to access this page. Please contact Administrator.......");
                return RedirectToAction("AuthorizationError", "Home");
            }
            if (HttpContext.Session.GetString("electionType").IsNullOrEmpty())
                return RedirectToAction("Login", "Home");
            qry = "SELECT TCODE,TNAME FROM TEHSIL WHERE E_ROLL=1 ORDER BY TNAME";
            ds = dm.create_dataset(qry);
            ViewBag.tehsils = ds;
            return View();
        }
        #endregion

        #region DECLARE RESULTS
        public IActionResult DeclareResult()
        {
            bool allOK = checkAuthorization(20);
            if (!allOK)
            {
                HttpContext.Session.SetString("errorMessage", "You are not authorized to access this page. Please contact Administrator.......");
                return RedirectToAction("AuthorizationError", "Home");
            }
            VotesModel md = new VotesModel();
            if (HttpContext.Session.GetString("electionType").IsNullOrEmpty())
                return RedirectToAction("Login", "Home");
            qry = "SELECT PASSWORD FROM USERS WHERE UID=" + HttpContext.Session.GetString("logUserID");
            ds = dm.create_dataset(qry);
            md.pwd = ds.Tables[0].Rows[0][0].ToString();
            md.panMun = HttpContext.Session.GetString("electionType");
            md.mode = "A";
            qry = "SELECT TYPE_CODE,TYPE_NAME FROM CONST_TYPE_MASTER WHERE PAN_MUN='" + md.panMun + "'";
            ds = dm.create_dataset(qry);
            ViewBag.constTypes = ds;
            md.constType = ds.Tables[0].Rows[0][0].ToString();
            md.constType = ds.Tables[0].Rows[0]["TYPE_CODE"].ToString();
            
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
            return View(md);

        }
        [HttpPost]
        public IActionResult DeclareResult(VotesModel md)
        {
            bool allOK = checkAuthorization(20);
            if (!allOK)
            {
                HttpContext.Session.SetString("errorMessage", "You are not authorized to access this page. Please contact Administrator.......");
                return RedirectToAction("AuthorizationError", "Home");
            }
            if (HttpContext.Session.GetString("electionType").IsNullOrEmpty())
                return RedirectToAction("Login", "Home");
            qry = "SELECT PASSWORD FROM USERS WHERE UID=" + HttpContext.Session.GetString("logUserID");
            ds = dm.create_dataset(qry);
            md.pwd = ds.Tables[0].Rows[0][0].ToString();
            qry = "SELECT TYPE_CODE,TYPE_NAME FROM CONST_TYPE_MASTER WHERE PAN_MUN='" + md.panMun + "' ORDER BY TYPE_NAME";
            ds = dm.create_dataset(qry);
            ViewBag.constTypes = ds;
            
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
                else 
                {
                    qry = "SELECT CONST_CODE,CONST_NAME FROM CONSTITUENCY WHERE TYPE_CODE LIKE " + md.constType;
                    qry += " AND TCODE=" + md.tehsil + " ORDER BY CONST_NAME";
                }
                
                ds = dm.create_dataset(qry);
                ViewBag.constituencies = ds;
               
            }
            else if (md.panMun == "M")
            {
                qry = "SELECT CONST_CODE,CONST_NAME FROM CONSTITUENCY WHERE TYPE_CODE LIKE " + md.constType;
                qry += " ORDER BY CONST_NO";
                ds = dm.create_dataset(qry);
                ViewBag.constituencies = ds;
                
            }
            else
            {
                return RedirectToAction("Login", "Home");
            }
            
            if (md.postCause == "btnFetch")
            {
                qry = "SELECT C.CID,C.CAND_SL_NO,C.CAND_NAME,C.CAND_NAME_V1,CASE C.GENDER WHEN 'M' THEN 'Male' WHEN 'F' THEN ";
                qry += "'Female' WHEN 'T' THEN 'Third Gender' END AS GENDER,CASE INDEPENDENT WHEN 1 THEN 'INDEPENDENT' ELSE ";
                qry += "P.SHORT_NAME END AS PANAME, S.SYMBOL,C.VOTES,C.WIN_STATUS FROM NOMINATIONS AS C  ";
                qry += " LEFT JOIN PARTY AS P ON C.PACODE = P.PACODE ";
                qry += "LEFT JOIN SYMBOLS AS S ON (P.SID = S.SID OR C.SID=S.SID) WHERE C.CONST_CODE=" + md.constName;
                qry += "  ORDER BY CAND_SL_NO";
                ds = dm.create_dataset(qry);
                ViewBag.candidateList = ds;
                qry = "SELECT CAST(MAX(VOTES) AS INT) AS MAXVOTES FROM NOMINATIONS WHERE CONST_CODE=" + md.constName;
                md.maxVotes = dm.create_scalar(qry);


            }
            return View(md);
        }

        public IActionResult DeclareWinner(VotesModel md)
        {
            bool allOK = checkAuthorization(20);
            if (!allOK)
            {
                HttpContext.Session.SetString("errorMessage", "You are not authorized to access this page. Please contact Administrator.......");
                return RedirectToAction("AuthorizationError", "Home");
            }
            SqlConnection con = new SqlConnection();
            try
            {
                dm.makeconnection(ref con);
            }
            catch (Exception ex)
            {
                TempData["message"] = "Connection to the Database Could not be established. Please try Again!";
            }
            SqlTransaction t = con.BeginTransaction();
            try
            {
                qry = "UPDATE NOMINATIONS SET WIN_STATUS='L' WHERE CONST_CODE=" + md.constName;
                dm.do_transaction(qry,ref con,t);
                qry = "UPDATE NOMINATIONS SET WIN_STATUS='W' WHERE CID=" + md.keyValue;
                dm.do_transaction(qry, ref con, t);
                t.Commit();
                con.Close();
            }
            catch (Exception ex)
            {
                t.Rollback();
                con.Close();
                TempData["message"] = "Could Not complete the Transaction. Please Try Again!";
            }


            return RedirectToActionPreserveMethod("DeclareResult");
        }

        public IActionResult RollbackWinner(VotesModel md)
        {
            bool allOK = checkAuthorization(20);
            if (!allOK)
            {
                HttpContext.Session.SetString("errorMessage", "You are not authorized to access this page. Please contact Administrator.......");
                return RedirectToAction("AuthorizationError", "Home");
            }
            qry = "UPDATE NOMINATIONS SET WIN_STATUS='C' WHERE CONST_CODE=" + md.constName;
            dm.runquery(qry);
            return RedirectToActionPreserveMethod("DeclareResult");
        }
        #endregion

        #region LIST OF WINNING CANDIDATES
        public IActionResult WinningCandidates()
        {
            bool allOK = checkAuthorization(24);
            if (!allOK)
            {
                HttpContext.Session.SetString("errorMessage", "You are not authorized to access this page. Please contact Administrator.......");
                return RedirectToAction("AuthorizationError", "Home");
            }
            if (HttpContext.Session.GetString("electionType").IsNullOrEmpty())
                return RedirectToAction("Login", "Home");
            qry = "SELECT TCODE,TNAME FROM TEHSIL WHERE E_ROLL=1 ORDER BY TNAME";
            ds = dm.create_dataset(qry);
            ViewBag.tehsils = ds;
            return View();
        }
        #endregion
    }
}

