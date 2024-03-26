using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SEMS.Models;
using System.Data;
using System.Diagnostics;


namespace SEMS.Controllers
{
    
    public class HomeController : Controller
    {
        string qry;
        public static string constr="";
        DataSet ds = new DataSet();
        SqlConnection con = new SqlConnection();
        private readonly ElectionDBContext myDbContext;
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration configuration;
        public HomeController(ILogger<HomeController> logger, ElectionDBContext myDbContext,IConfiguration configuration)
        {
            _logger = logger;
            this.myDbContext = myDbContext;
            constr=ConfigurationExtensions.GetConnectionString(configuration, "dbConnection").ToString();
        }
        DataManager dm = new DataManager(constr);

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
                dm.makeconnection(ref con);
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
        #region Login


        public IActionResult Login()
        {
            if (TempData.ContainsKey("validUser"))
            {
                ViewBag.validUser = (bool)TempData["validUser"];
            }
            return View();
        }
        [HttpPost]
        public IActionResult Login(LoginModel model)
        {
          if (!ModelState.IsValid)
            {
                return View(model);
            }
            //TempData["uid"] = model.uid;
            //TempData["pwd"] = model.pwd;
            return RedirectToActionPreserveMethod("Authenticate");
        }
        public IActionResult Authenticate(LoginModel md)
        {
            DataSet ds;
           
            qry = "SELECT UID,PASSWORD,TYPE_ID,DEPT_CODE,OFFCODE,ELECTION_TYPE FROM USERS WHERE STATUS=1 AND UPPER(USERNAME)='" + md.uid.ToString().ToUpper() + "' AND PASSWORD='" + md.pwd.ToUpper() + "'";
            ds = dm.create_dataset(qry);
            if (ds.Tables[0].Rows.Count == 0)
            {
                TempData["validUser"] = false;
                return RedirectToAction("Login");
            }
            else
            {
                TempData["validUser"] = true;
                HttpContext.Session.SetString("logUser", md.uid);
                HttpContext.Session.SetString("logUserID", ds.Tables[0].Rows[0][0].ToString());
                HttpContext.Session.SetString("logUserDeptCode", ds.Tables[0].Rows[0][3].ToString());
                HttpContext.Session.SetString("logUserOffCode", ds.Tables[0].Rows[0][4].ToString());
                HttpContext.Session.SetString("electionType", ds.Tables[0].Rows[0][5].ToString());
                qry = "SELECT USER_TYPE FROM USER_TYPE WHERE TYPE_ID=" + ds.Tables[0].Rows[0][2];
                string userType = dm.create_dataset(qry).Tables[0].Rows[0][0].ToString();
                HttpContext.Session.SetString("logUserType", userType.ToUpper());
            }
            return RedirectToActionPreserveMethod("Homepage");
        }
        public IActionResult Homepage()
        {

            return View();
        }
        #endregion

        #region Manage Users
        public IActionResult ManageUsers()
        {
            if (HttpContext.Session.GetString("logUserType")=="ADMIN")
            {
                qry = "SELECT TYPE_ID,USER_TYPE FROM USER_TYPE WHERE STATUS=1 ORDER BY USER_TYPE";
            }
            else
            {
                qry = "SELECT TYPE_ID,USER_TYPE FROM USER_TYPE WHERE STATUS=1 AND TYPE_ID IN (SELECT TYPE_ID";
                qry += " FROM USER_TYPE WHERE USER_LEVEL FROM USER_TYPE WHERE TYPE_ID=1)) ORDER BY USER_TYPE";
            }
            ds = dm.create_dataset(qry);
            ViewBag.userType = ds;
            if (HttpContext.Request.Method=="POST")
            {
                ViewBag.selectedUserType = HttpContext.Request.Form["type_id"];
                ViewBag.selectedElectionType = HttpContext.Request.Form["electionType"].ToString();
                if (TempData.ContainsKey("postCause"))
                {
                    ViewBag.postCause = TempData["postCause"];
                }
                string etype = ViewBag.selectedElectionType;
                if (TempData.ContainsKey("electionType"))
                {
                    etype = TempData["electionType"].ToString();
                }
                qry = "SELECT UID,USERNAME,DEFAULT_PASSWORD,TYPE_ID,CASE STATUS WHEN 1 THEN 'Active' ELSE 'Deactivated' END AS STATUS FROM USERS WHERE  TYPE_ID=" + ViewBag.selectedUserType + " AND ELECTION_TYPE='" + etype + "' ORDER BY USERNAME";
            }
            else
            {
                qry = "SELECT UID,USERNAME,TYPE_ID,DEFAULT_PASSWORD,CASE STATUS WHEN 1 THEN 'Active' ELSE 'Deactivated' END AS STATUS FROM USERS WHERE  TYPE_ID=" + ds.Tables[0].Rows[0][0] + " ORDER BY USERNAME";
            }
            ds = dm.create_dataset(qry);
            ViewBag.userList = ds;
            ViewBag.qry = qry;
            return View();
        }
        public IActionResult ManageUsersPost()
        {
            TempData["postCause"] = "userType";
            return RedirectToActionPreserveMethod("ManageUsers");
        }
        public IActionResult NewUser()
        {
            TempData["postCause"] = "newUser";
            return RedirectToActionPreserveMethod("ManageUsers");
        }
        public IActionResult EditUser(int id)
        {
            TempData["postCause"] = "editUser";
            TempData["user_id"] = id;
            return RedirectToActionPreserveMethod("ManageUsers");
        }
        public IActionResult CancelUser(UserModel model)
        {
            TempData["postCause"] = "cancelUser";
            TempData["electionType"] = model.electionType;
            return RedirectToActionPreserveMethod("ManageUsers");
        }
        
