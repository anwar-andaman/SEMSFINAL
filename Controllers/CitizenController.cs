using Microsoft.AspNetCore.Mvc;
using SEMS.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics;
//using Microsoft.CodeAnalysis.Scripting;
using System.Globalization;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Hosting;
//using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Reflection;
using SEMS.Models.Counting;
using SEMS.Models.Result;

namespace SEMS.Controllers
{
    public class CitizenController : Controller
    {
        string qry;
        DataSet ds = new DataSet();
        SqlConnection con = new SqlConnection();
        private readonly ElectionDBContext myDbContext;
        private readonly ILogger<CitizenController> _logger;
        private readonly IConfiguration configuration;
        RevisionModel md = new RevisionModel();
        public CitizenController(ILogger<CitizenController> logger, ElectionDBContext myDbContext, IConfiguration configuration)
        {
            _logger = logger;
            this.myDbContext = myDbContext;
            HomeController.constr = ConfigurationExtensions.GetConnectionString(configuration, "dbConnection").ToString();
        }
        DataManager dm = new DataManager(HomeController.constr);
        #region HOME PAGE
        public IActionResult Home()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Home(CitizenHomeModel md)
        {

            if (HttpContext.Request.Form["Place"] == "O")
            {

                return RedirectToAction("Home");
            }
            else if (HttpContext.Request.Form["Place"] == "P")
            {
                HttpContext.Session.SetString("pan_mun", "P");
                return RedirectToAction("NewRegistration");
            }
            else if (HttpContext.Request.Form["Place"] == "M")
            {
                HttpContext.Session.SetString("pan_mun", "M");
                return RedirectToAction("NewRegistration", "Citizen");
            }
            else
            {
                return RedirectToAction("Home");
            }
            /*
             
            if (md.residence == "O")
            {

                return RedirectToAction("Home");
            }
            else if (md.residence == "P")
            {
                HttpContext.Session.SetString("pan_mun", "P");
                return RedirectToAction("NewRegistration");
            }
            else if (md.residence == "M")
            {
                HttpContext.Session.SetString("pan_mun", "M");
                return RedirectToAction("NewRegistration", "Citizen");
            }
            else
            {
                return RedirectToAction("Home");
            }
             */

        }
        #endregion

