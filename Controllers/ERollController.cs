//using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using Microsoft.SqlServer.Server;
using SEMS.Models;
using SEMS.Models.Reports;
using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace SEMS.Controllers
{
    public class ERollController : Controller
    {
        string qry;
        //static string constr = "";
        DataSet ds = new DataSet();
        SqlConnection con = new SqlConnection();
        private readonly ElectionDBContext myDbContext;
        private readonly ILogger<ERollController> _logger;
        private readonly IConfiguration configuration;
        RevisionModel md = new RevisionModel();
        DataManager dm = new DataManager(HomeController.constr);
        public ERollController(ILogger<ERollController> logger, ElectionDBContext myDbContext, IConfiguration configuration)
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

        #region SCHEDULE REVISION
        public IActionResult ScheduleRevision()
        {
            return View(md);
        }
        [HttpPost]
        public IActionResult ScheduleRevision(RevisionModel md)
        {
           
            if (TempData.ContainsKey("postCause"))
            {
                    if (TempData["postCause"].ToString() == "fetch")
                    {
                        qry = "SELECT * FROM SE_EROLL.DBO.REVISIONS WHERE REVISIONNO=" + md.revisionNo + " AND REVISIONYEAR=";
                        qry += md.revisionYear;
                        ds = dm.create_dataset(qry);
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            md.revisionDate = (DateTime)ds.Tables[0].Rows[0]["REVISIONDATE"];
                            md.ageAsOn = (DateTime)ds.Tables[0].Rows[0]["AGEASON"];
                            md.supplementName = ds.Tables[0].Rows[0]["SUPPLEMENTNAME"].ToString();
                            md.publicationDate = (DateTime)ds.Tables[0].Rows[0]["PUBLICATIONDATE"];
                            md.qualifyingDate = (DateTime)ds.Tables[0].Rows[0]["QUALIFYINGDATE"];
                            md.erollHeader = ds.Tables[0].Rows[0]["EROLL_HEADER"].ToString();
                            md.supplementType = ds.Tables[0].Rows[0]["SUPPLEMENTTYPE"].ToString();
                            ViewBag.recordFound = true;

                        }
                        else
                        {
                            ViewBag.recordFound = false;
                        }

                    }
                    else
                    {
                        ViewBag.recordFound = false;
                    }
            }
            if (TempData.ContainsKey("inserted"))
            {
                if ((bool)TempData["inserted"] == true)
                {
                    ViewBag.recordFound = true;
                }
            }
            if (TempData.ContainsKey("updated"))
            {
                if ((bool)TempData["updated"] == true)
                {
                    ViewBag.recordFound = true;
                }
            }
                     
           
            return View(md);
        }
        public IActionResult ClearRevision()
        {
            return RedirectToAction("ScheduleRevision");
        }
        public IActionResult FetchRevision(RevisionModel md)
        {
            TempData["postCause"] = "fetch";
            return RedirectToActionPreserveMethod("ScheduleRevision");
        }
        public IActionResult CreateRevision(RevisionModel md)
        {
            qry = "INSERT INTO SE_EROLL.DBO.REVISIONS(REVISIONNO,REVISIONYEAR,REVISIONDATE,AGEASON,SUPPLEMENTNAME,PUBLICATIONDATE,";
            qry += "QUALIFYINGDATE,EROLL_HEADER,SUPPLEMENTTYPE,ISACTIVE) VALUES(" + md.revisionNo + "," + md.revisionYear + ",'";
            qry += md.revisionDate +"','" + md.ageAsOn + "','" + md.supplementName + "','" + md.publicationDate + "','" + md.qualifyingDate + "','";
            qry += md.erollHeader + "','" + md.supplementType + "',1)";
            dm.runquery(qry);
            TempData["inserted"] = true;
           
            return RedirectToActionPreserveMethod("ScheduleRevision");
        }
        public IActionResult UpdateRevision(RevisionModel md)
        {
            qry = "UPDATE SE_EROLL.DBO.REVISIONS SET REVISIONDATE='" + md.revisionDate + "',AGEASON='" + md.ageAsOn + "',SUPPLEMENTNAME='" + md.supplementName + "',";
            qry += "PUBLICATIONDATE='" + md.publicationDate + "',QUALIFYINGDATE='" + md.qualifyingDate + "',EROLL_HEADER='";
            qry += md.erollHeader + "',SUPPLEMENTTYPE='" + md.supplementType + "' WHERE REVISIONNO=" + md.revisionNo + " AND REVISIONYEAR=" + md.revisionYear;
            dm.runquery(qry);
            TempData["updated"] = true;
            return RedirectToActionPreserveMethod("ScheduleRevision");
        }
        #endregion

        #region FLOW SETTINGS

        public IActionResult FlowSettings()
        {
            qry = "SELECT STAGE_ID,STAGE,FLOW_LEVEL,USERTYPEID,CASE S.STATUS WHEN 1 THEN 'Active' ELSE 'Deactivated' ";
            qry += "END AS STATUS,U.USER_TYPE FROM SE_EROLL.DBO.FORM_STAGES AS S JOIN USER_TYPE AS U ON S.USERTYPEID=";
            qry += "U.TYPE_ID ORDER BY FLOW_LEVEL";
            ds = dm.create_dataset(qry);
            ViewBag.stages = ds;
            qry = "SELECT * FROM USER_TYPE WHERE EROLL_FORMS=1 AND STATUS=1 ORDER BY USER_TYPE";
            ds = dm.create_dataset(qry);
            ViewBag.userType = ds;
            return View(); 
        }
        [HttpPost]
        public IActionResult FlowSettings(FlowModel md)
        {
            qry = "SELECT STAGE_ID,STAGE,FLOW_LEVEL,USERTYPEID,CASE S.STATUS WHEN 1 THEN 'Active' ELSE 'Deactivated' ";
            qry += "END AS STATUS,U.USER_TYPE FROM SE_EROLL.DBO.FORM_STAGES AS S JOIN USER_TYPE AS U ON S.USERTYPEID=";
            qry += "U.TYPE_ID ORDER BY FLOW_LEVEL";
            ds = dm.create_dataset(qry);
            ViewBag.stages = ds;
            qry = "SELECT * FROM USER_TYPE WHERE EROLL_FORMS=1 AND STATUS=1 ORDER BY USER_TYPE";
            ds = dm.create_dataset(qry);
            ViewBag.userType = ds;
            if (TempData.ContainsKey("postCause"))
            {
                ViewBag.postCause = TempData["postCause"].ToString();
                if (TempData["postCause"].ToString() == "editStage")
                {
                    ViewBag.stage_id = TempData["stage_id"].ToString();
                }
            }
            return View(md);
        }
        public IActionResult EditStage(int id)
        {
            TempData["postCause"] = "editStage";
            TempData["stage_id"] = id;
            
            return RedirectToActionPreserveMethod("FlowSettings");
        }
        public IActionResult UpdateStage(int id)
        {
            TempData["postCause"] = "updateStage";
            TempData["stage_id"] = id;
            string stage = HttpContext.Request.Form["stage"];
            string level = HttpContext.Request.Form["flowLevel"];
            string status = HttpContext.Request.Form["status"];
            string userTypeID= HttpContext.Request.Form["userTypeID"];

            qry = "UPDATE SE_EROLL.DBO.FORM_STAGES SET STAGE='" + stage + "',FLOW_LEVEL=" + level + ",STATUS=" + status;
            qry += ",USERTYPEID=" + userTypeID + " WHERE STAGE_ID=" + id;
            dm.runquery(qry);
            return RedirectToActionPreserveMethod("FlowSettings");
        }

        public IActionResult AddStage()
        {
            TempData["postCause"] = "addStage";
            return RedirectToActionPreserveMethod("FlowSettings");
        }
        public IActionResult DeleteStage(int id)
        {
            TempData["postCause"] = "deleteStage";
            string item = HttpContext.Request.Form["hidDeleteItem"].ToString();
            qry = "DELETE FROM SE_EROLL.DBO.FORM_STAGES WHERE STAGE_ID=" + item;
            dm.runquery(qry);
            return RedirectToActionPreserveMethod("FlowSettings");
        }
        public IActionResult SaveStage(FlowModel md)
        {
            qry = "INSERT INTO SE_EROLL.DBO.FORM_STAGES(STAGE,FLOW_LEVEL,STATUS,USERTYPEID) VALUES('" + md.stage + "',";
            qry += md.flowLevel + ",1," + md.userTypeID + ")";
            dm.runquery(qry);
            return RedirectToAction("FlowSettings");
        }

        #endregion

        #region RE-ORAGANIZE ELECTORS
        public IActionResult ReOrganizeElectorsPanchayat()
        {
            qry = "SELECT *FROM FREEZE_MASTER WHERE F_ID=22";
            ds = dm.create_dataset(qry);
            if (ds.Tables[0].Rows.Count == 0)
            {
                HttpContext.Session.SetString("errorMessage", "Electoral Roll is Freezed. You cannot do any changes in the Roll now. For any assistance, Please contact Administrator.......");
                return RedirectToAction("AuthorizationError", "Home");
            }
            else if ((bool)ds.Tables[0].Rows[0]["FREEZED"] == true)
            {
                HttpContext.Session.SetString("errorMessage", "Electoral Roll is Freezed. You cannot do any changes in the Roll now. For any assistance, Please contact Administrator.......");
                return RedirectToAction("AuthorizationError", "Home");
            }
            string electionType = HttpContext.Session.GetString("electionType");
            PanchayatHeaderModel md = new PanchayatHeaderModel();
            if (electionType == "P")
            {
                qry = "SELECT 1 AS ORDERBY, PCODE,PAN_NAME FROM PANCHAYAT WHERE TCODE like (SELECT ISNULL(CAST(TCODE AS VARCHAR),'%')  FROM USERS ";
                qry += "WHERE UID=" + HttpContext.Session.GetString("logUserID") + ") ";
                qry += "UNION SELECT 2 AS ORDERBY,AREA_CODE AS PCODE,AREA_NAME AS PAN_NAME FROM SE_EROLL.DBO.AREA WHERE RESERVED=1 ORDER BY ORDERBY,PAN_NAME";
                ds = dm.create_dataset(qry);
                ViewBag.panchayats = ds;
                // qry = "SELECT CONST_NO,WARD_NAME FROM PAN_WARD WHERE PCODE=" + ds.Tables[0].Rows[0]["PCODE"] + " ORDER BY WARD_NAME";
                qry = "SELECT PART_NO AS CONST_NO,PART_NAME AS WARD_NAME FROM SE_EROLL.DBO.PARTLIST WHERE PCODE=" + ds.Tables[0].Rows[0]["PCODE"] + " AND PAN_MUN='P' AND TCODE LIKE  ";
                qry += "(SELECT ISNULL(CAST(TCODE AS VARCHAR),'%')  FROM USERS WHERE UID=" + HttpContext.Session.GetString("logUserID") + ") ";
                ds = dm.create_dataset(qry);
                ViewBag.wards = ds;
                //qry = "SELECT CONST_NO,WARD_NAME FROM PAN_WARD WHERE PCODE=" + md.selectedPanchayat + " ORDER BY WARD_NAME";
                qry = "SELECT PART_NO AS CONST_NO,PART_NAME AS WARD_NAME FROM SE_EROLL.DBO.PARTLIST WHERE PCODE=" + md.selectedPanchayat + " AND PAN_MUN='P' AND TCODE LIKE  ";
                qry += "(SELECT ISNULL(CAST(TCODE AS VARCHAR),'%')  FROM USERS WHERE UID=" + HttpContext.Session.GetString("logUserID") + ") ";
                ds = dm.create_dataset(qry);
                ViewBag.selectedWard = ds;
                /*qry = "SELECT E.EID,PART_NO,SLNOINPART, ENAME, CASE RLN_TYPE WHEN 'F' THEN 'Father' WHEN 'M' THEN 'Mother' WHEN 'H' THEN ";
                qry += "'Husband' ELSE 'Others' END AS RLN_TYPE,RLN_NAME,EPIC_NO,P.PCODE,W.WARD_NAME,P.PAN_NAME,W.CONST_NO,W.WARD_NAME FROM ";
                qry += "SE_EROLL.DBO.E_DETAIL AS E JOIN PAN_WARD AS W ON ISNULL(E.SHIFTED_PART_NO,PART_NO) = W.CONST_NO JOIN PANCHAYAT AS P ON W.PCODE = ";
                qry += "P.PCODE WHERE ISNULL(E.SHIFTED_PART_NO,PART_NO) = " + md.ward + " ORDER BY SLNOINPART";*/
                qry = "SELECT E.EID,E.PART_NO,SLNOINPART, ENAME, CASE RLN_TYPE WHEN 'F' THEN 'Father' WHEN 'M' THEN 'Mother' WHEN 'H' THEN ";
                qry += "'Husband' ELSE 'Others' END AS RLN_TYPE,RLN_NAME,EPIC_NO,P.PCODE,W.PART_NAME,P.PAN_NAME,W.PART_NO AS CONST_NO,W.PART_NAME FROM ";
                qry += "SE_EROLL.DBO.E_DETAIL AS E JOIN SE_EROLL.DBO.PARTLIST AS W ON ISNULL(E.SHIFTED_PART_NO,E.PART_NO) = W.PART_NO AND ";
                qry += "E.PAN_MUN=W.PAN_MUN JOIN PANCHAYAT_VW AS P ON W.PCODE = P.PCODE WHERE ISNULL(E.SHIFTED_PART_NO,E.PART_NO) =" + md.ward;
                qry += "AND E.PAN_MUN='P' ORDER BY SLNOINPART";
                ds = dm.create_dataset(qry);
                ViewBag.eList = ds;
            }
            else if (electionType == "M")
            {
                qry = "SELECT 1 AS ORDERBY, WARD_NO AS PCODE,WARD_NAME AS PAN_NAME FROM MUN_WARD WHERE TCODE like (SELECT ISNULL(CAST(TCODE AS VARCHAR),'%')  FROM USERS ";
                qry += "WHERE UID=" + HttpContext.Session.GetString("logUserID") + ") ";
                qry += "UNION SELECT 2 AS ORDERBY,AREA_CODE AS PCODE,AREA_NAME AS PAN_NAME FROM SE_EROLL.DBO.AREA WHERE RESERVED=1 ORDER BY ORDERBY,PCODE";
                ds = dm.create_dataset(qry);
               
                ViewBag.panchayats = ds;
                // qry = "SELECT CONST_NO,WARD_NAME FROM PAN_WARD WHERE PCODE=" + ds.Tables[0].Rows[0]["PCODE"] + " ORDER BY WARD_NAME";
                qry = "SELECT PART_NO AS CONST_NO,PART_NAME AS WARD_NAME FROM SE_EROLL.DBO.PARTLIST WHERE PCODE=" + ds.Tables[0].Rows[0]["PCODE"] + " AND PAN_MUN='M' AND TCODE LIKE  ";
                qry += "(SELECT ISNULL(CAST(TCODE AS VARCHAR),'%')  FROM USERS WHERE UID=" + HttpContext.Session.GetString("logUserID") + ") ";
                ds = dm.create_dataset(qry);
                ViewBag.wards = ds;
                //qry = "SELECT CONST_NO,WARD_NAME FROM PAN_WARD WHERE PCODE=" + md.selectedPanchayat + " ORDER BY WARD_NAME";
                qry = "SELECT PART_NO AS CONST_NO,PART_NAME AS WARD_NAME FROM SE_EROLL.DBO.PARTLIST WHERE PCODE=" + md.selectedPanchayat + " AND PAN_MUN='M' AND TCODE LIKE  ";
                qry += "(SELECT ISNULL(CAST(TCODE AS VARCHAR),'%')  FROM USERS WHERE UID=" + HttpContext.Session.GetString("logUserID") + ") ";
                ds = dm.create_dataset(qry);
                ViewBag.selectedWard = ds;
                /*qry = "SELECT E.EID,PART_NO,SLNOINPART, ENAME, CASE RLN_TYPE WHEN 'F' THEN 'Father' WHEN 'M' THEN 'Mother' WHEN 'H' THEN ";
                qry += "'Husband' ELSE 'Others' END AS RLN_TYPE,RLN_NAME,EPIC_NO,P.PCODE,W.WARD_NAME,P.PAN_NAME,W.CONST_NO,W.WARD_NAME FROM ";
                qry += "SE_EROLL.DBO.E_DETAIL AS E JOIN PAN_WARD AS W ON ISNULL(E.SHIFTED_PART_NO,PART_NO) = W.CONST_NO JOIN PANCHAYAT AS P ON W.PCODE = ";
                qry += "P.PCODE WHERE ISNULL(E.SHIFTED_PART_NO,PART_NO) = " + md.ward + " ORDER BY SLNOINPART";*/
                qry = "SELECT E.EID,E.PART_NO,SLNOINPART, ENAME, CASE RLN_TYPE WHEN 'F' THEN 'Father' WHEN 'M' THEN 'Mother' WHEN 'H' THEN ";
                qry += "'Husband' ELSE 'Others' END AS RLN_TYPE,RLN_NAME,EPIC_NO,P.PCODE,W.PART_NAME,P.PAN_NAME,W.PART_NO AS CONST_NO,W.PART_NAME FROM ";
                qry += "SE_EROLL.DBO.E_DETAIL AS E JOIN SE_EROLL.DBO.PARTLIST AS W ON ISNULL(E.SHIFTED_PART_NO,E.PART_NO) = W.PART_NO AND ";
                qry += "E.PAN_MUN=W.PAN_MUN JOIN MUNICIPAL_VW AS P ON W.PCODE = P.PCODE WHERE ISNULL(E.SHIFTED_PART_NO,E.PART_NO) =" + md.ward;
                qry += "AND E.PAN_MUN='M' ORDER BY SLNOINPART";
                ds = dm.create_dataset(qry);
                ViewBag.eList = ds;
            }
            else
            {
                RedirectToAction("Login", "Home");
            }
            return View();
        }
        [HttpPost]
        public IActionResult ReOrganizeElectorsPanchayat(PanchayatHeaderModel md)
        {
            qry = "SELECT *FROM FREEZE_MASTER WHERE F_ID=22";
            ds = dm.create_dataset(qry);
            if (ds.Tables[0].Rows.Count == 0)
            {
                HttpContext.Session.SetString("errorMessage", "Electoral Roll is Freezed. You cannot do any changes in the Roll now. For any assistance, Please contact Administrator.......");
                return RedirectToAction("AuthorizationError", "Home");
            }
            else if ((bool)ds.Tables[0].Rows[0]["FREEZED"] == true)
            {
                HttpContext.Session.SetString("errorMessage", "Electoral Roll is Freezed. You cannot do any changes in the Roll now. For any assistance, Please contact Administrator.......");
                return RedirectToAction("AuthorizationError", "Home");
            }
            string electionType = HttpContext.Session.GetString("electionType");
            if (electionType=="P")
            {
                qry = "SELECT 1 AS ORDERBY, PCODE,PAN_NAME FROM PANCHAYAT WHERE TCODE like (SELECT ISNULL(CAST(TCODE AS VARCHAR),'%')  FROM USERS ";
                qry += "WHERE UID=" + HttpContext.Session.GetString("logUserID") + ") ";
                qry += "UNION SELECT 2 AS ORDERBY,AREA_CODE AS PCODE,AREA_NAME AS PAN_NAME FROM SE_EROLL.DBO.AREA WHERE RESERVED=1 ORDER BY ORDERBY,PCODE";
                ds = dm.create_dataset(qry);
                ViewBag.panchayats = ds;
                // qry = "SELECT CONST_NO,WARD_NAME FROM PAN_WARD WHERE PCODE=" + ds.Tables[0].Rows[0]["PCODE"] + " ORDER BY WARD_NAME";
                qry = "SELECT PART_NO AS CONST_NO,PART_NAME AS WARD_NAME FROM SE_EROLL.DBO.PARTLIST WHERE PCODE=" + md.panchayat + " AND PAN_MUN='P' AND TCODE LIKE  ";
                qry += "(SELECT ISNULL(CAST(TCODE AS VARCHAR),'%')  FROM USERS WHERE UID=" + HttpContext.Session.GetString("logUserID") + ") ";
                ds = dm.create_dataset(qry);
                ViewBag.wards = ds;
                if (md.selectedPanchayat == 0)
                {
                    md.selectedPanchayat = md.panchayat;
                }
                qry = "SELECT PART_NO AS CONST_NO,PART_NAME AS WARD_NAME FROM SE_EROLL.DBO.PARTLIST WHERE PCODE=" + md.selectedPanchayat + " AND PAN_MUN='P' AND TCODE LIKE  ";
                qry += "(SELECT ISNULL(CAST(TCODE AS VARCHAR),'%')  FROM USERS WHERE UID=" + HttpContext.Session.GetString("logUserID") + ") ";
                //qry = "SELECT CONST_NO,WARD_NAME FROM PAN_WARD WHERE PCODE=" + md.selectedPanchayat + " ORDER BY WARD_NAME";
                ds = dm.create_dataset(qry);
                ViewBag.selectedWard = ds;
            }
            else if (electionType=="M")
            {
                qry = "SELECT 1 AS ORDERBY, WARD_NO AS PCODE,WARD_NAME AS PAN_NAME FROM MUN_WARD WHERE TCODE like (SELECT ISNULL(CAST(TCODE AS VARCHAR),'%')  FROM USERS ";
                qry += "WHERE UID=" + HttpContext.Session.GetString("logUserID") + ") ";
                qry += "UNION SELECT 2 AS ORDERBY,AREA_CODE AS PCODE,AREA_NAME AS PAN_NAME FROM SE_EROLL.DBO.AREA WHERE RESERVED=1 ORDER BY ORDERBY,PCODE";
                ds = dm.create_dataset(qry);
                ViewBag.panchayats = ds;
                // qry = "SELECT CONST_NO,WARD_NAME FROM PAN_WARD WHERE PCODE=" + ds.Tables[0].Rows[0]["PCODE"] + " ORDER BY WARD_NAME";
                qry = "SELECT PART_NO AS CONST_NO,PART_NAME AS WARD_NAME FROM SE_EROLL.DBO.PARTLIST WHERE PCODE=" + md.panchayat + " AND PAN_MUN='M' AND TCODE LIKE  ";
                qry += "(SELECT ISNULL(CAST(TCODE AS VARCHAR),'%')  FROM USERS WHERE UID=" + HttpContext.Session.GetString("logUserID") + ") ";
                ds = dm.create_dataset(qry);
                ViewBag.wards = ds;
                if (md.selectedPanchayat == 0)
                {
                    md.selectedPanchayat = md.panchayat;
                }
                qry = "SELECT PART_NO AS CONST_NO,PART_NAME AS WARD_NAME FROM SE_EROLL.DBO.PARTLIST WHERE PCODE=" + md.selectedPanchayat + " AND PAN_MUN='M' AND TCODE LIKE  ";
                qry += "(SELECT ISNULL(CAST(TCODE AS VARCHAR),'%')  FROM USERS WHERE UID=" + HttpContext.Session.GetString("logUserID") + ") ";
                //qry = "SELECT CONST_NO,WARD_NAME FROM PAN_WARD WHERE PCODE=" + md.selectedPanchayat + " ORDER BY WARD_NAME";
                ds = dm.create_dataset(qry);
                ViewBag.selectedWard = ds;
            }
            else
            {
                RedirectToAction("Login", "Home");
            }
            

            if (md.postCause=="edit")
            {
                ViewBag.edit = true;
                //ViewBag.qry = qry;
            }
            if (md.postCause == "update") 
            {
                qry = "SELECT ISNULL(SHIFTED_PART_NO,PART_NO) FROM SE_EROLL.DBO.E_DETAIL WHERE EID=" + md.eid;
                int part = dm.create_scalar(qry);
               if (part != md.selectedWard)
                {
                    qry = "UPDATE SE_EROLL.DBO.E_DETAIL SET SHIFTED_PART_NO=" + md.selectedWard + ",SHIFT_APPROVED=0 WHERE EID=" + md.eid;
                    dm.runquery(qry);
               }
                md.postCause = "normal";
            }
            if (md.postCause == "panchayat") 
            {
                md.ward = 0;
            }
            if (electionType=="P")
            {
                qry = "SELECT E.EID,E.PART_NO,SLNOINPART, ENAME, CASE RLN_TYPE WHEN 'F' THEN 'Father' WHEN 'M' THEN 'Mother' WHEN 'H' THEN ";
                qry += "'Husband' ELSE 'Others' END AS RLN_TYPE,RLN_NAME,EPIC_NO,P.PCODE,W.PART_NAME,P.PAN_NAME,W.PART_NO as CONST_NO,W.PART_NAME AS WARD_NAME FROM ";
                qry += "SE_EROLL.DBO.E_DETAIL AS E JOIN SE_EROLL.DBO.PARTLIST AS W ON ISNULL(E.SHIFTED_PART_NO,E.PART_NO) = W.PART_NO AND ";
                qry += "E.PAN_MUN=W.PAN_MUN JOIN PANCHAYAT_VW AS P ON W.PCODE = P.PCODE WHERE ISNULL(E.SHIFTED_PART_NO,E.PART_NO) =" + md.ward;
                qry += "AND E.PAN_MUN='P' ORDER BY SLNOINPART";
            }
            else
            {
                qry = "SELECT E.EID,E.PART_NO,SLNOINPART, ENAME, CASE RLN_TYPE WHEN 'F' THEN 'Father' WHEN 'M' THEN 'Mother' WHEN 'H' THEN ";
                qry += "'Husband' ELSE 'Others' END AS RLN_TYPE,RLN_NAME,EPIC_NO,P.PCODE,W.PART_NAME,P.PAN_NAME,W.PART_NO as CONST_NO,W.PART_NAME AS WARD_NAME FROM ";
                qry += "SE_EROLL.DBO.E_DETAIL AS E JOIN SE_EROLL.DBO.PARTLIST AS W ON ISNULL(E.SHIFTED_PART_NO,E.PART_NO) = W.PART_NO AND ";
                qry += "E.PAN_MUN=W.PAN_MUN JOIN MUNICIPAL_VW AS P ON W.PCODE = P.PCODE WHERE ISNULL(E.SHIFTED_PART_NO,E.PART_NO) =" + md.ward;
                qry += "AND E.PAN_MUN='M' ORDER BY SLNOINPART";
            }
            


            /*qry = "SELECT E.EID,PART_NO,SLNOINPART, ENAME, CASE RLN_TYPE WHEN 'F' THEN 'Father' WHEN 'M' THEN 'Mother' WHEN 'H' THEN ";
            qry += "'Husband' ELSE 'Others' END AS RLN_TYPE,RLN_NAME,EPIC_NO,P.PCODE,W.WARD_NAME,P.PAN_NAME,W.CONST_NO,W.WARD_NAME FROM ";
            qry += "SE_EROLL.DBO.E_DETAIL AS E JOIN PAN_WARD AS W ON ISNULL(E.SHIFTED_PART_NO,PART_NO) = W.CONST_NO JOIN PANCHAYAT AS P ON W.PCODE = ";
            qry += "P.PCODE WHERE ISNULL(E.SHIFTED_PART_NO,PART_NO) = " + md.ward + " ORDER BY SLNOINPART";*/
            ds = dm.create_dataset(qry);
            ViewBag.eList = ds;
            md.selectedPanchayat = md.panchayat;
            md.selectedWard = md.ward;
            return View(md);
        }


        #endregion

        #region DRAFT ELECTORAL ROLL
        public IActionResult DraftRoll()
        {
            string panMun = HttpContext.Session.GetString("electionType");
            if (panMun == "P")
            {
                qry = "SELECT PCODE,PAN_NAME FROM PANCHAYAT ORDER BY PAN_NAME";
                ds = dm.create_dataset(qry);
                ViewBag.panchayats = ds;
                qry = "SELECT PART_NO AS CONST_NO,PART_NAME AS WARD_NAME FROM SE_EROLL.DBO.PARTLIST WHERE PAN_MUN='" + panMun + "' AND PCODE=" + ds.Tables[0].Rows[0]["PCODE"] + " ORDER BY WARD_NAME";
                ds = dm.create_dataset(qry);
                ViewBag.wards = ds;
                qry = "SELECT DISTINCT REVISIONYEAR FROM SE_EROLL.DBO.REVISIONS ORDER BY REVISIONYEAR DESC";
                ds = dm.create_dataset(qry);
                ViewBag.revisionYears = ds;
            }
            else if (panMun=="M")
            {
                qry = "SELECT WARD_NO AS PCODE,WARD_NAME AS PAN_NAME FROM MUN_WARD ORDER BY PAN_NAME";
                ds = dm.create_dataset(qry);
                ViewBag.panchayats = ds;
                qry = "SELECT PART_NO AS CONST_NO,PART_NAME AS WARD_NAME FROM SE_EROLL.DBO.PARTLIST WHERE PAN_MUN='" + panMun + "' AND PCODE=" + ds.Tables[0].Rows[0]["PCODE"] + " ORDER BY WARD_NAME";
                ds = dm.create_dataset(qry);
                ViewBag.wards = ds;
                qry = "SELECT DISTINCT REVISIONYEAR FROM SE_EROLL.DBO.REVISIONS ORDER BY REVISIONYEAR DESC";
                ds = dm.create_dataset(qry);
                ViewBag.revisionYears = ds;
            }
            
            return View(); 
        }
        [HttpPost]
        public IActionResult DraftRoll(RepDraftRollModel md)
        {
            string panMun = HttpContext.Session.GetString("electionType");
            if (panMun == "P")
            {
                qry = "SELECT PCODE,PAN_NAME FROM PANCHAYAT ORDER BY PAN_NAME";
                ds = dm.create_dataset(qry);
                ViewBag.panchayats = ds;
                qry = "SELECT PART_NO AS CONST_NO,PART_NAME AS WARD_NAME FROM SE_EROLL.DBO.PARTLIST WHERE PAN_MUN='" + panMun + "' AND PCODE=" + md.panchayat + " ORDER BY WARD_NAME";
                ds = dm.create_dataset(qry);
                ViewBag.wards = ds;
                qry = "SELECT DISTINCT REVISIONYEAR FROM SE_EROLL.DBO.REVISIONS ORDER BY REVISIONYEAR DESC";
                ds = dm.create_dataset(qry);
                ViewBag.revisionYears = ds;
            }
            else if (panMun == "M")
            {
                qry = "SELECT WARD_NO AS PCODE,WARD_NAME AS PAN_NAME FROM MUN_WARD ORDER BY PAN_NAME";
                ds = dm.create_dataset(qry);
                ViewBag.panchayats = ds;
                qry = "SELECT PART_NO AS CONST_NO,PART_NAME AS WARD_NAME FROM SE_EROLL.DBO.PARTLIST WHERE PAN_MUN='" + panMun + "' AND PCODE=" + md.panchayat + " ORDER BY WARD_NAME";
                ds = dm.create_dataset(qry);
                ViewBag.wards = ds;
                qry = "SELECT DISTINCT REVISIONYEAR FROM SE_EROLL.DBO.REVISIONS ORDER BY REVISIONYEAR DESC";
                ds = dm.create_dataset(qry);
                ViewBag.revisionYears = ds;
            }


            //qry = "SELECT PCODE,PAN_NAME FROM PANCHAYAT ORDER BY PAN_NAME";
            //ds = dm.create_dataset(qry);
            //ViewBag.panchayats = ds;
            //qry = "SELECT CONST_NO,WARD_NAME FROM PAN_WARD WHERE PCODE=" + md.panchayat + " ORDER BY WARD_NAME";
            //ds = dm.create_dataset(qry);
            //ViewBag.wards = ds;
            //qry = "SELECT DISTINCT REVISIONYEAR FROM SE_EROLL.DBO.REVISIONS ORDER BY REVISIONYEAR DESC";
            //ds = dm.create_dataset(qry);
            //ViewBag.revisionYears = ds;
            return View(md);
        }

        #endregion

        #region GENERATE SERIAL NUMBERS
        public IActionResult Serialize()
        {
            bool allOK = checkAuthorization(24);
            if (!allOK)
            {
                HttpContext.Session.SetString("errorMessage", "You are not authorized to access this page. Please contact Administrator.......");
                return RedirectToAction("AuthorizationError","Home");
            }
            qry = "SELECT *FROM FREEZE_MASTER WHERE F_ID=22";
            ds = dm.create_dataset(qry);
            if (ds.Tables[0].Rows.Count == 0)
            {
                HttpContext.Session.SetString("errorMessage", "Electoral Roll is Freezed. You cannot do any changes in the Roll now. For any assistance, Please contact Administrator.......");
                return RedirectToAction("AuthorizationError", "Home");
            }
            else if ((bool)ds.Tables[0].Rows[0]["FREEZED"] == true)
            {
                HttpContext.Session.SetString("errorMessage", "Electoral Roll is Freezed. You cannot do any changes in the Roll now. For any assistance, Please contact Administrator.......");
                return RedirectToAction("AuthorizationError", "Home");
            }
            PanchayatHeaderModel md = new PanchayatHeaderModel();
            string panMun = HttpContext.Session.GetString("electionType");
            if (panMun == "P")
            {
                qry = "SELECT PCODE,PAN_NAME FROM PANCHAYAT WHERE TCODE like (SELECT ISNULL(CAST(TCODE AS VARCHAR),'%')  FROM USERS WHERE UID=";
                qry += HttpContext.Session.GetString("logUserID") + ") ORDER BY PAN_NAME";
                ds = dm.create_dataset(qry);
                ViewBag.panchayats = ds;
                qry = "SELECT PART_NO AS CONST_NO,PART_NAME AS WARD_NAME FROM SE_EROLL.DBO.PARTLIST WHERE PAN_MUN='" + panMun + "' AND PCODE=" + ds.Tables[0].Rows[0]["PCODE"] + " ORDER BY WARD_NAME";
                ds = dm.create_dataset(qry);
                ViewBag.wards = ds;
                qry = "SELECT REVISIONNO,SUPPLEMENTNAME FROM SE_EROLL.DBO.REVISIONS WHERE REVISIONYEAR=(SELECT MAX(REVISIONYEAR) FROM SE_EROLL.DBO.REVISIONS)";
                ds = dm.create_dataset(qry);
                ViewBag.revisions = ds;
            }
            else if (panMun == "M")
            {
                qry = "SELECT WARD_NO AS PCODE,WARD_NAME AS PAN_NAME FROM MUN_WARD WHERE TCODE like (SELECT ISNULL(CAST(TCODE AS VARCHAR),'%')  FROM USERS WHERE UID=";
                qry += HttpContext.Session.GetString("logUserID") + ") ORDER BY PAN_NAME";
                ds = dm.create_dataset(qry);
                ViewBag.panchayats = ds;
                qry = "SELECT PART_NO AS CONST_NO,PART_NAME AS WARD_NAME FROM SE_EROLL.DBO.PARTLIST WHERE PAN_MUN='" + panMun + "' AND PCODE=" + ds.Tables[0].Rows[0]["PCODE"] + " ORDER BY WARD_NAME";
                ds = dm.create_dataset(qry);
                ViewBag.wards = ds;
                qry = "SELECT REVISIONNO,SUPPLEMENTNAME FROM SE_EROLL.DBO.REVISIONS WHERE REVISIONYEAR=(SELECT MAX(REVISIONYEAR) FROM SE_EROLL.DBO.REVISIONS)";
                ds = dm.create_dataset(qry);
                ViewBag.revisions = ds;
            }

           
            
            return View(md);
        }
        [HttpPost]
        public IActionResult Serialize(PanchayatHeaderModel md)
        {
            bool allOK = checkAuthorization(24);
            if (!allOK)
            {
                HttpContext.Session.SetString("errorMessage", "You are not authorized to access this page. Please contact Administrator.......");
                return RedirectToAction("AuthorizationError", "Home");
            }
            qry = "SELECT *FROM FREEZE_MASTER WHERE F_ID=22";
            ds = dm.create_dataset(qry);
            if (ds.Tables[0].Rows.Count == 0)
            {
                HttpContext.Session.SetString("errorMessage", "Electoral Roll is Freezed. You cannot do any changes in the Roll now. For any assistance, Please contact Administrator.......");
                return RedirectToAction("AuthorizationError", "Home");
            }
            else if ((bool)ds.Tables[0].Rows[0]["FREEZED"] == true)
            {
                HttpContext.Session.SetString("errorMessage", "Electoral Roll is Freezed. You cannot do any changes in the Roll now. For any assistance, Please contact Administrator.......");
                return RedirectToAction("AuthorizationError", "Home");
            }
            string panMun = HttpContext.Session.GetString("electionType");
            if (panMun == "P")
            {
                qry = "SELECT PCODE,PAN_NAME FROM PANCHAYAT WHERE TCODE like (SELECT ISNULL(CAST(TCODE AS VARCHAR),'%')  FROM USERS WHERE UID=";
                qry += HttpContext.Session.GetString("logUserID") + ") ORDER BY PAN_NAME";
                ds = dm.create_dataset(qry);
                ViewBag.panchayats = ds;
                qry = "SELECT PART_NO AS CONST_NO,PART_NAME AS WARD_NAME FROM SE_EROLL.DBO.PARTLIST WHERE PAN_MUN='" + panMun + "' AND PCODE=" + md.panchayat + " ORDER BY WARD_NAME";
                ds = dm.create_dataset(qry);
                ViewBag.wards = ds;
                qry = "SELECT REVISIONNO,SUPPLEMENTNAME FROM SE_EROLL.DBO.REVISIONS WHERE REVISIONYEAR=(SELECT MAX(REVISIONYEAR) FROM SE_EROLL.DBO.REVISIONS)";
                ds = dm.create_dataset(qry);
                ViewBag.revisions = ds;
            }
            else if (panMun == "M")
            {
                qry = "SELECT WARD_NO AS PCODE,WARD_NAME AS PAN_NAME FROM MUN_WARD WHERE TCODE like (SELECT ISNULL(CAST(TCODE AS VARCHAR),'%')  FROM USERS WHERE UID=";
                qry += HttpContext.Session.GetString("logUserID") + ") ORDER BY PAN_NAME";
                ds = dm.create_dataset(qry);
                ViewBag.panchayats = ds;
                qry = "SELECT PART_NO AS CONST_NO,PART_NAME AS WARD_NAME FROM SE_EROLL.DBO.PARTLIST WHERE PAN_MUN='" + panMun + "' AND PCODE=" + md.panchayat + " ORDER BY WARD_NAME";
                ds = dm.create_dataset(qry);
                ViewBag.wards = ds;
                qry = "SELECT REVISIONNO,SUPPLEMENTNAME FROM SE_EROLL.DBO.REVISIONS WHERE REVISIONYEAR=(SELECT MAX(REVISIONYEAR) FROM SE_EROLL.DBO.REVISIONS)";
                ds = dm.create_dataset(qry);
                ViewBag.revisions = ds;
            }
            if (md.postCause == "genSlNo")
            {
                ViewBag.numberGenerated = true;
            }
            return View();
        }

        public IActionResult GenerateSlNo(PanchayatHeaderModel md)
        {
            string panMun = HttpContext.Session.GetString("electionType");
            if (md.radio=='I')
            {
                if (md.revisionNo == 0)
                {
                    qry = "EXEC SE_EROLL.DBO.SP_PARTWISE_SLNO_MOTHER_ROLL " + md.ward + "," + panMun;
                    dm.runquery(qry);
                }
                else
                {

                }
                Thread.Sleep(3000);
            }
            else
            {
                if (md.revisionNo == 0)
                {
                    qry = "SELECT DISTINCT E.PART_NO FROM SE_EROLL.DBO.E_DETAIL AS E JOIN SE_EROLL.DBO.PARTLIST AS P ON E.PART_NO=P.PART_NO ";
                    qry += "WHERE P.PAN_MUN='" + panMun + "' AND AREA_TYPE='" + panMun + "' ORDER BY PART_NO";
                    ds=dm.create_dataset(qry);
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        qry = "EXEC SE_EROLL.DBO.SP_PARTWISE_SLNO_MOTHER_ROLL " + row[0] + "," + panMun;
                        dm.runquery(qry);
                    }
                }
                else
                {

                }
                Thread.Sleep(3000);
            }
            
            return RedirectToActionPreserveMethod("Serialize");
        }
        #endregion

        #region FORM PROCESSING
        public IActionResult ListForms()
        {
            ProcessFormModel md = new ProcessFormModel();
            bool allOK = checkAuthorization(25);
            if (!allOK)
            {
                HttpContext.Session.SetString("errorMessage", "You are not authorized to access this page. Please contact Administrator.......");
                return RedirectToAction("AuthorizationError", "Home");
            }
            qry = "SELECT *FROM FREEZE_MASTER WHERE F_ID=22";
            ds = dm.create_dataset(qry);
            if (ds.Tables[0].Rows.Count == 0)
            {
                HttpContext.Session.SetString("errorMessage", "Electoral Roll is Freezed. You cannot process forms now. For any assistance, Please contact Administrator.......");
                return RedirectToAction("AuthorizationError", "Home");
            }
            else if ((bool)ds.Tables[0].Rows[0]["FREEZED"] == true)
            {
                HttpContext.Session.SetString("errorMessage", "Electoral Roll is Freezed. You cannot process forms now. For any assistance, Please contact Administrator.......");
                return RedirectToAction("AuthorizationError", "Home");
            }
            md.panMun = HttpContext.Session.GetString("electionType");
            return View(md);
        }
        [HttpPost]
        public IActionResult ListForms(ProcessFormModel md)
        {
            bool allOK = checkAuthorization(25);
            if (!allOK)
            {
                HttpContext.Session.SetString("errorMessage", "You are not authorized to access this page. Please contact Administrator.......");
                return RedirectToAction("AuthorizationError", "Home");
            }
            qry = "SELECT *FROM FREEZE_MASTER WHERE F_ID=22";
            ds = dm.create_dataset(qry);
            if (ds.Tables[0].Rows.Count == 0)
            {
                HttpContext.Session.SetString("errorMessage", "Electoral Roll is Freezed. You cannot process forms now. For any assistance, Please contact Administrator.......");
                return RedirectToAction("AuthorizationError", "Home");
            }
            else if ((bool)ds.Tables[0].Rows[0]["FREEZED"] == true)
            {
                HttpContext.Session.SetString("errorMessage", "Electoral Roll is Freezed. You cannot process forms now. For any assistance, Please contact Administrator.......");
                return RedirectToAction("AuthorizationError", "Home");
            }
            string userType = HttpContext.Session.GetString("logUserType");
            if (userType == "AERO")
            {
                if (md.panMun=="P")
                {
                    qry = "SELECT FH.FORMID,FH.STAGE_NO,FH.STAGE_ID,FH.STAGE_DATE,F.ENAME,F.RLN_NAME,F.FORM_NO,CAST(F.FORM_DATE AS DATE) ";
                    qry += "AS FORM_DATE,CASE F.ONLINE_FORM WHEN 1 THEN 'ON' ELSE 'OF' END AS ONLINEFORM,P.PAN_NAME,PL.PART_NAME,F.SLNOINPART,F.EPIC_NO FROM SE_EROLL.DBO.FORM_HISTORY AS FH ";
                    qry += "JOIN SE_EROLL.DBO.FORMS AS F ON FH.FORMID=F.FORMID  JOIN SE_EROLL.DBO.PARTLIST AS PL ON F.PART_NO=PL.PART_NO AND F.PAN_MUN=PL.PAN_MUN ";
                    qry += "JOIN PANCHAYAT AS P ON PL.PCODE=P.PCODE WHERE FH.LATEST=1 AND F.FORM_TYPE='" + md.formType +"' AND PL.PAN_MUN='P' AND FH.STAGE_ID ";
                    qry += "IN(SELECT STAGE_ID FROM SE_EROLL.DBO.FORM_STAGES WHERE USERTYPEID=(SELECT TYPE_ID FROM USER_TYPE WHERE USER_TYPE='" + userType +"'))";

                }
                else if (md.panMun=="M")
                {
                    qry = "SELECT FH.FORMID,FH.STAGE_NO,FH.STAGE_ID,FH.STAGE_DATE,F.ENAME,F.RLN_NAME,F.FORM_NO,CAST(F.FORM_DATE AS DATE) ";
                    qry += "AS FORM_DATE,CASE F.ONLINE_FORM WHEN 1 THEN 'ON' ELSE 'OF' END AS ONLINEFORM,P.WARD_NAME AS PAN_NAME,PL.PART_NAME,F.SLNOINPART,F.EPIC_NO FROM SE_EROLL.DBO.FORM_HISTORY AS FH ";
                    qry += "JOIN SE_EROLL.DBO.FORMS AS F ON FH.FORMID=F.FORMID  JOIN SE_EROLL.DBO.PARTLIST AS PL ON F.PART_NO=PL.PART_NO AND F.PAN_MUN=PL.PAN_MUN ";
                    qry += "JOIN MUN_WARD AS P ON PL.PCODE=P.WARD_NO WHERE FH.LATEST=1 AND F.FORM_TYPE='" + md.formType + "' AND PL.PAN_MUN='m' AND FH.STAGE_ID ";
                    qry += "IN(SELECT STAGE_ID FROM SE_EROLL.DBO.FORM_STAGES WHERE USERTYPEID=(SELECT TYPE_ID FROM USER_TYPE WHERE USER_TYPE='" + userType + "'))";
                }
                ds = dm.create_dataset(qry);
                DataColumn dc = new DataColumn("SNO");
                ds.Tables[0].Columns.Add(dc);
                int cnt = 0;
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    dr["SNO"] = ++cnt;
                }
                md.rowCount = cnt;
                ViewBag.forms = ds;
            }
            else if (HttpContext.Session.GetString("logUserType") == "AERO")
            {
            }
            else if (HttpContext.Session.GetString("logUserType") == "ERO")
            {

            }
            else
            {
                HttpContext.Session.SetString("errorMessage", "You are not authorized to access this page. Please contact Administrator.......");
                return RedirectToAction("AuthorizationError", "Home");
            }
            return View(md);
        }
        public IActionResult ProcessForm()
        {
            ProcessFormModel md = new ProcessFormModel();
            bool allOK = checkAuthorization(25);
            if (!allOK)
            {
                HttpContext.Session.SetString("errorMessage", "You are not authorized to access this page. Please contact Administrator.......");
                return RedirectToAction("AuthorizationError", "Home");
            }
            return View(md); 
        }
        [HttpPost]
        public IActionResult ProcessForm(ProcessFormModel md)
        {
            {//Checking Authorization and Authenticity
                bool allOK = checkAuthorization(25);
                if (!allOK)
                {
                    HttpContext.Session.SetString("errorMessage", "You are not authorized to access this page. Please contact Administrator.......");
                    return RedirectToAction("AuthorizationError", "Home");
                }
                if (HttpContext.Session.GetString("logUserType") == "FVO")
                {

                    //ds = dm.create_dataset(qry);
                   // ViewBag.forms = ds;
                }
                else if (HttpContext.Session.GetString("logUserType") == "AERO")
                {
                }
                else if (HttpContext.Session.GetString("logUserType") == "ERO")
                {

                }
                else
                {
                    HttpContext.Session.SetString("errorMessage", "You are not authorized to access this page. Please contact Administrator.......");
                    return RedirectToAction("AuthorizationError", "Home");
                }
            }//Authorization

            qry = "SELECT P.TCODE,P.PCODE,F.*,ISNULL(CONVERT(VARCHAR,F.DOB,103),CAST(AGE AS VARCHAR)) AGEDOB FROM SE_EROLL.DBO.FORMS AS F JOIN SE_EROLL.DBO.PARTLIST AS P ON F.PART_NO=";
            qry += "P.PART_NO AND F.PAN_MUN=P.PAN_MUN WHERE F.FORMID=" + md.formid;
            ds = dm.create_dataset(qry);
            DataRow formRow = ds.Tables[0].Rows[0];
            md.tehsil = formRow["TCODE"].ToString();
            md.panchayat = formRow["PCODE"].ToString();
            md.ward= formRow["PART_NO"].ToString();
            md.ageProof = (byte[])formRow["AGE_PROOF"];
            md.addressProof = (byte[])formRow["ADDRESS_PROOF"];
            md.photo = (byte[])formRow["PHOTO"];
            md.ename = formRow["ENAME"].ToString();
            if (formRow["RLN_TYPE"].ToString() == "F")
                md.rlnType = "FATHER";
            else if (formRow["RLN_TYPE"].ToString() == "M")
                md.rlnType = "MOTHER";
            else if (formRow["RLN_TYPE"].ToString() == "H")
                md.rlnType = "HUSBAND";
            else
                md.rlnType = "OTHER";
            md.rlnName= formRow["RLN_NAME"].ToString();
            md.dob= formRow["AGEDOB"].ToString();
            if (formRow["GENDER"].ToString() == "M")
                md.gender = "MALE";
            else if (formRow["GENDER"].ToString() == "F")
                md.gender = "FEMALE";
            else
                md.gender = "THIRD GENDER";
          
            if (md.panMun == "P")
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
                    md.panchayat = "0";
                }
                qry = "SELECT CONST_NO,WARD_NAME FROM PAN_WARD WHERE PCODE=" + md.panchayat + " ORDER BY WARD_NAME";
                ds = dm.create_dataset(qry);
                ViewBag.wards = ds;
                qry = "SELECT * FROM SE_EROLL.DBO.RLNTYPE ORDER BY RLNTYPE";
                ds = dm.create_dataset(qry);
                ViewBag.rlnTypes = ds;
                qry = "SELECT VCODE,VNAME FROM VILLAGE /*WHERE TCODE=" + md.addressTehsil + "*/ ORDER BY VNAME";
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
                    if (row[0].ToString() == md.panchayat)
                    {
                        flag = true;
                    }
                }
                if (flag == false)
                {
                    md.panchayat = "0";
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
                return RedirectToAction("Home","Home");
            }
            return View(md);


        }

        #endregion

        #region FINAL ELECTORAL ROLL
        public IActionResult FinalRoll()
        {
            string panMun = HttpContext.Session.GetString("electionType");
            if (panMun == "P")
            {
                qry = "SELECT PCODE,PAN_NAME FROM PANCHAYAT ORDER BY PAN_NAME";
                ds = dm.create_dataset(qry);
                ViewBag.panchayats = ds;
                qry = "SELECT PART_NO AS CONST_NO,PART_NAME AS WARD_NAME FROM SE_EROLL.DBO.PARTLIST WHERE PAN_MUN='" + panMun + "' AND PCODE=" + ds.Tables[0].Rows[0]["PCODE"] + " ORDER BY WARD_NAME";
                ds = dm.create_dataset(qry);
                ViewBag.wards = ds;
                qry = "SELECT DISTINCT REVISIONYEAR FROM SE_EROLL.DBO.REVISIONS ORDER BY REVISIONYEAR DESC";
                ds = dm.create_dataset(qry);
                ViewBag.revisionYears = ds;
            }
            else if (panMun == "M")
            {
                qry = "SELECT WARD_NO AS PCODE,WARD_NAME AS PAN_NAME FROM MUN_WARD ORDER BY PAN_NAME";
                ds = dm.create_dataset(qry);
                ViewBag.panchayats = ds;
                qry = "SELECT PART_NO AS CONST_NO,PART_NAME AS WARD_NAME FROM SE_EROLL.DBO.PARTLIST WHERE PAN_MUN='" + panMun + "' AND PCODE=" + ds.Tables[0].Rows[0]["PCODE"] + " ORDER BY WARD_NAME";
                ds = dm.create_dataset(qry);
                ViewBag.wards = ds;
                qry = "SELECT DISTINCT REVISIONYEAR FROM SE_EROLL.DBO.REVISIONS ORDER BY REVISIONYEAR DESC";
                ds = dm.create_dataset(qry);
                ViewBag.revisionYears = ds;
            }

            return View();
        }
        [HttpPost]
        public IActionResult FinalRoll(RepDraftRollModel md)
        {
            string panMun = HttpContext.Session.GetString("electionType");
            if (panMun == "P")
            {
                qry = "SELECT PCODE,PAN_NAME FROM PANCHAYAT ORDER BY PAN_NAME";
                ds = dm.create_dataset(qry);
                ViewBag.panchayats = ds;
                qry = "SELECT PART_NO AS CONST_NO,PART_NAME AS WARD_NAME FROM SE_EROLL.DBO.PARTLIST WHERE PAN_MUN='" + panMun + "' AND PCODE=" + md.panchayat + " ORDER BY WARD_NAME";
                ds = dm.create_dataset(qry);
                ViewBag.wards = ds;
                qry = "SELECT DISTINCT REVISIONYEAR FROM SE_EROLL.DBO.REVISIONS ORDER BY REVISIONYEAR DESC";
                ds = dm.create_dataset(qry);
                ViewBag.revisionYears = ds;
            }
            else if (panMun == "M")
            {
                qry = "SELECT WARD_NO AS PCODE,WARD_NAME AS PAN_NAME FROM MUN_WARD ORDER BY PAN_NAME";
                ds = dm.create_dataset(qry);
                ViewBag.panchayats = ds;
                qry = "SELECT PART_NO AS CONST_NO,PART_NAME AS WARD_NAME FROM SE_EROLL.DBO.PARTLIST WHERE PAN_MUN='" + panMun + "' AND PCODE=" + md.panchayat + " ORDER BY WARD_NAME";
                ds = dm.create_dataset(qry);
                ViewBag.wards = ds;
                qry = "SELECT DISTINCT REVISIONYEAR FROM SE_EROLL.DBO.REVISIONS ORDER BY REVISIONYEAR DESC";
                ds = dm.create_dataset(qry);
                ViewBag.revisionYears = ds;
            }
            return View(md);
        }
        #endregion

        #region EROLL REPORTS
        public IActionResult ErollReports()
        {
            return View();
        }
        #endregion

        #region PS-WISE ELECTOR DETAILS
        public IActionResult PSWiseElectors()
        {
            PSWiseElectorsModel md = new PSWiseElectorsModel();
            md.reportLevel = "1";
            return View(md);
        }
        [HttpPost]
        public IActionResult PSWiseElectors(PSWiseElectorsModel md)
        {
            if (md.reportLevel!="1")
            {
                qry = "SELECT * FROM DISTRICT ORDER BY DIST_NAME";
                ds = dm.create_dataset(qry);
                ViewBag.districts = ds;
                if (md.reportLevel=="3" || md.reportLevel=="4")
                {
                    qry = "SELECT TCODE,TNAME FROM TEHSIL WHERE DIST_CODE=" + md.district + " ORDER BY TNAME";
                    ds = dm.create_dataset(qry);
                    ViewBag.tehsils = ds;
                    //if (md.reportLevel=="P")
                    //{
                        qry = "SELECT PCODE,PAN_NAME FROM PANCHAYAT WHERE TCODE=" + md.tehsil + " ORDER BY PAN_NAME";
                        ds = dm.create_dataset(qry);
                        ViewBag.panchayats = ds;
                    //}
                }
            }
            qry = "SELECT DISTINCT REVISIONYEAR FROM SE_EROLL.DBO.REVISIONS ORDER BY REVISIONYEAR DESC";
            ds=dm.create_dataset(qry); 
            ViewBag.revisionYear = ds;
            return View(md);
        }

        #endregion

        #region PANCHAYAT-WISE ELECTOR DETAILS
        public IActionResult PanchayatWiseElectors()
        {
            PSWiseElectorsModel md = new PSWiseElectorsModel();
            md.reportLevel = "1";
            return View(md);
        }
        [HttpPost]
        public IActionResult PanchayatWiseElectors(PSWiseElectorsModel md)
        {
            if (md.reportLevel != "1")
            {
                qry = "SELECT * FROM DISTRICT ORDER BY DIST_NAME";
                ds = dm.create_dataset(qry);
                ViewBag.districts = ds;
                if (md.reportLevel == "3" || md.reportLevel == "4")
                {
                    qry = "SELECT TCODE,TNAME FROM TEHSIL WHERE DIST_CODE=" + md.district + " ORDER BY TNAME";
                    ds = dm.create_dataset(qry);
                    ViewBag.tehsils = ds;
                    //if (md.reportLevel=="P")
                    //{
                    qry = "SELECT PCODE,PAN_NAME FROM PANCHAYAT WHERE TCODE=" + md.tehsil + " ORDER BY PAN_NAME";
                    ds = dm.create_dataset(qry);
                    ViewBag.panchayats = ds;
                    //}
                }
            }
            qry = "SELECT DISTINCT REVISIONYEAR FROM SE_EROLL.DBO.REVISIONS ORDER BY REVISIONYEAR DESC";
            ds = dm.create_dataset(qry);
            ViewBag.revisionYear = ds;
            return View(md);
        }

        #endregion

        #region PS-WISE ELECTOR CHANGES (ADDITIONS & DELETIONS)

        public IActionResult PSWiseElectorChanges()
        {
            PSWiseElectorsModel md = new PSWiseElectorsModel();
            md.reportLevel = "1";
            return View(md);
        }
        [HttpPost]
        public IActionResult PSWiseElectorChanges(PSWiseElectorsModel md)
        {
            if (md.reportLevel != "1")
            {
                qry = "SELECT * FROM DISTRICT ORDER BY DIST_NAME";
                ds = dm.create_dataset(qry);
                ViewBag.districts = ds;
                if (md.reportLevel == "3" || md.reportLevel == "4")
                {
                    qry = "SELECT TCODE,TNAME FROM TEHSIL WHERE DIST_CODE=" + md.district + " ORDER BY TNAME";
                    ds = dm.create_dataset(qry);
                    ViewBag.tehsils = ds;
                    //if (md.reportLevel=="P")
                    //{
                    qry = "SELECT PCODE,PAN_NAME FROM PANCHAYAT WHERE TCODE=" + md.tehsil + " ORDER BY PAN_NAME";
                    ds = dm.create_dataset(qry);
                    ViewBag.panchayats = ds;
                    //}
                }
            }
            qry = "SELECT DISTINCT REVISIONYEAR FROM SE_EROLL.DBO.REVISIONS ORDER BY REVISIONYEAR DESC";
            ds = dm.create_dataset(qry);
            ViewBag.revisionYear = ds;
            return View(md);
        }

        #endregion





    }
}