        public IActionResult SaveUser(UserModel model)
        {
            
            TempData["postCause"] = "saveUser";
            TempData["electionType"] = model.electionType;
            try
            {
                qry = "INSERT INTO USERS(USERNAME,TYPE_ID,PASSWORD,DEFAULT_PASSWORD,CREATED_BY,ELECTION_TYPE) VALUES('" + model.username + "',";
                qry += model.type_id + ",'" + model.default_password + "','" + model.default_password + "'," + HttpContext.Session.GetString("logUserID");
                qry += ",'" + model.electionType + "')";
                dm.runquery(qry);
            }
            catch (Exception ex)
            {
                
            }
           
            return RedirectToActionPreserveMethod("ManageUsers");
        }

        public IActionResult UpdateUser(int id,UserModel model)
        {
            TempData["postCause"] = "updateUser";
            qry = "UPDATE USERS SET PASSWORD='" + model.default_password + "',DEFAULT_PASSWORD='" + model.default_password;
            qry += "',STATUS=" + model.status + " WHERE UID=" + id;
            dm.runquery(qry);
            return RedirectToActionPreserveMethod("ManageUsers");
        }
        public IActionResult DeleteUser(int id)
        {
            TempData["postCause"] = "deleteUser";
            qry = "DELETE FROM USERS WHERE UID=" + HttpContext.Request.Form["hidDeleteItem"];
            dm.runquery(qry);
            return RedirectToActionPreserveMethod("ManageUsers");
        }

        #endregion
        #region Authorization Error
        public ActionResult AuthorizationError()
        {
            return View();
        }
        #endregion
        #region BACKUP DATABASE
        public ActionResult BackupDB()
        {
            bool allOK = checkAuthorization(3);
            if (!allOK)
            {
                HttpContext.Session.SetString("errorMessage", "You are not authorized to access this page. Please contact Administrator.......");
                return RedirectToAction("AuthorizationError");
            }
            if (TempData.ContainsKey("backup"))
            {
                ViewBag.backup = TempData["backup"].ToString();
            }
            return View();
        }

