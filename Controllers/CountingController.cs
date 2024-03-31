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

        #region POSTAL VOTES ENTRY
        public IActionResult PostalVotes()
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
        public IActionResult PostalVotes(VotesModel md)
        {
            if (HttpContext.Session.GetString("electionType").IsNullOrEmpty())
                return RedirectToAction("Login", "Home");
            md.panMun = HttpContext.Session.GetString("electionType");
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
            else if (md.constType == "5")
            {
                qry = "SELECT CONST_CODE,CONST_NAME FROM CONSTITUENCY WHERE TYPE_CODE LIKE " + md.constType;
                qry += " ORDER BY CONST_NO";
            }
            ds = dm.create_dataset(qry);
            ViewBag.constituencies = ds;
            if (md.panMun == "P")
            {
                if (md.constType == "5")
                {
                    qry = "SELECT PSCODE,PS_NAME FROM POLLING_STATION AS PS JOIN CONSTITUENCY AS C ON PS.CONST_NO=C.CONST_NO ";
                    qry += "AND PS.PAN_MUN=C.PAN_MUN AND C.PAN_MUN='" + md.panMun + "' AND ZILLA_CODE=" + md.constName;
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
            }
            else if (md.panMun == "M")
            {
                qry = "SELECT PSCODE,PS_NAME FROM POLLING_STATION AS PS JOIN CONSTITUENCY AS C ON PS.CONST_NO=C.CONST_NO ";
                qry += "AND PS.PAN_MUN=C.PAN_MUN AND C.PAN_MUN='" + md.panMun + "' WHERE C.CONST_NO=(SELECT CONST_NO FROM CONSTITUENCY WHERE ";
                qry += "CONST_CODE=" + md.constName + ")";
            }
            ds = dm.create_dataset(qry);
            ViewBag.pollingStations = ds;
            qry = "SELECT COUNT(*) FROM POLLING_STATION WHERE PAN_MUN='" + md.panMun + "'";
            ViewBag.psCnt = dm.create_scalar(qry);
            if (md.postCause=="btnFetch")
            {
                int psNo, constCode=0;
                if (md.mode == "A")
                {
                    psNo = md.psNo;
                }
                else
                {
                    psNo = int.Parse(md.pollingStation);
                }
                if (md.constType=="1")
                {
                    constCode = int.Parse(md.constType);
                }
                else if (md.constType=="2" || md.constType=="3")
                {
                    qry = "SELECT CAST(CONST_CODE AS INT) FROM CONSTITUENCY WHERE TYPE_CODE=" + md.constType + " AND CONST_NO=(SELECT ";
                    qry += "PCODE FROM CONSTITUENCY WHERE TYPE_CODE=1 AND CONST_NO=" + psNo + ")";
                    constCode = dm.create_scalar(qry);
                }
                qry = "SELECT C.CID,C.CAND_SL_NO,C.CAND_NAME,C.CAND_NAME_V1,CASE C.GENDER WHEN 'M' THEN 'Male' WHEN 'F' THEN ";
                qry += "'Female' WHEN 'T' THEN 'Third Gender' END AS GENDER,CASE INDEPENDENT WHEN 1 THEN 'INDEPENDENT' ELSE ";
                qry += "P.SHORT_NAME END AS PANAME, S.SYMBOL FROM NOMINATIONS AS C LEFT JOIN PARTY AS P ON C.PACODE = P.PACODE ";
                qry += "LEFT JOIN SYMBOLS AS S ON (P.SID = S.SID OR C.SID=S.SID) WHERE CONST_CODE=" + constCode + " ORDER BY CAND_SL_NO";
                ds = dm.create_dataset(qry);
                ViewBag.candidateList = ds;
            }
            return View(md);
        }

        
        #endregion

    }
}