        #region NEW REGISTRATION
        public IActionResult NewRegistration()
        {
            if (HttpContext.Session.Keys.Contains("pan_mun"))
            {
                if (HttpContext.Session.GetString("pan_mun") == "P")
                {

                }
                else
                {

                }
            }
            else
            {

                return RedirectToAction("Home");

            }
            RegistrationModel md = new RegistrationModel();
            qry = "SELECT REVISIONNO,QUALIFYINGDATE,REVISIONYEAR FROM SE_EROLL.DBO.REVISIONS WHERE REVISIONNO=(SELECT ";
            qry += "MAX(REVISIONNO) FROM SE_EROLL.DBO.REVISIONS WHERE ISACTIVE=1)";
            ds = dm.create_dataset(qry);
            qry = ds.Tables[0].Rows[0]["REVISIONNO"].ToString();
            ViewBag.revisionNo = int.Parse(qry);
            string ry = ds.Tables[0].Rows[0]["REVISIONYEAR"].ToString();
            ViewBag.revisionYear = ry;
            ViewBag.qualifyingDate = (DateTime)ds.Tables[0].Rows[0]["QUALIFYINGDATE"];
            ViewBag.panMun = HttpContext.Session.GetString("pan_mun");
            if (HttpContext.Session.GetString("pan_mun") == "P")
            {
                qry = "SELECT TCODE,TNAME FROM TEHSIL WHERE E_ROLL=1 ORDER BY TNAME";
                ds = dm.create_dataset(qry);
                ViewBag.tehsils = ds;
                ViewBag.addressTehsils = ds;
                qry = "SELECT PCODE,PAN_NAME FROM PANCHAYAT WHERE TCODE=" + md.tehsil + " ORDER BY PAN_NAME";
                ds = dm.create_dataset(qry);
                ViewBag.panchayats = ds;
                bool flag = false;
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    if (row[0].ToString() == md.panchayat.ToString())
                    {
                        flag = true;
                    }
                }
                if (flag == false)
                {
                    md.panchayat = 0;
                }
                qry = "SELECT CONST_NO,WARD_NAME FROM PAN_WARD WHERE PCODE=" + md.panchayat + " ORDER BY WARD_NAME";
                ds = dm.create_dataset(qry);
                ViewBag.wards = ds;
                qry = "SELECT * FROM SE_EROLL.DBO.RLNTYPE ORDER BY RLNTYPE";
                ds = dm.create_dataset(qry);
                ViewBag.rlnTypes = ds;
                qry = "SELECT VCODE,VNAME FROM VILLAGE WHERE TCODE=" + md.addressTehsil + " ORDER BY VNAME";
                ds = dm.create_dataset(qry);
                ViewBag.villages = ds;
                return View(md);
            }
            else if (HttpContext.Session.GetString("pan_mun") == "M")
            {
                qry = "SELECT M_ID AS TCODE,MC_NAME AS TNAME FROM MUNICIPALS ORDER BY MC_NAME";
                ds = dm.create_dataset(qry);
                ViewBag.tehsils = ds;

                qry = "SELECT WARD_NO AS PCODE,WARD_NAME AS PAN_NAME FROM MUN_WARD WHERE M_ID=" + md.tehsil + " ORDER BY WARD_NO";
                ds = dm.create_dataset(qry);
                ViewBag.panchayats = ds;
                bool flag = false;
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    if (row[0].ToString() == md.panchayat.ToString())
                    {
                        flag = true;
                    }
                }
                if (flag == false)
                {
                    md.panchayat = 0;
                }
                qry = "SELECT PART_NO AS CONST_NO,PART_NAME AS WARD_NAME FROM SE_EROLL.DBO.PARTLIST WHERE AREA_TYPE='M' AND CONST_NO=" + md.panchayat + " ORDER BY WARD_NAME";
                ds = dm.create_dataset(qry);
                ViewBag.wards = ds;
                qry = "SELECT * FROM SE_EROLL.DBO.RLNTYPE ORDER BY RLNTYPE";
                ds = dm.create_dataset(qry);
                ViewBag.rlnTypes = ds;
                qry = "SELECT TCODE,TNAME FROM TEHSIL WHERE E_ROLL=1 AND TCODE IN (SELECT TCODE FROM MUNICIPALS) ORDER BY TNAME";
                ds = dm.create_dataset(qry);
                ViewBag.addressTehsils = ds;
                qry = "SELECT VCODE,VNAME FROM VILLAGE WHERE TCODE=" + md.addressTehsil + " ORDER BY VNAME";
                ds = dm.create_dataset(qry);
                ViewBag.villages = ds;
                return View(md);
            }
            else
            {
                return RedirectToAction("Home");
            }

        }
        [HttpPost]
        public IActionResult NewRegistration(RegistrationModel md)
        {
            if (HttpContext.Session.Keys.Contains("pan_mun"))
            {
                if (HttpContext.Session.GetString("pan_mun") == "P")
                {

                }
                else
                {

                }
            }
            else
            {

                return RedirectToAction("Home");

            }
            qry = "SELECT REVISIONNO,QUALIFYINGDATE,REVISIONYEAR FROM SE_EROLL.DBO.REVISIONS WHERE REVISIONNO=(SELECT ";
            qry += "MAX(REVISIONNO) FROM SE_EROLL.DBO.REVISIONS WHERE ISACTIVE=1)";
            ds = dm.create_dataset(qry);
            qry = ds.Tables[0].Rows[0]["REVISIONNO"].ToString();
            ViewBag.revisionNo = int.Parse(qry);
            string ry = ds.Tables[0].Rows[0]["REVISIONYEAR"].ToString();
            ViewBag.revisionYear = ry;
            ViewBag.qualifyingDate = (DateTime)ds.Tables[0].Rows[0]["QUALIFYINGDATE"];
            ViewBag.panMun = HttpContext.Session.GetString("pan_mun");
            if (HttpContext.Session.GetString("pan_mun") == "P")
            {
                qry = "SELECT TCODE,TNAME FROM TEHSIL WHERE E_ROLL=1 ORDER BY TNAME";
                ds = dm.create_dataset(qry);
                ViewBag.tehsils = ds;
                ViewBag.addressTehsils = ds;
                qry = "SELECT PCODE,PAN_NAME FROM PANCHAYAT WHERE TCODE=" + md.tehsil + " ORDER BY PAN_NAME";
                ds = dm.create_dataset(qry);
                ViewBag.panchayats = ds;
                bool flag = false;
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    if (row[0].ToString() == md.panchayat.ToString())
                    {
                        flag = true;
                    }
                }
                if (flag == false)
                {
                    md.panchayat = 0;
                }
                qry = "SELECT CONST_NO,WARD_NAME FROM PAN_WARD WHERE PCODE=" + md.panchayat + " ORDER BY WARD_NAME";
                ds = dm.create_dataset(qry);
                ViewBag.wards = ds;
                qry = "SELECT * FROM SE_EROLL.DBO.RLNTYPE ORDER BY RLNTYPE";
                ds = dm.create_dataset(qry);
                ViewBag.rlnTypes = ds;
                qry = "SELECT VCODE,VNAME FROM VILLAGE WHERE TCODE=" + md.addressTehsil + " ORDER BY VNAME";
                ds = dm.create_dataset(qry);
                ViewBag.villages = ds;
            }
            else if (HttpContext.Session.GetString("pan_mun") == "M")
            {
                qry = "SELECT M_ID AS TCODE,MC_NAME AS TNAME FROM MUNICIPALS ORDER BY MC_NAME";
                ds = dm.create_dataset(qry);
                ViewBag.tehsils = ds;
                qry = "SELECT WARD_NO AS PCODE,WARD_NAME AS PAN_NAME FROM MUN_WARD WHERE M_ID=" + md.tehsil + " ORDER BY WARD_NO";
                ds = dm.create_dataset(qry);
                ViewBag.panchayats = ds;
                bool flag = false;
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    if (row[0].ToString() == md.panchayat.ToString())
                    {
                        flag = true;
                    }
                }
                if (flag == false)
                {
                    md.panchayat = 0;
                }
                qry = "SELECT PART_NO AS CONST_NO,PART_NAME AS WARD_NAME FROM SE_EROLL.DBO.PARTLIST WHERE AREA_TYPE='M' AND CONST_NO=" + md.panchayat + " ORDER BY WARD_NAME";
                ds = dm.create_dataset(qry);
                ViewBag.wards = ds;
                qry = "SELECT * FROM SE_EROLL.DBO.RLNTYPE ORDER BY RLNTYPE";
                ds = dm.create_dataset(qry);
                ViewBag.rlnTypes = ds;
                qry = "SELECT TCODE,TNAME FROM TEHSIL WHERE E_ROLL=1 AND TCODE IN (SELECT TCODE FROM MUNICIPALS) ORDER BY TNAME";
                ds = dm.create_dataset(qry);
                ViewBag.addressTehsils = ds;
                qry = "SELECT VCODE,VNAME FROM VILLAGE WHERE TCODE=" + md.addressTehsil + " ORDER BY VNAME";
                ds = dm.create_dataset(qry);
                ViewBag.villages = ds;

            }
            else
            {
                return RedirectToAction("Home");
            }
            if (md.postCause == "register")
            {
                ViewBag.formno = ViewBag.formNo;
            }
            return View();
        }
        public IActionResult Register(RegistrationModel md)
        {
            IFormFile ageProof = md.ageProof;
            IFormFile addProof = md.addressProof;
            IFormFile photo = md.photo;
            byte[] ageData = { }, addData = { },photoData= { };

            if (ageProof != null && ageProof.Length != 0)
            {
                var agepr = ageProof.OpenReadStream();
                var add = addProof.OpenReadStream();
                var phot = photo.OpenReadStream();
                var ageStream = new MemoryStream();
                var addStream = new MemoryStream();
                var photoStream = new MemoryStream();
                agepr.CopyTo(ageStream);
                add.CopyTo(addStream);
                phot.CopyTo(photoStream);
                ageData = ageStream.ToArray();
                addData = addStream.ToArray();
                photoData = photoStream.ToArray();
            }
            string dob, age, addline1, addline2, vcode, post, mobile, email, panMun;

            age = md.age.ToString();
            if (md.ageDob == 'D')
            {
                dob = "'" + md.dob + "'";
            }
            else
            {
                dob = "NULL";
            }
            if (!md.addLine1.IsNullOrEmpty())
            {
                addline1 = "'" + md.addLine1.Trim() + "'";
            }
            else
            {
                addline1 = "NULL";
            }
            if (!md.addLine2.IsNullOrEmpty())
            {
                addline2 = "'" + md.addLine2.Trim() + "'";
            }
            else
            {
                addline2 = "NULL";
            }

            if (md.village == 0)
            {
                vcode = "NULL";
            }
            else
            {
                vcode = md.village.ToString();
            }

            if (!md.post.IsNullOrEmpty())
            {
                post = "'" + md.post.Trim() + "'";
            }
            else
            {
                post = "NULL";
            }
            if (!md.email.IsNullOrEmpty())
            {
                email = "'" + md.email.Trim() + "'";
            }
            else
            {
                email = "NULL";
            }


            if (md.mobileNo == 0)
            {
                mobile = "NULL";
            }
            else
            {
                mobile = md.mobileNo.ToString();
            }
            // Read file content into a byte array
            panMun = md.panMun.Trim();

            SqlParameter ageParam = new SqlParameter("ageparam", ageData);
            SqlParameter addParam = new SqlParameter("addparam", addData);
            SqlParameter photoParam = new SqlParameter("photoparam", photoData);
            qry = "INSERT INTO SE_EROLL.DBO.FORMS(FORM_TYPE,ONLINE_FORM,PAN_MUN,PART_NO,ENAME,RLN_TYPE,RLN_NAME,HOUSE_NO,ADDRESS_LINE1,";
            qry += "ADDRESS_LINE2,POSTOFF,VCODE,GENDER,AGE,DOB,REVISIONNO,REVISIONYEAR,MOBILENO,EMAIL,ADDRESS_PROOF,AGE_PROOF,PHOTO) VALUES('A',1,'" + panMun;
            qry += "'," + md.ward + ",'" + md.ename + "','" + md.rlnType + "','" + md.rlnName + "','" + md.houseNo + "',";
            qry += addline1 + "," + addline2 + "," + post + "," + vcode + ",'" + md.gender + "'," + age;
            qry += "," + dob + "," + md.revisionNo + "," + md.revisionYear + "," + mobile + "," + email + "," + "@ageparam" + "," + "@addparam" + "," + "@photoparam" + ") SELECT @@IDENTITY";
            decimal formid = dm.create_scalar_with_image(qry, ageParam, addParam, photoParam);
            qry = "SELECT FORM_NO,FORM_DATE FROM SE_EROLL.DBO.FORMS WHERE FORMID=" + formid;
            ds = dm.create_dataset(qry);
            string formno = "";
            formno = ds.Tables[0].Rows[0][0].ToString();
            DateTime formDate = (DateTime)ds.Tables[0].Rows[0][1];
            formno = "ON" + formDate.Day.ToString().PadLeft(2, '0') + formDate.Month.ToString().PadLeft(2, '0') + formDate.Year.ToString() + formno.PadLeft(5, '0');
            TempData["message"] = "Form Successfully Submitted. Please note the Form No. for any future Reference. Your Form No. is: " + formno;
            return RedirectToAction("CitizenMessage");
        }
        
           
        
        #endregion

        #region TRACK FORM STATUS
        public IActionResult GoToTrack()
        {
            return RedirectToAction("TrackForm");
        }
        public IActionResult TrackForm()
        {
            return View();
        }
        [HttpPost]
        public IActionResult TrackForm(TrackModel md)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.mdState = false;
                return View(md);
            }
            ViewBag.mdState = true;
            string formNo;
            string formDate;
            if (!md.formNo.IsNullOrEmpty())
            {
                formNo = md.formNo.Substring(10);
                formDate = md.formNo.Substring(6,4) + "-" + md.formNo.Substring(4,2) + "-" + md.formNo.Substring(2,2);
                int onOff;
                if (md.formNo.Substring(0,2)=="ON")
                {
                    onOff = 1;
                }
                else
                {
                    onOff=0;
                }

                qry = "SELECT H.STAGE_NO,S.STAGE,S.FLOW_LEVEL FROM SE_EROLL.DBO.FORM_HISTORY AS H JOIN SE_EROLL.DBO.FORM_STAGES AS S ";
                qry += "ON H.STAGE_ID=S.STAGE_ID WHERE FORMID=(SELECT FORMID FROM SE_EROLL.DBO.FORMS WHERE FORM_NO='" + formNo + "' AND CAST(FORM_DATE AS DATE)='" + formDate + "' AND ONLINE_FORM=" + onOff + ")";
                ds = dm.create_dataset(qry);
                ViewBag.status = ds;
                byte maxLevel = 0, level;
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    level = (byte)row["FLOW_LEVEL"];
                    if (level > maxLevel)
                    {
                        maxLevel = level;
                    }
                }
                qry = "SELECT STAGE FROM SE_EROLL.DBO.FORM_STAGES WHERE FLOW_LEVEL>" + maxLevel + " AND TRACK_STATUS=1 ORDER BY FLOW_LEVEL";
                ds = dm.create_dataset(qry);
                ViewBag.levels = ds;
                return View(md);
                
            }
            else
            {
                 return RedirectToAction("TrackForm");
           
            }
        }


        #endregion

        #region SEARCH ELECTORS
        public IActionResult GoToSearch()
        {
            return RedirectToAction("SearchElector");
        }
        public IActionResult SearchElector()
        {
            ViewBag.searchBy = "E";
            ViewBag.panMun = "P";
            return View();
        }
        [HttpPost]
        public IActionResult SearchElector(SearchEelectorModel md)
        {
            ViewBag.searchBy = md.searchBy;
            ViewBag.panMun = md.panMun;
            
            if (md.searchBy == "P") 
            {
                qry = "SELECT MAX(PART_NO) FROM SE_EROLL.DBO.PARTLIST WHERE PAN_MUN='" + md.panMun + "' AND AREA_TYPE='" + md.panMun + "'";
                ds = dm.create_dataset(qry);
                ViewBag.maxPart = ds.Tables[0].Rows[0][0];
                qry = "SELECT ISNULL(MAX(SLNOINPART),1) FROM SE_EROLL.DBO.E_DETAIL WHERE PAN_MUN='" + md.panMun + "'";
               // return qry;
                ds = dm.create_dataset(qry);
                ViewBag.slNo = ds.Tables[0].Rows[0][0];
            }
            if (!TempData.ContainsKey("qry"))
            {
                qry = "SELECT ' ' AS PAN_NAME,' ' as PART_NAME,E.PART_NO,E.SLNOINPART,E.ENAME,E.RLN_NAME,E.AGE,E.EPIC_NO,";
                qry += "E.GENDER FROM SE_EROLL.DBO.E_DETAIL AS E WHERE E.PART_NO IS NULL AND E.SLNOINPART IS NULL";
                //return qry;
                ds = dm.create_dataset(qry);
                ViewBag.eList = ds;
            }
            else
            {
                qry = TempData["qry"].ToString();
                //return qry;
                ds = dm.create_dataset(qry);
                ViewBag.eList = ds;
            }

            return View(md);
            //return qry;
        }

        public IActionResult SearchVoter(SearchEelectorModel md)
        {
            string tableName;
            string wardColumn;
            string pcode;
            if (md.panMun == "P")
            {
                tableName = "PANCHAYAT";
                wardColumn = "PAN_NAME";
                pcode = "PCODE";
            }
            else
            {
                tableName = "MUN_WARD";
                wardColumn = "WARD_NAME";
                pcode = "WARD_NO";
            }
            switch (md.searchBy)
            {
                case "P":
                    {
                        qry = "SELECT P." + wardColumn + " AS PAN_NAME,W.PART_NAME,E.PART_NO,E.SLNOINPART,E.ENAME,E.RLN_NAME,E.AGE,E.EPIC_NO,";
                        qry += "E.GENDER FROM SE_EROLL.DBO.E_DETAIL AS E JOIN SE_EROLL.DBO.PARTLIST AS W ON E.PART_NO=W.PART_NO AND ";
                        qry += "E.PAN_MUN=W.PAN_MUN JOIN " + tableName + " AS P ON W.PCODE=P." + pcode + " WHERE E.PART_NO =" + md.partNo + " AND E.SLNOINPART =" + md.slNo;
                        qry += "AND W.PAN_MUN='" + md.panMun + "' AND W.AREA_TYPE='" + md.panMun + "' AND E.CHNGLISTNO IS NULL";
                        break;
                    }
                case "E":
                    {
                        qry = "SELECT P." + wardColumn + " AS PAN_NAME,W.PART_NAME,E.PART_NO,E.SLNOINPART,E.ENAME,E.RLN_NAME,E.AGE,E.EPIC_NO,";
                        qry += "E.GENDER FROM SE_EROLL.DBO.E_DETAIL AS E JOIN SE_EROLL.DBO.PARTLIST AS W ON E.PART_NO=W.PART_NO AND ";
                        qry += "E.PAN_MUN=W.PAN_MUN JOIN " + tableName + " AS P ON W.PCODE=P." + pcode + " WHERE E.EPIC_NO='" + md.epic + "'";
                        qry += "AND W.PAN_MUN='" + md.panMun + "' AND W.AREA_TYPE='" + md.panMun + "' AND E.CHNGLISTNO IS NULL";
                        break;
                    }
                case "N":
                    {
                        qry = "SELECT P." + wardColumn + " AS PAN_NAME,W.PART_NAME,E.PART_NO,E.SLNOINPART,E.ENAME,E.RLN_NAME,E.AGE,E.EPIC_NO,";
                        qry += "E.GENDER FROM SE_EROLL.DBO.E_DETAIL AS E JOIN SE_EROLL.DBO.PARTLIST AS W ON E.PART_NO=W.PART_NO AND ";
                        qry += "E.PAN_MUN=W.PAN_MUN JOIN " + tableName + " AS P ON W.PCODE=P." + pcode + " WHERE E.ENAME LIKE '%" + md.ename + "%' ";
                        qry += "AND RLN_NAME LIKE '%" + md.rlnName + "%' AND W.PAN_MUN='" + md.panMun + "' AND W.AREA_TYPE='" + md.panMun + "' AND E.CHNGLISTNO IS NULL";
                        break;
                    }
            }
            
            TempData["qry"] = qry;
            return RedirectToActionPreserveMethod("SearchElector");
           // return qry;
        }
        #endregion

        #region DELETION OF NAMES
        public IActionResult CheckID(CitizenHomeModel md)
        {
            qry = "SELECT PAN_MUN FROM SE_EROLL.DBO.E_DETAIL WHERE PART_NO<1000 AND EPIC_NO='" + md.epic + "' AND CHNGLISTNO IS NULL AND STATUSTYPE<>'D'";
            ds=dm.create_dataset(qry);
            if (ds.Tables[0].Rows.Count > 0)
            {
                HttpContext.Session.SetString("epicNo", md.epic);
                HttpContext.Session.SetString("panMun", ds.Tables[0].Rows[0][0].ToString());
                return RedirectToAction("DeleteElector");
            }
            else
            {
                HttpContext.Session.Remove("epicNo");
                return RedirectToAction("Home"); 
            }
            
        }

        public IActionResult DeleteElector()
        {
            byte[] a;
            if (!HttpContext.Session.TryGetValue("epicNo",out a))
            {
                return RedirectToAction("Home");
            }
            else
            {
                DeleteModel md = new DeleteModel();
                string tableName="";
                string wardColumn="";
                string pcode = "";
                md.applEpic = HttpContext.Session.GetString("epicNo");
                md.panMun = HttpContext.Session.GetString("panMun");
                if (md.panMun == "P")
                {
                    tableName = "PANCHAYAT";
                    wardColumn = "PAN_NAME";
                    pcode = "PCODE";
                }
                else if (md.panMun == "M")
                {
                    tableName = "MUN_WARD";
                    wardColumn = "WARD_NAME";
                    pcode = "WARD_NO";
                }
                qry = "SELECT P." + wardColumn + " AS PAN_NAME,W.PART_NAME,E.PART_NO,E.SLNOINPART,E.ENAME,E.RLN_NAME,E.AGE,E.EPIC_NO,";
                qry += "E.GENDER FROM SE_EROLL.DBO.E_DETAIL AS E JOIN SE_EROLL.DBO.PARTLIST AS W ON E.PART_NO=W.PART_NO AND ";
                qry += "E.PAN_MUN=W.PAN_MUN JOIN " + tableName + " AS P ON W.PCODE=P." + pcode + " WHERE E.EPIC_NO='" + md.applEpic + "'";
                qry += "AND W.PAN_MUN='" + md.panMun + "' AND W.AREA_TYPE='" + md.panMun + "' AND E.CHNGLISTNO IS NULL";
                //return RedirectToAction ("returnString",qry);
                ds = dm.create_dataset(qry);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    DataRow row = ds.Tables[0].Rows[0];
                    md.applPartNo = row["PART_NO"].ToString();
                    md.applSlNo = row["SLNOINPART"].ToString();
                    md.applEpic = row["EPIC_NO"].ToString();
                    md.applEname = row["ENAME"].ToString();
                    md.applRlnName = row["RLN_NAME"].ToString();
                    md.applPanchayat = row["PAN_NAME"].ToString();
                    md.applWard = row["PART_NAME"].ToString();
                }
                qry = "SELECT REVISIONNO,QUALIFYINGDATE,REVISIONYEAR FROM SE_EROLL.DBO.REVISIONS WHERE REVISIONNO=(SELECT ";
                qry += "MAX(REVISIONNO) FROM SE_EROLL.DBO.REVISIONS WHERE ISACTIVE=1)";
                ds = dm.create_dataset(qry);
                qry = ds.Tables[0].Rows[0]["REVISIONNO"].ToString();
                md.revisionNo = int.Parse(qry);
                md.revisionYear = int.Parse(ds.Tables[0].Rows[0]["REVISIONYEAR"].ToString());
               
                if (HttpContext.Session.GetString("panMun") == "P")
                {
                    qry = "SELECT TCODE,TNAME FROM TEHSIL WHERE E_ROLL=1 ORDER BY TNAME";
                    ds = dm.create_dataset(qry);
                    ViewBag.addressTehsils = ds;
                }
                else if (HttpContext.Session.GetString("pan_mun") == "M")
                {
                    qry = "SELECT TCODE,TNAME FROM TEHSIL WHERE E_ROLL=1 AND TCODE IN (SELECT TCODE FROM MUNICIPALS) ORDER BY TNAME";
                    ds = dm.create_dataset(qry);
                    ViewBag.addressTehsils = ds;
                }
                else
                {
                    return RedirectToAction("Home");
                }
                md.addressTehsil = "0";
                qry = "SELECT VCODE,VNAME FROM VILLAGE WHERE TCODE=" + md.addressTehsil + " ORDER BY VNAME";
                ds = dm.create_dataset(qry);
                ViewBag.villages = ds;
                return View(md);
            }
            
        }
        [HttpPost]
        public IActionResult DeleteElector(DeleteModel md)
        {
           // try
            //{
                byte[] a;
                if (!HttpContext.Session.TryGetValue("epicNo", out a))
                {
                     return RedirectToAction("Home");
                }
                else
                {
                    if (md.searchBy == "P")
                    {
                        qry = "SELECT MAX(PART_NO) FROM SE_EROLL.DBO.PARTLIST WHERE PAN_MUN='" + md.panMun + "' AND AREA_TYPE='" + md.panMun + "'";
                        ds = dm.create_dataset(qry);
                        ViewBag.maxPart = ds.Tables[0].Rows[0][0];
                        qry = "SELECT ISNULL(MAX(SLNOINPART),1) FROM SE_EROLL.DBO.E_DETAIL WHERE PAN_MUN='" + md.panMun + "'";
                        // return qry;
                        ds = dm.create_dataset(qry);
                        ViewBag.slNo = ds.Tables[0].Rows[0][0];
                    }
                    qry = "SELECT REVISIONNO,QUALIFYINGDATE,REVISIONYEAR FROM SE_EROLL.DBO.REVISIONS WHERE REVISIONNO=(SELECT ";
                    qry += "MAX(REVISIONNO) FROM SE_EROLL.DBO.REVISIONS WHERE ISACTIVE=1)";
                    ds = dm.create_dataset(qry);
                    qry = ds.Tables[0].Rows[0]["REVISIONNO"].ToString();
                    ViewBag.revisionNo = int.Parse(qry);
                    string ry = ds.Tables[0].Rows[0]["REVISIONYEAR"].ToString();
                    ViewBag.revisionYear = ry;
                    if (HttpContext.Session.GetString("panMun") == "P")
                    {
                        qry = "SELECT TCODE,TNAME FROM TEHSIL WHERE E_ROLL=1 ORDER BY TNAME";
                        ds = dm.create_dataset(qry);
                        ViewBag.addressTehsils = ds;
                    }
                    else if (HttpContext.Session.GetString("pan_mun") == "M")
                    {
                        qry = "SELECT TCODE,TNAME FROM TEHSIL WHERE E_ROLL=1 AND TCODE IN (SELECT TCODE FROM MUNICIPALS) ORDER BY TNAME";
                        ds = dm.create_dataset(qry);
                        ViewBag.addressTehsils = ds;
                    }
                    else
                    {
                         return RedirectToAction("Home");
                    }
                    
                    qry = "SELECT VCODE,VNAME FROM VILLAGE WHERE TCODE=" + md.addressTehsil + " ORDER BY VNAME";
                    ds = dm.create_dataset(qry);
                    ViewBag.villages = ds;
                    ViewBag.searchBy = md.searchBy;
                }
                if (md.fetch=="true")
                {
                    if (TempData.ContainsKey("qry"))
                    {
                       
                        qry = TempData["qry"].ToString();
                        ds = dm.create_dataset(qry);
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            DataRow row = ds.Tables[0].Rows[0];
                            md.fetch = "true";
                            md.partNo = row["PART_NO"].ToString();
                            md.slNo = row["SLNOINPART"].ToString();
                            md.epic = row["EPIC_NO"].ToString();
                            md.ename = row["ENAME"].ToString();
                            md.rlnName = row["RLN_NAME"].ToString();
                            md.panchayat = row["PAN_NAME"].ToString();
                            md.ward = row["PART_NAME"].ToString();
                        }
                        else
                        {
                           // md.fetch = "false";
                            md.partNo = "1";
                            md.slNo = "1";
                            //md.epic = "";
                            md.ename = "a";
                            md.rlnName = "";
                            md.panchayat = "";
                            md.ward = "";
                        }
                    }
                }
                return View(md);
                //return qry;
            //}
            //catch (Exception ex)
            //{
            //    //return qry;
            //    return RedirectToAction("Home");
            //}
        }
        public IActionResult FetchRecord(DeleteModel md)
        {
            string tableName;
            string wardColumn;
            string pcode;
            if (md.panMun == "P")
            {
                tableName = "PANCHAYAT";
                wardColumn = "PAN_NAME";
                pcode = "PCODE";
            }
            else
            {
                tableName = "MUN_WARD";
                wardColumn = "WARD_NAME";
                pcode = "WARD_NO";
            }
            switch (md.searchBy)
            {
                case "P":
                {
                    qry = "SELECT P." + wardColumn + " AS PAN_NAME,W.PART_NAME,E.PART_NO,E.SLNOINPART,E.ENAME,E.RLN_NAME,E.AGE,E.EPIC_NO,";
                    qry += "E.GENDER FROM SE_EROLL.DBO.E_DETAIL AS E JOIN SE_EROLL.DBO.PARTLIST AS W ON E.PART_NO=W.PART_NO AND ";
                    qry += "E.PAN_MUN=W.PAN_MUN JOIN " + tableName + " AS P ON W.PCODE=P." + pcode + " WHERE E.PART_NO =" + md.partNo + " AND E.SLNOINPART =" + md.slNo;
                    qry += " AND W.PAN_MUN='" + md.panMun + "' AND W.AREA_TYPE='" + md.panMun + "' AND E.CHNGLISTNO IS NULL";
                    break;
                }
                case "E":
                {
                    qry = "SELECT P." + wardColumn + " AS PAN_NAME,W.PART_NAME,E.PART_NO,E.SLNOINPART,E.ENAME,E.RLN_NAME,E.AGE,E.EPIC_NO,";
                    qry += "E.GENDER FROM SE_EROLL.DBO.E_DETAIL AS E JOIN SE_EROLL.DBO.PARTLIST AS W ON E.PART_NO=W.PART_NO AND ";
                    qry += "E.PAN_MUN=W.PAN_MUN JOIN " + tableName + " AS P ON W.PCODE=P." + pcode + " WHERE E.EPIC_NO='" + md.epic + "' ";
                    qry += "AND W.PAN_MUN='" + md.panMun + "' AND W.AREA_TYPE='" + md.panMun + "' AND E.CHNGLISTNO IS NULL";
                    break;
                }
            }
            TempData["qry"] = qry;
            return RedirectToActionPreserveMethod("DeleteElector");
            //return md.postCause;
        }

        public IActionResult returnString(string qry)
        {
            ViewBag.returnString = qry;
            return View();
        }

        public IActionResult SubmitDelete(DeleteModel md)
        {
            string addline1,addline2,vcode,post,email,mobile,remarks;
            if (!md.addLine1.IsNullOrEmpty())
            {
                addline1 = "'" + md.addLine1.Trim() + "'";
            }
            else
            {
                addline1 = "NULL";
            }
            if (!md.addLine2.IsNullOrEmpty())
            {
                addline2 = "'" + md.addLine2.Trim() + "'";
            }
            else
            {
                addline2 = "NULL";
            }
            
            if (md.village.IsNullOrEmpty())
            {
                vcode = "NULL";
            }
            else
            {
                vcode = md.village.ToString();
            }

            if (!md.post.IsNullOrEmpty())
            {
                post = "'" + md.post.Trim() + "'";
            }
            else
            {
                post = "NULL";
            }
            if (!md.email.IsNullOrEmpty())
            {
                email = "'" + md.email.Trim() + "'";
            }
            else
            {
                email = "NULL";
            }


            if (md.mobileNo.IsNullOrEmpty())
            {
                mobile = "NULL";
            }
            else
            {
                mobile = md.mobileNo.ToString();
            }
            if (md.remarks.IsNullOrEmpty())
            {
                remarks = "NULL";
            }
            else
            {
                remarks = md.remarks.ToString();
            }

            qry = "INSERT INTO SE_EROLL.DBO.FORMS(FORM_TYPE,ONLINE_FORM,APPLICANT_ID,PAN_MUN,PART_NO,SLNOINPART,ENAME,RLN_NAME,EPIC_NO,";
            qry += "HOUSE_NO,ADDRESS_LINE1,ADDRESS_LINE2,POSTOFF,VCODE,REVISIONNO,REVISIONYEAR,MOBILENO,EMAIL,DELETE_REASON,REMARKS) VALUES(";
            qry += "'D',1,'" + md.applEpic + "','" + md.panMun + "'," + md.partNo + "," + md.slNo + ",'" + md.ename + "','" + md.rlnName + "','" + md.epic + "','";
            qry += md.houseNo + "'," + addline1 + "," + addline2 + "," + post + "," + vcode + ",";
            qry += md.revisionNo + "," + md.revisionYear + "," + mobile + "," + email + ",'" + md.reason + "','" + md.remarks + "') SELECT @@IDENTITY";
            decimal formid = dm.insertRecord(qry);
            qry = "SELECT FORM_NO,FORM_DATE FROM SE_EROLL.DBO.FORMS WHERE FORMID=" + formid;
            ds = dm.create_dataset(qry);
            string formno = "";
            formno = ds.Tables[0].Rows[0][0].ToString();
            DateTime formDate = (DateTime)ds.Tables[0].Rows[0][1];
            formno = "ON" + formDate.Day.ToString().PadLeft(2, '0') + formDate.Month.ToString().PadLeft(2, '0') + formDate.Year.ToString() + formno.PadLeft(5, '0');
            TempData["message"] = "Request for deletion of Names Submitted Successfully. Please note the Form No. for any future reference. Your Form No. is : " + formno;
            return RedirectToAction("CitizenMessage");
        }




        #endregion

        #region MESSAGE FORM
        public IActionResult CitizenMessage()
        {
            return View();
        }
        #endregion

        #region DISPLAY RESULTS
        public IActionResult ViewResults()
        {
            return RedirectToAction("DisplayResults");
        }
        public IActionResult DisplayResults()
        {
            ResultModel md = new ResultModel();
            md.panMun = "P";
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
            qry = "SELECT CONST_CODE,CONST_NAME FROM CONSTITUENCY WHERE TYPE_CODE LIKE " + md.constType;
            qry += " AND PCODE=" + md.panchayat + " ORDER BY CONST_NAME";
            ds = dm.create_dataset(qry);
            ViewBag.constituencies = ds;
            md.constCode = ds.Tables[0].Rows[0][0].ToString();
            qry = "SELECT PSCODE,PS_NAME FROM POLLING_STATION AS PS JOIN CONSTITUENCY AS C ON PS.CONST_NO=C.CONST_NO ";
            qry += "AND PS.PAN_MUN=C.PAN_MUN AND C.PAN_MUN='" + md.panMun + "' WHERE C.CONST_NO=(SELECT CONST_NO FROM CONSTITUENCY WHERE ";
            qry += "CONST_CODE=" + md.constCode + ")";
            ds = dm.create_dataset(qry);
            ViewBag.pollingStations = ds;
            qry = "SELECT COUNT(*) FROM POLLING_STATION WHERE PAN_MUN='" + md.panMun + "'";
            ViewBag.psCnt = dm.create_scalar(qry);
            qry = "SELECT N.CAND_NAME,P.SHORT_NAME,N.VOTES,N.MARGIN,N.WIN_STATUS FROM NOMINATIONS AS N JOIN PARTY AS P ON ";
            qry += "N.PACODE=P.PACODE WHERE CONST_CODE=" + md.constCode + " ORDER BY VOTES DESC";
            ds = dm.create_dataset(qry);
            ViewBag.resultList = ds;
            //List<string> label = new List<string>();
            //List<string> mydata = new List<string>();
            int candCnt = ds.Tables[0].Rows.Count;
            string[] colors = { "red", "green", "blue", "orange", "yellow", "brown", "violet", "darkcyan", "pink", "indigo", "maroon", "purple", "navy", "skyblue", "yellowgreen", "wheat" };
            string[] label = new string[candCnt], mydata = new string[candCnt],bgcolors = new string[candCnt];
            if (ds.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i <= ds.Tables[0].Rows.Count - 1; i++)
                {
                    label[i] = ds.Tables[0].Rows[i]["CAND_NAME"].ToString();
                    mydata[i] = ds.Tables[0].Rows[i]["VOTES"].ToString();
                    bgcolors[i] = colors[i];
                }
            }
            ViewBag.labels = label;
            ViewBag.mydata = mydata;
            ViewBag.bgcolors = bgcolors;
            return View(md);

        }

        [HttpPost]
        public IActionResult DisplayResults(ResultModel md)
        {
           
            qry = "SELECT TYPE_CODE,TYPE_NAME FROM CONST_TYPE_MASTER WHERE PAN_MUN='" + md.panMun + "' ORDER BY TYPE_NAME";
            ds = dm.create_dataset(qry);
            ViewBag.constTypes = ds;
            qry = "SELECT DIST_CODE,DIST_NAME FROM DISTRICT ORDER BY DIST_CODE";
            ds = dm.create_dataset(qry);
            ViewBag.districts = ds;
            if (md.postCause == "divConstType")
                md.district= ds.Tables[0].Rows[0][0].ToString();
            qry = "SELECT TCODE,TNAME FROM TEHSIL WHERE E_ROLL=1 ORDER BY TNAME";
            ds = dm.create_dataset(qry);
            ViewBag.tehsils = ds;
            if (md.postCause == "divConstType")
                md.tehsil = ds.Tables[0].Rows[0][0].ToString();
            qry = "SELECT PNO,PAN_NAME FROM PANCHAYAT WHERE TCODE=" + md.tehsil + " ORDER BY PAN_NAME";
            ds = dm.create_dataset(qry);
            if (md.postCause == "divTehsil")
                md.panchayat = ds.Tables[0].Rows[0][0].ToString();
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
                if (md.postCause != "ddwnConstituency")
                    md.constCode = ds.Tables[0].Rows[0][0].ToString();
                if (md.constType == "4")
                {
                    qry = "SELECT PSCODE,PS_NAME FROM POLLING_STATION AS PS JOIN CONSTITUENCY AS C ON PS.CONST_NO=C.CONST_NO ";
                    qry += "AND PS.PAN_MUN=C.PAN_MUN WHERE C.PAN_MUN='" + md.panMun + "' AND ZILLA_CODE=(SELECT CONST_NO FROM ";
                    qry += "CONSTITUENCY WHERE CONST_CODE=" + md.constCode + ")";
                }
                else if (md.constType == "1")
                {
                    qry = "SELECT PSCODE,PS_NAME FROM POLLING_STATION AS PS JOIN CONSTITUENCY AS C ON PS.CONST_NO=C.CONST_NO ";
                    qry += "AND PS.PAN_MUN=C.PAN_MUN AND C.PAN_MUN='" + md.panMun + "' AND PCODE=" + md.panchayat;
                }
                else
                {
                    qry = "SELECT PSCODE,PS_NAME FROM POLLING_STATION AS PS JOIN CONSTITUENCY AS C ON PS.CONST_NO=C.CONST_NO ";
                    qry += "AND PS.PAN_MUN=C.PAN_MUN AND C.PAN_MUN='" + md.panMun + "' AND PCODE=(SELECT CONST_NO FROM CONSTITUENCY WHERE CONST_CODE=" + md.constCode + ")";
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
                qry += "CONST_CODE=" + md.constCode + ")";
                ds = dm.create_dataset(qry);
                ViewBag.pollingStations = ds;
            }
            else
            {
                md.panMun = "P";
            }
            if (md.postCause == "ddwnConstituency")
                md.constCode = HttpContext.Request.Form["constCode"];
            qry = "SELECT N.CAND_NAME,P.SHORT_NAME,N.VOTES,N.MARGIN,N.WIN_STATUS FROM NOMINATIONS AS N JOIN PARTY AS P ON ";
            qry += "N.PACODE=P.PACODE WHERE CONST_CODE=" + md.constCode + " ORDER BY VOTES DESC";
            ds = dm.create_dataset(qry);
            ViewBag.resultList = ds;
            int candCnt = ds.Tables[0].Rows.Count;
            string[] colors = { "red", "green", "blue",  "indigo", "hotpink", "violet", "brown", "darkcyan", "orange", "yellow", "maroon", "purple", "navy", "skyblue", "yellowgreen", "wheat" };
            string[] label = new string[candCnt], mydata = new string[candCnt], bgcolors = new string[candCnt];
            if (ds.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i <= ds.Tables[0].Rows.Count - 1; i++)
                {
                    label[i] = ds.Tables[0].Rows[i]["CAND_NAME"].ToString();
                    mydata[i] = ds.Tables[0].Rows[i]["VOTES"].ToString();
                    bgcolors[i] = colors[i];
                }
            }
            ViewBag.labels = label;
            ViewBag.mydata = mydata;
            ViewBag.bgcolors = bgcolors;
            

            return View(md);
        }
        #endregion
    }
}