        public ActionResult DoBackupDB()
        {

            string qry, bkupPath = System.IO.Directory.GetCurrentDirectory();
            string bkupPath1 = bkupPath + "\\Database\\STATEELECTION.BAK";
            string bkupPath2 = bkupPath + "\\Database\\SEROLL.BAK";
            string qry1;
            qry = " BACKUP DATABASE STATE_ELECTION TO  DISK = '" + bkupPath1 + "' WITH NOFORMAT, INIT, NAME = 'STATE_ELECTION-fullBackup', SKIP, NOREWIND, NOUNLOAD, STATS = 10";
            qry1 = " BACKUP DATABASE SE_EROLL TO  DISK = '" + bkupPath2 + "' WITH NOFORMAT, INIT, NAME = 'SE_EROLL-fullBackup', SKIP, NOREWIND, NOUNLOAD, STATS = 10";
            try
            {
                dm.runquery(qry);
                dm.runquery(qry1);
                TempData["backup"] = "success";
            }
            catch (Exception ex)
            {
                TempData["backup"] = "failed";
            }
            return RedirectToActionPreserveMethod("BackupDB");
        }
        public IActionResult DownloadDB()
        {
            /* string qry, bkupPath = System.IO.Directory.GetCurrentDirectory();
             string folderPath = "D:\\";

             string bkupPath1 = bkupPath + "\\Database\\STATEELECTION.BAK";
             string bkupPath2 = bkupPath + "\\Database\\SEEROLL.BAK";
             var net = new System.Net.WebClient();
             var data = net.DownloadData(bkupPath1);
             var content = new System.IO.MemoryStream(data);
             var contentType = "APPLICATION/octet-stream";
             var fileName = "STATE_ELECTION.bak";
             var res= File(content, contentType, fileName);
             var net1 = new System.Net.WebClient();
             var data1 = net.DownloadData(bkupPath2);
             var content1= new System.IO.MemoryStream(data1);
             var contentType1 = "APPLICATION/octet-stream";
             var fileName1 = "SE_EROLL.bak";
             var res1 = File(content, contentType, fileName1);
             return res;*/
            string bkupPath = System.IO.Directory.GetCurrentDirectory();
            bkupPath += "\\Database\\STATEELECTION.BAK";
            var net = new System.Net.WebClient();
            var data = net.DownloadData(bkupPath);
            var content = new System.IO.MemoryStream(data);
            var contentType = "APPLICATION/octet-stream";
            var fileName = "STATEELECTION.BAK";
            return File(content, contentType, fileName);

        }
        #endregion

        #region FREEZE DATA

        public ActionResult FreezeData()
        {
            string qry;
            bool allOK = checkAuthorization(4);
            if (!allOK)
            {
                HttpContext.Session.SetString("errorMessage", "You are not authorized to access this page. Please contact Administrator.......");
                return RedirectToAction("AuthorizationError");
            }
            if (TempData.ContainsKey("freezeSuccess"))
            {
                ViewBag.freezeSuccess = TempData["freezeSuccess"].ToString();
            }
            if (TempData.ContainsKey("unfreezeSuccess"))
            {
                ViewBag.unfreezeSuccess = TempData["unfreezeSuccess"].ToString();
            }
            qry = "SELECT F_ID,FREEZE_ITEM,FREEZED FROM FREEZE_MASTER ORDER BY FREEZE_ITEM";
            ds = dm.create_dataset(qry);
            ViewBag.freezeItemList = ds;
            if (HttpContext.Request.Method == "POST")
            {
                ViewBag.freezeItem = HttpContext.Request.Form["ddwnFreezeItem"];
            }
            return View();
        }
        public ActionResult FreezeItem()
        {
            string qry;
            bool passwordMatched = false;
            qry = "SELECT PASSWORD FROM USERS WHERE TYPE_ID=1";
            dm.makeconnection(ref con);
            ds = dm.create_dataset(qry);
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                if (HttpContext.Request.Form["txtFreezePassword"].ToString() == row[0].ToString())
                {
                    qry = "UPDATE FREEZE_MASTER SET FREEZED=1 WHERE F_ID=" + HttpContext.Request.Form["ddwnFreezeItem"];
                    dm.runquery(qry);
                    passwordMatched = true;
                    break;
                }
            }
            if (passwordMatched)
            {
                TempData["freezeSuccess"] = "success";
            }
            else
            {
                TempData["freezeSuccess"] = "failed";
            }
            return RedirectToActionPreserveMethod("FreezeData");
        }
        public ActionResult UnfreezeItem()
        {
            string qry;
            bool passwordMatched = false;
            qry = "SELECT PASSWORD FROM USERS WHERE TYPE_ID=1";
            ds = dm.create_dataset(qry);
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                if (HttpContext.Request.Form["txtunFreezePassword"].ToString() == row[0].ToString())
                {
                    qry = "UPDATE FREEZE_MASTER SET FREEZED=0 WHERE F_ID=" + HttpContext.Request.Form["ddwnFreezeItem"];
                    dm.runquery(qry);
                    passwordMatched = true;
                    break;
                }
            }
            if (passwordMatched)
            {
                TempData["unfreezeSuccess"] = "success";
            }
            else
            {
                TempData["unfreezeSuccess"] = "failed";
            }
            return RedirectToActionPreserveMethod("FreezeData");
        }


        #endregion

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
