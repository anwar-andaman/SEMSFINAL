using Microsoft.AspNetCore.Mvc;
using SEMS.Models;
using System.Data;
using Microsoft.Data.SqlClient;
using SEMS.Models.Ballot;
using Microsoft.CodeAnalysis.Elfie.Model;
using System.Reflection.Emit;
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
        #region MANAGE POLITICAL PARTIES
        public IActionResult ManageParties()
        {
            PolPartyModel md = new PolPartyModel();
            qry = "SELECT PACODE,PANAME,PANAME_V1,SHORT_NAME,SHORT_NAME_V1,S.SYMBOL,P.SID FROM PARTY AS P ";
            qry += "JOIN SYMBOLS AS S ON P.SID=S.SID ORDER BY PANAME";
            ds = dm.create_dataset(qry);
            ViewBag.party = ds;
            qry = "SELECT SID,SYMBOL_NAME FROM SYMBOLS WHERE SID NOT IN(SELECT SID FROM PARTY) ORDER BY SYMBOL_NAME";
            ds = dm.create_dataset(qry);
            ViewBag.symbols = ds;
            return View(md);
        }
        [HttpPost]
        public IActionResult ManageParties(PolPartyModel md)
        {
            qry = "SELECT PACODE,PANAME,PANAME_V1,SHORT_NAME,SHORT_NAME_V1,S.SYMBOL,P.SID FROM PARTY AS P ";
            qry += "JOIN SYMBOLS AS S ON P.SID=S.SID ORDER BY PANAME";
            ds = dm.create_dataset(qry);
            ViewBag.party = ds;
            if (md.formStatus == "edit")
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    if (row["PACODE"].ToString() == md.editItem.ToString())
                    {
                        md.partyName = row["PANAME"].ToString();
                        md.partyNameV1 = row["PANAME_V1"].ToString();
                        md.shortName = row["SHORT_NAME"].ToString();
                        md.shortNameV1 = row["SHORT_NAME_V1"].ToString();
                        md.sid = row["SID"].ToString();
                    }

                }
            }
            qry = "SELECT SID,SYMBOL_NAME FROM SYMBOLS WHERE SID NOT IN(SELECT SID FROM PARTY) ORDER BY SYMBOL_NAME";
            ds = dm.create_dataset(qry);
            ViewBag.symbols = ds;
            if (md.formStatus == "save")
                md.formStatus = "new";
            return View(md);
        }

        public IActionResult AddParty(PolPartyModel md)
        {
            qry = "INSERT INTO PARTY(PANAME,PANAME_V1,SHORT_NAME,SHORT_NAME_V1,SID) VALUES('" + md.partyName + "','";
            qry += md.partyNameV1 + "','" + md.shortName + "','" + md.shortNameV1 + "'," + md.sid + ")";
            dm.runquery(qry);
            return RedirectToAction("ManageParties");
        }

        public IActionResult UpdateParty(int id, PolPartyModel md)
        {
            qry = "UPDATE PARTY SET PANAME='" + md.partyName + "',PANAME_V1='" + md.partyNameV1 + "',SHORT_NAME='";
            qry += md.shortName + "',SHORT_NAME_V1='" + md.shortNameV1 + "',SID=" + md.sid;
            dm.runquery(qry);
            return RedirectToAction("ManageParties");
        }
        public IActionResult DeleteParty(PolPartyModel md)
        {
            qry = "DELETE FROM PARTY WHERE PACODE=" + md.deleteItem;
            dm.runquery(qry);
            return RedirectToAction("ManageParties");
        }
        #endregion

        #region MANAGE SYMBOLS
        public IActionResult ManageSymbols()
        {
            SymbolsModel md = new SymbolsModel();
            qry = "SELECT SID,SYMBOL_NAME,SYMBOL FROM SYMBOLS ORDER BY SYMBOL_NAME";
            ds = dm.create_dataset(qry);
            ViewBag.symbols = ds;
            md.symbol = (byte[])ds.Tables[0].Rows[0][2];
            return View(md); 
        }

        [HttpPost]
        public IActionResult ManageSymbols(SymbolsModel md)
        {
            qry = "SELECT SID,SYMBOL_NAME,SYMBOL FROM SYMBOLS ORDER BY SYMBOL_NAME";
            ds = dm.create_dataset(qry);
            ViewBag.symbols = ds;
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                if (md.sid == row["SID"].ToString())
                {
                    md.symbol = (byte[])row["SYMBOL"];
                }
            }
            return View(md);
        }

        public IActionResult AddSymbol(SymbolsModel md)
        {
            IFormFile sym = md.symbolfile1;
            byte[] symbolData = { };
            if (sym != null && sym.Length != 0)
            {
                var symstr = sym.OpenReadStream();
                var symStream = new MemoryStream();
                symstr.CopyTo(symStream);
                symbolData = symStream.ToArray();
            }
            SqlParameter ageParam = new SqlParameter("ageparam", symbolData);
            SqlParameter ageParam1 = new SqlParameter("ageparam1", symbolData);
            qry = "INSERT INTO SYMBOLS(SYMBOL_NAME,SYMBOL) VALUES('" + md.symbolName + "'," + "@ageparam" + ")";
            dm.runquery_with_image(qry, ageParam, ageParam1);
            return RedirectToAction("ManageSymbols");
        }
        public IActionResult UpdateSymbol(SymbolsModel md)
        {
            IFormFile sym = md.symbolfile;
            byte[] symbolData = { };

            if (sym != null && sym.Length != 0)
            {
                var symstr = sym.OpenReadStream();
                var symStream = new MemoryStream();
                symstr.CopyTo(symStream);
                symbolData = symStream.ToArray();
            }
            SqlParameter ageParam = new SqlParameter("ageparam", symbolData);
            SqlParameter ageParam1 = new SqlParameter("ageparam1", symbolData);
            qry = "UPDATE SYMBOLS SET SYMBOL=@ageparam WHERE SID=" + md.sid ;
            dm.runquery_with_image(qry, ageParam, ageParam1);
            return RedirectToActionPreserveMethod("ManageSymbols");
        }

        public IActionResult DeleteSymbol(SymbolsModel md)
        {
            qry = "DELETE FROM SYMBOLS WHERE SID=" + md.sid;
            dm.runquery(qry);
            return RedirectToAction("ManageSymbols");
        }
        #endregion

        #region MANAGE CANDIDATES
        public IActionResult ManageCandidates()
        {
            CandidateModel md = new CandidateModel();
            qry = "SELECT POSTNAME,TYPE_CODE,POSTSHORTNAME FROM CONST_TYPE_MASTER WHERE STATUS=1 AND PAN_MUN='P' ORDER BY POSTNAME";
            ds = dm.create_dataset(qry);
            ViewBag.posts = ds;
            md.pstCode = ds.Tables[0].Rows[0]["TYPE_CODE"].ToString();
            qry = "SELECT PNO,PAN_NAME FROM PANCHAYAT ORDER BY PAN_NAME";
            ds = dm.create_dataset(qry);
            ViewBag.panchayats = ds;
            md.panchayat = ds.Tables[0].Rows[0]["PNO"].ToString();
            if (md.pstCode=="1")
            {
                qry = "SELECT CONST_CODE,CONST_NAME FROM CONSTITUENCY WHERE TYPE_CODE=1 AND PCODE=" + md.panchayat + " ORDER BY CONST_NAME";
            }
            else
            {
                qry = "SELECT CONST_CODE,CONST_NAME FROM CONSTITUENCY WHERE TYPE_CODE=" + md.pstCode + " ORDER BY CONST_NAME";
            }
            ds = dm.create_dataset(qry);
            ViewBag.constituencies = ds;
            md.constCode = ds.Tables[0].Rows[0]["CONST_CODE"].ToString();
            qry = "SELECT PACODE,PANAME FROM PARTY ORDER BY PANAME";
            ds = dm.create_dataset(qry);
            ViewBag.parties = ds;
           
            qry = "SELECT C.CID,C.CAND_SL_NO,C.CAND_NAME,C.CAND_NAME_V1,CASE C.GENDER WHEN 'M' THEN 'Male' WHEN 'F' THEN ";
            qry += "'Female' WHEN 'T' THEN 'Third Gender' END AS GENDER,CASE INDEPENDENT WHEN 1 THEN 'INDEPENDENT' ELSE ";
            qry += "P.PANAME END AS PANAME, S.SYMBOL FROM NOMINATIONS AS C LEFT JOIN PARTY AS P ON C.PACODE = P.PACODE ";
            qry += "LEFT JOIN SYMBOLS AS S ON (P.SID = S.SID OR C.SID=S.SID) WHERE CONST_CODE=" + md.constCode + " ORDER BY CAND_SL_NO";
            ds = dm.create_dataset(qry);
            ViewBag.candidates = ds;
            qry = "SELECT SID,SYMBOL_NAME FROM SYMBOLS ORDER BY SYMBOL_NAME";
            ds = dm.create_dataset(qry);
            ViewBag.symbols = ds;
            return View(md);
        }

        [HttpPost]
        public IActionResult ManageCandidates(CandidateModel md)
        {
            qry = "SELECT POSTNAME,TYPE_CODE,POSTSHORTNAME FROM CONST_TYPE_MASTER WHERE STATUS=1 AND PAN_MUN='P' ORDER BY POSTNAME";
            ds = dm.create_dataset(qry);
            ViewBag.posts = ds;
            qry = "SELECT PNO,PAN_NAME FROM PANCHAYAT ORDER BY PAN_NAME";
            ds = dm.create_dataset(qry);
            ViewBag.panchayats = ds;
            if (md.postCause=="ddwnPost")
            {
                md.panchayat = ds.Tables[0].Rows[0]["PNO"].ToString();
            }
            if (md.pstCode == "1")
            {
                qry = "SELECT CONST_CODE,CONST_NAME FROM CONSTITUENCY WHERE TYPE_CODE=1 AND PCODE=" + md.panchayat + " ORDER BY CONST_NAME";
            }
            else
            {
                qry = "SELECT CONST_CODE,CONST_NAME FROM CONSTITUENCY WHERE TYPE_CODE=" + md.pstCode + " ORDER BY CONST_NAME";
            }
            ds = dm.create_dataset(qry);
            if (md.postCause == "ddwnPost" || md.postCause == "ddwnPanchayat")
            {
                md.constCode= ds.Tables[0].Rows[0]["CONST_CODE"].ToString();
            }
            ViewBag.constituencies = ds;
            qry = "SELECT PACODE,PANAME FROM PARTY ORDER BY PANAME";
            ds = dm.create_dataset(qry);
            ViewBag.parties = ds;
            qry = "SELECT C.CID,C.CAND_SL_NO,C.CAND_NAME,C.CAND_NAME_V1,CASE C.GENDER WHEN 'M' THEN 'Male' WHEN 'F' THEN ";
            qry += "'Female' WHEN 'T' THEN 'Third Gender' END AS GENDER,CASE INDEPENDENT WHEN 1 THEN 'INDEPENDENT' ELSE ";
            qry += "P.PANAME END AS PANAME, S.SYMBOL FROM NOMINATIONS AS C LEFT JOIN PARTY AS P ON C.PACODE = P.PACODE ";
            qry += "LEFT JOIN SYMBOLS AS S ON (P.SID = S.SID OR C.SID=S.SID) WHERE CONST_CODE=" + md.constCode + " ORDER BY CAND_SL_NO";
            ds = dm.create_dataset(qry);
            ViewBag.candidates = ds;
            qry = "SELECT SID,SYMBOL_NAME FROM SYMBOLS ORDER BY SYMBOL_NAME";
            ds = dm.create_dataset(qry);
            ViewBag.symbols = ds;
            if (md.formStatus=="edit")
            {
                qry = "SELECT *FROM NOMINATIONS WHERE CID=" + md.editValue;
                ds=dm.create_dataset(qry);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    DataRow row = ds.Tables[0].Rows[0];
                    md.slNo = row["CAND_SL_NO"].ToString();
                    md.cName= row["CAND_NAME"].ToString();
                    md.cNameV1= row["CAND_NAME_V1"].ToString();
                    md.gender= row["GENDER"].ToString();
                    md.independent= (bool)row["INDEPENDENT"];
                    md.sid = row["SID"].ToString();
                    md.pacode = row["PACODE"].ToString();
                }
                else
                {
                    md.formStatus = "new";
                }
                
            }
            return View(md);
        }
        public IActionResult SaveCandidate(CandidateModel md)
        {
            string ind,pacode,sid;
            if (HttpContext.Request.Form["chkIndependent"].ToArray().Length > 0 )
            {
                ind = "1";
                pacode = "NULL";
                sid = md.sid;
            }
            else
            {
                ind = "0";
                pacode = md.pacode;
                sid = "NULL";
            }
            qry = "INSERT INTO NOMINATIONS(PSTCODE,CONST_CODE,CAND_NAME,CAND_NAME_V1,GENDER,PACODE,INDEPENDENT,SID) VALUES(";
            qry += md.pstCode + "," + md.constCode + ",'" + md.cName + "',N'" + md.cNameV1 + "','" + md.gender + "'," + pacode;
            qry += "," + ind + "," + sid + ")";
            dm.runquery(qry);
            return RedirectToActionPreserveMethod("ManageCandidates");
        }
        
        public IActionResult UpdateCandidate(CandidateModel md)
        {
            string ind, pacode, sid;
            if (HttpContext.Request.Form["chkIndependent"].ToArray().Length > 0)
            {
                ind = "1";
                pacode = "NULL";
                sid = md.sid;
            }
            else
            {
                ind = "0";
                pacode = md.pacode;
                sid = "NULL";
            }
            qry = "UPDATE NOMINATIONS SET CAND_NAME='" + md.cName + "',CAND_NAME_V1=N'" + md.cNameV1 + "',CAND_SL_NO=";
            qry += md.slNo + ",GENDER='" + md.gender + "',PACODE=" + pacode + ",INDEPENDENT=" + ind + ",SID=" + sid;
            qry += " WHERE CID=" + md.editValue;
            dm.runquery(qry);
            return RedirectToActionPreserveMethod("ManageCandidates");
        }
        public IActionResult DeleteCandidate(CandidateModel md)
        {
            qry = "DELETE FROM NOMINATIONS WHERE CID=" + md.editValue;
            dm.runquery(qry);
            return RedirectToActionPreserveMethod("ManageCandidates");
        }


        #endregion

        #region GENERATE BALLOT PAPER
        public IActionResult BallotPaper()
        {
            BallotModel md= new BallotModel();
            qry = "SELECT POSTNAME,TYPE_CODE,POSTSHORTNAME FROM CONST_TYPE_MASTER WHERE STATUS=1 AND PAN_MUN='P' ORDER BY POSTNAME";
            ds = dm.create_dataset(qry);
            ViewBag.posts = ds;
            md.pstCode = ds.Tables[0].Rows[0]["TYPE_CODE"].ToString();
            qry = "SELECT PNO,PAN_NAME FROM PANCHAYAT ORDER BY PAN_NAME";
            ds = dm.create_dataset(qry);
            ViewBag.panchayats = ds;
            md.panchayat = ds.Tables[0].Rows[0]["PNO"].ToString();
            if (md.pstCode == "1")
            {
                qry = "SELECT CONST_CODE,CONST_NAME FROM CONSTITUENCY WHERE TYPE_CODE=1 AND PCODE=" + md.panchayat + " ORDER BY CONST_NAME";
            }
            else
            {
                qry = "SELECT CONST_CODE,CONST_NAME FROM CONSTITUENCY WHERE TYPE_CODE=" + md.pstCode + " ORDER BY CONST_NAME";
            }
            ds = dm.create_dataset(qry);
            ViewBag.constituencies = ds;
            md.constCode = ds.Tables[0].Rows[0]["CONST_CODE"].ToString();
            qry = "SELECT PACODE,PANAME FROM PARTY ORDER BY PANAME";
            ds = dm.create_dataset(qry);
            return View(md);
            
        }
        [HttpPost]
        public IActionResult BallotPaper(BallotModel md)
        {
            qry = "SELECT POSTNAME,TYPE_CODE,POSTSHORTNAME FROM CONST_TYPE_MASTER WHERE STATUS=1 AND PAN_MUN='P' ORDER BY POSTNAME";
            ds = dm.create_dataset(qry);
            ViewBag.posts = ds;
            qry = "SELECT PNO,PAN_NAME FROM PANCHAYAT ORDER BY PAN_NAME";
            ds = dm.create_dataset(qry);
            ViewBag.panchayats = ds;
            if (md.postCause == "ddwnPost")
            {
                md.panchayat = ds.Tables[0].Rows[0]["PNO"].ToString();
            }
            if (md.pstCode == "1")
            {
                qry = "SELECT CONST_CODE,CONST_NAME FROM CONSTITUENCY WHERE TYPE_CODE=1 AND PCODE=" + md.panchayat + " ORDER BY CONST_NAME";
            }
            else
            {
                qry = "SELECT CONST_CODE,CONST_NAME FROM CONSTITUENCY WHERE TYPE_CODE=" + md.pstCode + " ORDER BY CONST_NAME";
            }
            ds = dm.create_dataset(qry);
            if (md.postCause == "ddwnPost" || md.postCause == "ddwnPanchayat")
            {
                md.constCode = ds.Tables[0].Rows[0]["CONST_CODE"].ToString();
            }
            ViewBag.constituencies = ds;
            return View(md);

        }
        #endregion

        #region
        public IActionResult CandidateList()
        {
            BallotModel md = new BallotModel();
            qry = "SELECT POSTNAME,TYPE_CODE,POSTSHORTNAME FROM CONST_TYPE_MASTER WHERE STATUS=1 AND PAN_MUN='P' ORDER BY POSTNAME";
            ds = dm.create_dataset(qry);
            ViewBag.posts = ds;
            md.pstCode = ds.Tables[0].Rows[0]["TYPE_CODE"].ToString();
            qry = "SELECT PNO,PAN_NAME FROM PANCHAYAT ORDER BY PAN_NAME";
            ds = dm.create_dataset(qry);
            ViewBag.panchayats = ds;
            md.panchayat = ds.Tables[0].Rows[0]["PNO"].ToString();
            if (md.pstCode == "1")
            {
                qry = "SELECT CONST_CODE,CONST_NAME FROM CONSTITUENCY WHERE TYPE_CODE=1 AND PCODE=" + md.panchayat + " ORDER BY CONST_NAME";
            }
            else
            {
                qry = "SELECT CONST_CODE,CONST_NAME FROM CONSTITUENCY WHERE TYPE_CODE=" + md.pstCode + " ORDER BY CONST_NAME";
            }
            ds = dm.create_dataset(qry);
            ViewBag.constituencies = ds;
            md.constCode = ds.Tables[0].Rows[0]["CONST_CODE"].ToString();
            qry = "SELECT PACODE,PANAME FROM PARTY ORDER BY PANAME";
            ds = dm.create_dataset(qry);
            return View(md);

        }
        [HttpPost]
        public IActionResult CandidateList(BallotModel md)
        {
            qry = "SELECT POSTNAME,TYPE_CODE,POSTSHORTNAME FROM CONST_TYPE_MASTER WHERE STATUS=1 AND PAN_MUN='P' ORDER BY POSTNAME";
            ds = dm.create_dataset(qry);
            ViewBag.posts = ds;
            qry = "SELECT PNO,PAN_NAME FROM PANCHAYAT ORDER BY PAN_NAME";
            ds = dm.create_dataset(qry);
            ViewBag.panchayats = ds;
            if (md.postCause == "ddwnPost")
            {
                md.panchayat = ds.Tables[0].Rows[0]["PNO"].ToString();
            }
            if (md.pstCode == "1")
            {
                qry = "SELECT CONST_CODE,CONST_NAME FROM CONSTITUENCY WHERE TYPE_CODE=1 AND PCODE=" + md.panchayat + " ORDER BY CONST_NAME";
            }
            else
            {
                qry = "SELECT CONST_CODE,CONST_NAME FROM CONSTITUENCY WHERE TYPE_CODE=" + md.pstCode + " ORDER BY CONST_NAME";
            }
            ds = dm.create_dataset(qry);
            if (md.postCause == "ddwnPost" || md.postCause == "ddwnPanchayat")
            {
                md.constCode = ds.Tables[0].Rows[0]["CONST_CODE"].ToString();
            }
            ViewBag.constituencies = ds;
            return View(md);

        }
        #endregion

    }
}
