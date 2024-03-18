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
            return RedirectToActionPreserveMethod("ManageSymbols");
        }
        public IActionResult UpdateSymbol(SymbolsModel md)
        {
            //IFormFile sym = md.symbolfile;

            //byte[] symbolData = { };

            //if (sym != null && sym.Length != 0)
            //{
            //    var symstr = sym.OpenReadStream();
            //    var symStream = new MemoryStream();
            //    symstr.CopyTo(symStream);
            //    symbolData = symStream.ToArray();
            //}
            //SqlParameter ageParam = new SqlParameter("ageparam", symbolData);
            //SqlParameter ageParam1 = new SqlParameter("ageparam1", symbolData);
            //qry = "INSERT INTO SYMBOLS(SYMBOL_NAME,SYMBOL) VALUES('FSDFAS'," + "@ageparam" + ")";
            //dm.runquery_with_image(qry, ageParam, ageParam1);
            return RedirectToActionPreserveMethod("ManageSymbols");
        }
        #endregion

    }
}
