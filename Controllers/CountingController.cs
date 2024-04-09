using Microsoft.AspNetCore.Mvc;
using SEMS.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using SEMS.Models.Counting;
using Microsoft.IdentityModel.Tokens;
using System.Reflection.Metadata;
namespace SEMS.Controllers
{
    public class CountingController : Controller
    {
        string qry;
        //static string constr = "";
        DataSet ds = new DataSet();
        SqlConnection con = new SqlConnection();
        private readonly ElectionDBContext myDbContext;
        private readonly ILogger<CountingController> _logger;
        private readonly IConfiguration configuration;
        RevisionModel md = new RevisionModel();
        DataManager dm = new DataManager(HomeController.constr);
        public CountingController(ILogger<CountingController> logger, ElectionDBContext myDbContext, IConfiguration configuration)
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
        #region POSTAL VOTES ENTRY
        public IActionResult PostalVotes()
        {
            bool allOK = checkAuthorization(19);
            if (!allOK)
            {
                HttpContext.Session.SetString("errorMessage", "You are not authorized to access this page. Please contact Administrator.......");
                return RedirectToAction("AuthorizationError", "Home");
            }
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
        public IActionResult PostalVotes(VotesModel md)
        {
            bool allOK = checkAuthorization(19);
            if (!allOK)
            {
                HttpContext.Session.SetString("errorMessage", "You are not authorized to access this page. Please contact Administrator.......");
                return RedirectToAction("AuthorizationError", "Home");
            }
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

                qry = "SELECT CID FROM NOMINATIONS WHERE CONST_CODE=" + md.constName + " ORDER BY CAND_SL_NO";
                ds = dm.create_dataset(qry);
                qry = "SELECT COUNT(*) FROM PSWISE_VOTES WHERE CONST_CODE=" + md.constName + " AND PSCODE=";
                qry += md.pollingStation + " AND BALLOT_TYPE='P'";
                psNo = dm.create_scalar(qry);
                qry = "SELECT COUNT(*) FROM TENDERED_REJECTED WHERE CONST_CODE=" + md.constName + " AND PSCODE=";
                qry += md.pollingStation + " AND BALLOT_TYPE='P'";
                int rejCnt = dm.create_scalar(qry);
                if (psNo == 0 || rejCnt == 0)
                {
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
                        if (psNo == 0)
                        {
                            foreach (DataRow row in ds.Tables[0].Rows)
                            {
                                qry = "INSERT INTO PSWISE_VOTES(CONST_CODE,PSCODE,CID,BALLOT_TYPE,VOTES) VALUES(" + md.constName;
                                qry += "," + md.pollingStation + "," + row["CID"] + ",'P',0)";
                                dm.do_transaction(qry, ref con, t);
                            }
                        }
                        if (rejCnt == 0)
                        {
                            qry = "INSERT INTO TENDERED_REJECTED(CONST_CODE,PSCODE,BALLOT_TYPE,TENDERED,REJECTED) VALUES(";
                            qry += md.constName + "," + md.pollingStation + ",'P',0,0)";
                            dm.do_transaction(qry, ref con, t);
                        }
                        t.Commit();
                        con.Close();
                    }
                    catch (Exception ex)
                    {
                        t.Rollback();
                        con.Close();
                        TempData["message"] = "Could Not complete the Transaction. Please Try Again!";
                    }

                }
                qry = "SELECT C.CID,C.CAND_SL_NO,C.CAND_NAME,C.CAND_NAME_V1,CASE C.GENDER WHEN 'M' THEN 'Male' WHEN 'F' THEN ";
                qry += "'Female' WHEN 'T' THEN 'Third Gender' END AS GENDER,CASE INDEPENDENT WHEN 1 THEN 'INDEPENDENT' ELSE ";
                qry += "P.SHORT_NAME END AS PANAME, S.SYMBOL,CAST(PV.VOTES AS INT) AS VOTES FROM NOMINATIONS AS C JOIN PSWISE_VOTES AS PV ON PV.CID=C.CID ";
                qry += "AND PV.CONST_CODE=C.CONST_CODE AND PV.BALLOT_TYPE='P' LEFT JOIN PARTY AS P ON C.PACODE = P.PACODE ";
                qry += "LEFT JOIN SYMBOLS AS S ON (P.SID = S.SID OR C.SID=S.SID) WHERE C.CONST_CODE=" + md.constName;
                qry += " AND PV.PSCODE=" + md.pollingStation + " ORDER BY CAND_SL_NO";
                ds = dm.create_dataset(qry);
                int arrCnt = 0;
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    int votes = int.Parse(row["VOTES"].ToString());
                    md.votes[arrCnt++] = votes;
                }
                ViewBag.candidateList = ds;
                qry = "SELECT CAST(REJECTED AS INT) FROM TENDERED_REJECTED WHERE CONST_CODE=" + md.constName + " AND PSCODE=";
                qry += md.pollingStation + " AND BALLOT_TYPE='P'";
                ViewBag.rejVotes = dm.create_scalar(qry);
            }
            return View(md);
        }

        public IActionResult UpdatePostalVotes(VotesModel md)
        {
            bool allOK = checkAuthorization(19);
            if (!allOK)
            {
                HttpContext.Session.SetString("errorMessage", "You are not authorized to access this page. Please contact Administrator.......");
                return RedirectToAction("AuthorizationError", "Home");
            }
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
                        qry += "CONST_NO FROM POLLING_STATION WHERE PAN_MUN='P' AND PS_NO=" + psNo + ")"; ;
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
            qry = "SELECT CID FROM NOMINATIONS WHERE CONST_CODE=" + md.constName + " ORDER BY CAND_SL_NO";
            ds = dm.create_dataset(qry);
            int rowCnt = 1;
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                string numvotes = HttpContext.Request.Form["numVotes" + rowCnt];
                qry = "UPDATE PSWISE_VOTES SET VOTES=" + numvotes + " WHERE CONST_CODE=" + md.constName;
                qry += " AND PSCODE=" + md.pollingStation + " AND BALLOT_TYPE='P' AND CID=" + row["CID"];
                dm.runquery(qry);
                qry = "UPDATE TENDERED_REJECTED SET REJECTED=" + md.rejected + " WHERE CONST_CODE=" + md.constName;
                qry += " AND PSCODE=" + md.pollingStation + " AND BALLOT_TYPE='P'";
                dm.runquery(qry);
                rowCnt++;
            }
            return RedirectToActionPreserveMethod("PostalVotes");
        }
        #endregion

        #region NORMAL VOTES ENTRY
        public IActionResult NormalVotes()
        {
            bool allOK = checkAuthorization(19);
            if (!allOK)
            {
                HttpContext.Session.SetString("errorMessage", "You are not authorized to access this page. Please contact Administrator.......");
                return RedirectToAction("AuthorizationError", "Home");
            }
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
        public IActionResult NormalVotes(VotesModel md)
        {
            bool allOK = checkAuthorization(19);
            if (!allOK)
            {
                HttpContext.Session.SetString("errorMessage", "You are not authorized to access this page. Please contact Administrator.......");
                return RedirectToAction("AuthorizationError", "Home");
            }
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
                            qry += "CONST_NO FROM POLLING_STATION WHERE PAN_MUN='P' AND PS_NO=" + psNo + ")"; ;
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
                qry = "SELECT CID FROM NOMINATIONS WHERE CONST_CODE=" + md.constName + " ORDER BY CAND_SL_NO";
                ds = dm.create_dataset(qry);
                qry = "SELECT COUNT(*) FROM PSWISE_VOTES WHERE CONST_CODE=" + md.constName + " AND PSCODE=";
                qry += md.pollingStation + " AND BALLOT_TYPE='N'";
                psNo = dm.create_scalar(qry);
                qry = "SELECT COUNT(*) FROM TENDERED_REJECTED WHERE CONST_CODE=" + md.constName + " AND PSCODE=";
                qry += md.pollingStation + " AND BALLOT_TYPE='N'";
                int rejCnt = dm.create_scalar(qry);
                if (psNo == 0 || rejCnt == 0)
                {
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
                        if (psNo == 0)
                        {
                            foreach (DataRow row in ds.Tables[0].Rows)
                            {
                                qry = "INSERT INTO PSWISE_VOTES(CONST_CODE,PSCODE,CID,BALLOT_TYPE,VOTES) VALUES(" + md.constName;
                                qry += "," + md.pollingStation + "," + row["CID"] + ",'N',0)";
                                dm.do_transaction(qry, ref con, t);
                            }
                        }
                        if (rejCnt == 0)
                        {
                            qry = "INSERT INTO TENDERED_REJECTED(CONST_CODE,PSCODE,BALLOT_TYPE,TENDERED,REJECTED) VALUES(";
                            qry += md.constName + "," + md.pollingStation + ",'N',0,0)";
                            dm.do_transaction(qry, ref con, t);
                        }
                        t.Commit();
                        con.Close();
                    }
                    catch (Exception ex)
                    {
                        t.Rollback();
                        con.Close();
                        TempData["message"] = "Could Not complete the Transaction. Please Try Again!";
                    }

                }
                qry = "SELECT C.CID,C.CAND_SL_NO,C.CAND_NAME,C.CAND_NAME_V1,CASE C.GENDER WHEN 'M' THEN 'Male' WHEN 'F' THEN ";
                qry += "'Female' WHEN 'T' THEN 'Third Gender' END AS GENDER,CASE INDEPENDENT WHEN 1 THEN 'INDEPENDENT' ELSE ";
                qry += "P.SHORT_NAME END AS PANAME, S.SYMBOL,CAST(PV.VOTES AS INT) AS VOTES FROM NOMINATIONS AS C JOIN PSWISE_VOTES AS PV ON PV.CID=C.CID ";
                qry += "AND PV.CONST_CODE=C.CONST_CODE AND PV.BALLOT_TYPE='N' LEFT JOIN PARTY AS P ON C.PACODE = P.PACODE ";
                qry += "LEFT JOIN SYMBOLS AS S ON (P.SID = S.SID OR C.SID=S.SID) WHERE C.CONST_CODE=" + md.constName;
                qry += " AND PV.PSCODE=" + md.pollingStation + " ORDER BY CAND_SL_NO";
                ds = dm.create_dataset(qry);
                int arrCnt = 0;
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    int votes = int.Parse(row["VOTES"].ToString());
                    md.votes[arrCnt++] = votes;
                }
                ViewBag.candidateList = ds;
                qry = "SELECT CAST(REJECTED AS INT) AS REJECTED,CAST(TENDERED AS INT) AS TENDERED FROM TENDERED_REJECTED WHERE CONST_CODE=" + md.constName + " AND PSCODE=";
                qry += md.pollingStation + " AND BALLOT_TYPE='N'";
                ds = dm.create_dataset(qry);
                ViewBag.rejVotes = ds.Tables[0].Rows[0]["REJECTED"].ToString();
                ViewBag.tenderedVotes = ds.Tables[0].Rows[0]["TENDERED"].ToString();
            }
            return View(md);
        }

        public IActionResult UpdateNormalVotes(VotesModel md)
        {
            bool allOK = checkAuthorization(19);
            if (!allOK)
            {
                HttpContext.Session.SetString("errorMessage", "You are not authorized to access this page. Please contact Administrator.......");
                return RedirectToAction("AuthorizationError", "Home");
            }
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
                        qry += "CONST_NO FROM POLLING_STATION WHERE PAN_MUN='P' AND PS_NO=" + psNo + ")"; ;
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
            qry = "SELECT CID FROM NOMINATIONS WHERE CONST_CODE=" + md.constName + " ORDER BY CAND_SL_NO";
            ds = dm.create_dataset(qry);
            int rowCnt = 1;
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                string numvotes = HttpContext.Request.Form["numVotes" + rowCnt];
                qry = "UPDATE PSWISE_VOTES SET VOTES=" + numvotes + " WHERE CONST_CODE=" + md.constName;
                qry += " AND PSCODE=" + md.pollingStation + " AND BALLOT_TYPE='N' AND CID=" + row["CID"];
                dm.runquery(qry);
                qry = "UPDATE TENDERED_REJECTED SET REJECTED=" + md.rejected + ",TENDERED=" + md.tendered + " WHERE CONST_CODE=" + md.constName;
                qry += " AND PSCODE=" + md.pollingStation + " AND BALLOT_TYPE='N'";
                dm.runquery(qry);
                rowCnt++;
            }
            return RedirectToActionPreserveMethod("NormalVotes");
        }
        #endregion

    }   
}
