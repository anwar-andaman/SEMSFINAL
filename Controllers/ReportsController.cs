//using AspNetCore.Reporting;
using Microsoft.Reporting.NETCore;
using Microsoft.AspNetCore.Mvc;
using System.Security.AccessControl;
using Microsoft.Data.SqlClient;
using System.Data;
using AspNetCore.ReportingServices.ReportProcessing.ReportObjectModel;
using System.Security.Cryptography;
using System.Reflection.Metadata;
//using AspNetCore.Reporting.ReportExecutionService;
using System.IO;
using Microsoft.ReportingServices.ReportProcessing.ReportObjectModel;

namespace SEMS.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ILogger<ReportsController> _logger;
        private IWebHostEnvironment _webHostEnv;
        DataManager dm = new DataManager(HomeController.constr);
        public ReportsController(ILogger<ReportsController> logger, IWebHostEnvironment webHostEnv)
        {
            
            this._logger = logger;
            this._webHostEnv = webHostEnv;
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        }
       
        string qry;
        #region DRAFT ELECTORAL ROLL
        public IActionResult DraftRoll(int id)
        {
            if (HttpContext.Session.GetString("logUserID") == null)
            {
                return RedirectToAction("Login", "Home");
            }
            int part = id, revisionYear = 2024;
            string panMun = HttpContext.Session.GetString("electionType");
            string qry;
            System.Data.DataSet ds1, ds2, ds3, ds4;
            DataTable DraftEroll = new DataTable();
            string panchTable, panwardTable;
            if (panMun == "P")
            {
                panchTable = "PANCHAYAT";
                qry = "SELECT * FROM (SELECT ROW_NUMBER() OVER (ORDER BY (SELECT SLNOINPART)) AS SNO,E.EID, E.PART_NO, ";
                qry += "E.SLNOINPART,  E.ENAME, E.RLN_TYPE, E.RLN_NAME, E.AGE, E.EPIC_NO,E.HOUSE_NO,EP.PHOTO,P.PAN_NAME,";
                qry += "W.PART_NAME AS WARD_NAME FROM SE_EROLL.dbo.E_DETAIL AS E JOIN SE_EROLL.DBO.E_PHOTOS AS EP ON EP.EID=E.EID JOIN SE_EROLL.DBO.PARTLIST ";
                qry += "AS W ON E.PART_NO=W.PART_NO AND E.PAN_MUN=W.PAN_MUN JOIN " + panchTable + " AS P ON W.PCODE=P.PCODE WHERE E.PAN_MUN='" + panMun + "' AND E.PART_NO=" + part + ") AS DRAFTROLL ";
                qry += "WHERE (SNO % 3)=1 ORDER BY SNO";
                ds1 = dm.create_dataset(qry);
                qry = "SELECT * FROM (SELECT ROW_NUMBER() OVER (ORDER BY (SELECT SLNOINPART)) AS SNO,E.EID, E.PART_NO, ";
                qry += "E.SLNOINPART,  E.ENAME, E.RLN_TYPE, E.RLN_NAME, E.AGE, E.EPIC_NO,E.HOUSE_NO,EP.PHOTO,P.PAN_NAME,";
                qry += "W.PART_NAME AS WARD_NAME FROM SE_EROLL.dbo.E_DETAIL AS E JOIN SE_EROLL.DBO.E_PHOTOS AS EP ON EP.EID=E.EID JOIN SE_EROLL.DBO.PARTLIST ";
                qry += "AS W ON E.PART_NO=W.PART_NO AND E.PAN_MUN=W.PAN_MUN JOIN " + panchTable + " AS P ON W.PCODE=P.PCODE WHERE E.PAN_MUN='" + panMun + "' AND E.PART_NO=" + part + ") AS DRAFTROLL ";
                qry += "WHERE (SNO % 3)=2 ORDER BY SNO";
                ds2 = dm.create_dataset(qry);
                qry = "SELECT * FROM (SELECT ROW_NUMBER() OVER (ORDER BY (SELECT SLNOINPART)) AS SNO,E.EID, E.PART_NO, ";
                qry += "E.SLNOINPART,  E.ENAME, E.RLN_TYPE, E.RLN_NAME, E.AGE, E.EPIC_NO,E.HOUSE_NO,EP.PHOTO,P.PAN_NAME,";
                qry += "W.PART_NAME AS WARD_NAME FROM SE_EROLL.dbo.E_DETAIL AS E JOIN SE_EROLL.DBO.E_PHOTOS AS EP ON EP.EID=E.EID JOIN SE_EROLL.DBO.PARTLIST ";
                qry += "AS W ON E.PART_NO=W.PART_NO AND E.PAN_MUN=W.PAN_MUN JOIN " + panchTable + " AS P ON W.PCODE=P.PCODE WHERE E.PAN_MUN='" + panMun + "' AND E.PART_NO=" + part + ") AS DRAFTROLL ";
                qry += "WHERE (SNO % 3)=0 ORDER BY SNO";
                ds3 = dm.create_dataset(qry);

            }
            else if (panMun == "M")
            {
                panchTable = "MUN_WARD";
                qry = "SELECT * FROM (SELECT ROW_NUMBER() OVER (ORDER BY (SELECT SLNOINPART)) AS SNO,E.EID, E.PART_NO, ";
                qry += "E.SLNOINPART,  E.ENAME, E.RLN_TYPE, E.RLN_NAME, E.AGE, E.EPIC_NO,E.HOUSE_NO,EP.PHOTO,P.WARD_NAME AS PAN_NAME,";
                qry += "W.PART_NAME AS WARD_NAME FROM SE_EROLL.dbo.E_DETAIL AS E JOIN SE_EROLL.DBO.E_PHOTOS AS EP ON EP.EID=E.EID JOIN SE_EROLL.DBO.PARTLIST ";
                qry += "AS W ON E.PART_NO=W.PART_NO AND E.PAN_MUN=W.PAN_MUN JOIN " + panchTable + " AS P ON W.PCODE=P.WARD_NO WHERE E.PAN_MUN='" + panMun + "' AND E.PART_NO=" + part + ") AS DRAFTROLL ";
                qry += "WHERE (SNO % 3)=1 ORDER BY SNO";
                ds1 = dm.create_dataset(qry);
                qry = "SELECT * FROM (SELECT ROW_NUMBER() OVER (ORDER BY (SELECT SLNOINPART)) AS SNO,E.EID, E.PART_NO, ";
                qry += "E.SLNOINPART,  E.ENAME, E.RLN_TYPE, E.RLN_NAME, E.AGE, E.EPIC_NO,E.HOUSE_NO,EP.PHOTO,P.WARD_NAME AS PAN_NAME,";
                qry += "W.PART_NAME AS WARD_NAME FROM SE_EROLL.dbo.E_DETAIL AS E JOIN SE_EROLL.DBO.E_PHOTOS AS EP ON EP.EID=E.EID JOIN SE_EROLL.DBO.PARTLIST ";
                qry += "AS W ON E.PART_NO=W.PART_NO AND E.PAN_MUN=W.PAN_MUN JOIN " + panchTable + " AS P ON W.PCODE=P.WARD_NO WHERE E.PAN_MUN='" + panMun + "' AND E.PART_NO=" + part + ") AS DRAFTROLL ";
                qry += "WHERE (SNO % 3)=2 ORDER BY SNO";
                ds2 = dm.create_dataset(qry);
                qry = "SELECT * FROM (SELECT ROW_NUMBER() OVER (ORDER BY (SELECT SLNOINPART)) AS SNO,E.EID, E.PART_NO, ";
                qry += "E.SLNOINPART,  E.ENAME, E.RLN_TYPE, E.RLN_NAME, E.AGE, E.EPIC_NO,E.HOUSE_NO,EP.PHOTO,P.WARD_NAME AS PAN_NAME,";
                qry += "W.PART_NAME AS WARD_NAME FROM SE_EROLL.dbo.E_DETAIL AS E JOIN SE_EROLL.DBO.E_PHOTOS AS EP ON EP.EID=E.EID JOIN SE_EROLL.DBO.PARTLIST ";
                qry += "AS W ON E.PART_NO=W.PART_NO AND E.PAN_MUN=W.PAN_MUN JOIN " + panchTable + " AS P ON W.PCODE=P.WARD_NO WHERE E.PAN_MUN='" + panMun + "' AND E.PART_NO=" + part + ") AS DRAFTROLL ";
                qry += "WHERE (SNO % 3)=0 ORDER BY SNO";
                ds3 = dm.create_dataset(qry);
            }
            else
            {
                return RedirectToAction("Login", "Home");
            }
            qry = "SELECT * FROM SE_EROLL.DBO.REVISIONS WHERE REVISIONNO=0 AND REVISIONYEAR=" + revisionYear;
            ds4 = dm.create_dataset(qry);
            string format = "PDF";
            int extension = (int)(DateTime.Now.Ticks >> 10);
            string mimeType = "application/pdf";
            string reportPath = $"{this._webHostEnv.WebRootPath}\\Reports\\RepDraftRoll.rdlc";
            //Dictionary<string, string> parameters = new Dictionary<string, string>();
            //parameters.Add("panchayat", "WIMBERLYGUNJ");
            //parameters.Add("ward", "KANYAPURAM-2");
            //parameters.Add("psno", "459");
            

            //var report = new AspNetCore.Reporting.LocalReport(reportPath);
            //report.AddDataSource("DRAFTROLL", ds1.Tables[0]);
            //report.AddDataSource("DRAFTROLL1", ds2.Tables[0]);
            //report.AddDataSource("DRAFTROLL2", ds3.Tables[0]);
            //report.AddDataSource("REVISIONS", ds4.Tables[0]);
            //var result = report.Execute(RenderType.Pdf, extension, parameters, "application/pdf");
            LocalReport report = new LocalReport();
            report.ReportPath = reportPath;
            report.DataSources.Add(new ReportDataSource("DRAFTROLL", ds1.Tables[0]));
            report.DataSources.Add(new ReportDataSource("DRAFTROLL1", ds2.Tables[0]));
            report.DataSources.Add(new ReportDataSource("DRAFTROLL2", ds3.Tables[0]));
            report.DataSources.Add(new ReportDataSource("REVISIONS", ds4.Tables[0]));
            if (panMun == "P")
            {
                report.SetParameters(new[] { new ReportParameter("header", "PANCHAYAT") });
                report.SetParameters(new[] { new ReportParameter("panch_ward", "Panchayat") });
                //parameters.Add("header", "PANCHAYAT");
                //parameters.Add("panch_ward", "Panchayat:");
            }
            else if (panMun == "M")
            {
                //parameters.Add("header", "MUNICIPAL");
                //parameters.Add("panch_ward", "Ward :");
                report.SetParameters(new[] { new ReportParameter("header", "MUNICIPAL") });
                report.SetParameters(new[] { new ReportParameter("panch_ward", "Ward") });
            }
            else
            {
                return RedirectToAction("Login", "Home");
            }
            // report.SetParameters(new[] { new ReportParameter("Parameter1", "Parameter value") });
            byte[] pdf = report.Render("PDF");
            //var result = report.Execute(RenderType.Pdf, extension, null, "application/pdf");
            // return File(result.MainStream, mimeType);
            Stream strm = new MemoryStream(pdf);
            var result = new FileStreamResult(strm, "application/pdf");
            // return File(pdf, mimeType, "myreport.pdf", false);
            return File(strm, mimeType);

            //return File(result.MainStream, mimeType);
        }
        #endregion

        #region FINAL ELECTORAL ROLL
        public IActionResult FinalRoll(int id)
        {
            if (HttpContext.Session.GetString("logUserID") == null)
            {
                return RedirectToAction("Login", "Home");
            }
            int part = id, revisionYear = 2024;
            string panMun = HttpContext.Session.GetString("electionType");
            
            System.Data.DataSet  ds1,ds2,ds3,ds4,ds5,ds6,ds7,ds8;
           
            DataTable DraftEroll = new DataTable();
            string panchTable, panwardTable;
            if (panMun == "P")
            {
                panchTable = "PANCHAYAT";
                qry = "SELECT * FROM (SELECT ROW_NUMBER() OVER (ORDER BY (SELECT REVISIONNO),(SELECT SLNOINPART)) AS SNO,E.EID, E.PART_NO, ";
                qry += "E.SLNOINPART,  E.ENAME, E.RLN_TYPE, E.RLN_NAME, E.AGE, E.EPIC_NO,E.HOUSE_NO,EP.PHOTO,P.PAN_NAME,";
                qry += "W.PART_NAME AS WARD_NAME,STATUSTYPE,ROLL_STATUSTYPE,REVISIONNO,GENDER FROM SE_EROLL.dbo.E_DETAIL AS E JOIN SE_EROLL.DBO.E_PHOTOS AS EP ON EP.EID=E.EID JOIN SE_EROLL.DBO.PARTLIST ";
                qry += "AS W ON E.PART_NO=W.PART_NO AND E.PAN_MUN=W.PAN_MUN JOIN " + panchTable + " AS P ON W.PCODE=P.PCODE WHERE E.PAN_MUN='" + panMun + "' AND E.PART_NO=" + part + " AND E.REVISIONNO=0) AS DRAFTROLL ";
                qry += " ORDER BY SNO";
                ds1 = dm.create_dataset(qry);
                
                qry = "SELECT * FROM (SELECT ROW_NUMBER() OVER (ORDER BY (SELECT REVISIONNO),(SELECT SLNOINPART)) AS SNO,E.EID, E.PART_NO, ";
                qry += "E.SLNOINPART,  E.ENAME, E.RLN_TYPE, E.RLN_NAME, E.AGE, E.EPIC_NO,E.HOUSE_NO,EP.PHOTO,P.PAN_NAME,";
                qry += "W.PART_NAME AS WARD_NAME,STATUSTYPE,ROLL_STATUSTYPE,REVISIONNO,GENDER FROM SE_EROLL.dbo.E_DETAIL AS E JOIN SE_EROLL.DBO.E_PHOTOS AS EP ON EP.EID=E.EID JOIN SE_EROLL.DBO.PARTLIST ";
                qry += "AS W ON E.PART_NO=W.PART_NO AND E.PAN_MUN=W.PAN_MUN JOIN " + panchTable + " AS P ON W.PCODE=P.PCODE WHERE E.PAN_MUN='" + panMun + "' AND E.PART_NO=" + part + " AND E.REVISIONNO=1) AS DRAFTROLL ";
                qry += " ORDER BY SNO";
                ds2 = dm.create_dataset(qry);
               
                qry = "SELECT * FROM (SELECT ROW_NUMBER() OVER (ORDER BY (SELECT REVISIONNO),(SELECT SLNOINPART)) AS SNO,E.EID, E.PART_NO, ";
                qry += "E.SLNOINPART,  E.ENAME, E.RLN_TYPE, E.RLN_NAME, E.AGE, E.EPIC_NO,E.HOUSE_NO,EP.PHOTO,P.PAN_NAME,";
                qry += "W.PART_NAME AS WARD_NAME,STATUSTYPE,ROLL_STATUSTYPE,REVISIONNO,GENDER FROM SE_EROLL.dbo.E_DETAIL AS E JOIN SE_EROLL.DBO.E_PHOTOS AS EP ON EP.EID=E.EID JOIN SE_EROLL.DBO.PARTLIST ";
                qry += "AS W ON E.PART_NO=W.PART_NO AND E.PAN_MUN=W.PAN_MUN JOIN " + panchTable + " AS P ON W.PCODE=P.PCODE WHERE E.PAN_MUN='" + panMun + "' AND E.PART_NO=" + part + " AND E.REVISIONNO=0 AND STATUSTYPE='D') AS DRAFTROLL ";
                qry += " ORDER BY SNO";
                ds3 = dm.create_dataset(qry);

            }
            else if (panMun == "M")
            {
                panchTable = "MUN_WARD";
                qry = "SELECT * FROM (SELECT ROW_NUMBER() OVER (ORDER BY (SELECT REVISIONNO),(SELECT SLNOINPART)) AS SNO,E.EID, E.PART_NO, ";
                qry += "E.SLNOINPART,  E.ENAME, E.RLN_TYPE, E.RLN_NAME, E.AGE, E.EPIC_NO,E.HOUSE_NO,EP.PHOTO,P.WARD_NAME AS PAN_NAME,";
                qry += "W.PART_NAME AS WARD_NAME,STATUSTYPE,ROLL_STATUSTYPE,REVISIONNO,GENDER FROM SE_EROLL.dbo.E_DETAIL AS E JOIN SE_EROLL.DBO.E_PHOTOS AS EP ON EP.EID=E.EID JOIN SE_EROLL.DBO.PARTLIST ";
                qry += "AS W ON E.PART_NO=W.PART_NO AND E.PAN_MUN=W.PAN_MUN JOIN " + panchTable + " AS P ON W.PCODE=P.WARD_NO WHERE E.PAN_MUN='" + panMun + "' AND E.PART_NO=" + part + " AND E.REVISIONNO=0) AS DRAFTROLL ";
                qry += " ORDER BY SNO";
                ds1 = dm.create_dataset(qry);
             
                qry = "SELECT * FROM (SELECT ROW_NUMBER() OVER (ORDER BY (SELECT REVISIONNO),(SELECT SLNOINPART)) AS SNO,E.EID, E.PART_NO, ";
                qry += "E.SLNOINPART,  E.ENAME, E.RLN_TYPE, E.RLN_NAME, E.AGE, E.EPIC_NO,E.HOUSE_NO,EP.PHOTO,P.WARD_NAME AS PAN_NAME,";
                qry += "W.PART_NAME AS WARD_NAME,STATUSTYPE,ROLL_STATUSTYPE,REVISIONNO,GENDER FROM SE_EROLL.dbo.E_DETAIL AS E JOIN SE_EROLL.DBO.E_PHOTOS AS EP ON EP.EID=E.EID JOIN SE_EROLL.DBO.PARTLIST ";
                qry += "AS W ON E.PART_NO=W.PART_NO AND E.PAN_MUN=W.PAN_MUN JOIN " + panchTable + " AS P ON W.PCODE=P.WARD_NO WHERE E.PAN_MUN='" + panMun + "' AND E.PART_NO=" + part + " AND E.REVISIONNO=1) AS DRAFTROLL ";
                qry += " ORDER BY SNO";
                ds2 = dm.create_dataset(qry);
                
                qry = "SELECT * FROM (SELECT ROW_NUMBER() OVER (ORDER BY (SELECT REVISIONNO),(SELECT SLNOINPART)) AS SNO,E.EID, E.PART_NO, ";
                qry += "E.SLNOINPART,  E.ENAME, E.RLN_TYPE, E.RLN_NAME, E.AGE, E.EPIC_NO,E.HOUSE_NO,EP.PHOTO,P.WARD_NAME AS PAN_NAME,";
                qry += "W.PART_NAME AS WARD_NAME,STATUSTYPE,ROLL_STATUSTYPE,REVISIONNO,GENDER FROM SE_EROLL.dbo.E_DETAIL AS E JOIN SE_EROLL.DBO.E_PHOTOS AS EP ON EP.EID=E.EID JOIN SE_EROLL.DBO.PARTLIST ";
                qry += "AS W ON E.PART_NO=W.PART_NO AND E.PAN_MUN=W.PAN_MUN JOIN " + panchTable + " AS P ON W.PCODE=P.WARD_NO WHERE E.PAN_MUN='" + panMun + "' AND E.PART_NO=" + part + " AND E.REVISIONNO=0 AND STATUSTYPE='D') AS DRAFTROLL ";
                qry += " ORDER BY SNO";
                ds3 = dm.create_dataset(qry);
            }
            else
            {
                return RedirectToAction("Login", "Home");
            }
            qry = "SELECT * FROM SE_EROLL.DBO.REVISIONS WHERE REVISIONNO=0 AND REVISIONYEAR=" + revisionYear;
            ds4 = dm.create_dataset(qry);
           
            string format = "PDF";
            int extension = (int)(DateTime.Now.Ticks >> 10);
            string mimeType = "application/pdf";
            string reportPath = $"{this._webHostEnv.WebRootPath}\\Reports\\RepFinalRoll.rdlc";
            // var report = LocalReport(reportPath, format, extension);
            //Dictionary<string, string> parameters = new Dictionary<string, string>();
            //parameters.Add("panchayat", "WIMBERLYGUNJ");
            //parameters.Add("ward", "KANYAPURAM-2");
            //parameters.Add("psno", "459");


            //var report = new AspNetCore.Reporting.LocalReport(reportPath);
            //report.AddDataSource("DRAFTROLL", ds1.Tables[0]);
            //report.AddDataSource("SUPROLL", ds2.Tables[0]);
            //report.AddDataSource("DELETIONLIST", ds3.Tables[0]);
            //report.AddDataSource("REVISIONS", ds4.Tables[0]);
            //var result = report.Execute(RenderType.Pdf, extension, parameters, "application/pdf");
            //return File(result.MainStream, mimeType);
            LocalReport report = new LocalReport();
            report.ReportPath = reportPath;
            report.DataSources.Add(new ReportDataSource("DRAFTROLL", ds1.Tables[0]));
            report.DataSources.Add(new ReportDataSource("SUPROLL", ds2.Tables[0]));
            report.DataSources.Add(new ReportDataSource("DELETIONLIST", ds3.Tables[0]));
            report.DataSources.Add(new ReportDataSource("REVISIONS", ds4.Tables[0]));
            if (panMun == "P")
            {
                report.SetParameters(new[] { new ReportParameter("header", "PANCHAYAT") });
                report.SetParameters(new[] { new ReportParameter("panch_ward", "Panchayat") });
                //parameters.Add("header", "PANCHAYAT");
                //parameters.Add("panch_ward", "Panchayat:");
            }
            else if (panMun == "M")
            {
                //parameters.Add("header", "MUNICIPAL");
                //parameters.Add("panch_ward", "Ward :");
                report.SetParameters(new[] { new ReportParameter("header", "MUNICIPAL") });
                report.SetParameters(new[] { new ReportParameter("panch_ward", "Ward") });
            }
            else
            {
                return RedirectToAction("Login", "Home");
            }
            // report.SetParameters(new[] { new ReportParameter("Parameter1", "Parameter value") });
            byte[] pdf = report.Render("PDF");
            //var result = report.Execute(RenderType.Pdf, extension, null, "application/pdf");
            // return File(result.MainStream, mimeType);
            Stream strm = new MemoryStream(pdf);
            var result = new FileStreamResult(strm, "application/pdf");
            // return File(pdf, mimeType, "myreport.pdf", false);
            return File(strm, mimeType);
        }
        #endregion

        #region PS-WISE ELECTOR DETAILS
        public IActionResult PSWiseElectors(int id,string panmun,int revYear,int dataUpto,int keyValue)
        {
            System.Data.DataSet ds = new System.Data.DataSet();
            System.Data.DataTable dt = new System.Data.DataTable();
            System.Data.DataColumn col1 = new System.Data.DataColumn("REVYEAR");
            System.Data.DataColumn col2 = new System.Data.DataColumn("HEADER");
            dt.Columns.Add(col1);
            dt.Columns.Add(col2);
            System.Data.DataRow dr = dt.NewRow();
            string format = "PDF";
            int extension = (int)(DateTime.Now.Ticks >> 10);
            string mimeType = "application/pdf";
            
           // Dictionary<string, string> parameters = new Dictionary<string, string>();
            string reportPath = "";
            if (panmun=="P")
            {
                if (dataUpto == 0)
                {
                    dr["HEADER"] = "AS PER DRAFT PANCHAYAT ROLL";
                }
                else if (dataUpto == 1)
                {
                    dr["HEADER"] = "AS PER FINAL PANCHAYAT ROLL";
                }
            }
            else if (panmun=="M") 
            {
                if (dataUpto == 0)
                {
                    dr["HEADER"] = "AS PER DRAFT MUNICIPAL ROLL";
                }
                else if (dataUpto == 1)
                {
                    dr["HEADER"] = "AS PER FINAL MUNICIPAL ROLL";
                }
            }
            dr["REVYEAR"] = revYear.ToString();
           dt.Rows.Add(dr);
            if (id == 1)
            {
                if (panmun=="P")
                {
                    qry = "SELECT D.DIST_CODE,D.DIST_NAME,T.TNAME,T.TNO,E.PART_NO,PL.PART_NAME,SUM(CASE GENDER WHEN 'M' THEN 1 ";
                    qry += "ELSE 0 END) AS MALE,SUM(CASE GENDER WHEN 'F' THEN 1 ELSE 0 END) AS FEMALE,SUM(CASE GENDER WHEN 'T' ";
                    qry += "THEN 1 ELSE 0 END) AS TG FROM SE_EROLL.DBO.E_DETAIL AS E JOIN SE_EROLL.DBO.PARTLIST AS PL ON ";
                    qry += "E.PART_NO=PL.PART_NO AND E.PAN_MUN=PL.PAN_MUN JOIN PANCHAYAT AS P ON PL.PCODE=P.PCODE JOIN ";
                    qry += "TEHSIL AS T ON P.TCODE=T.TCODE JOIN DISTRICT AS D ON T.DIST_CODE=D.DIST_CODE WHERE E.PAN_MUN='P' ";
                    qry += "AND PL.AREA_TYPE='P' AND E.REVISIONNO<=" + dataUpto + " GROUP BY D.DIST_CODE,D.DIST_NAME,T.TNO,T.TNAME,E.PART_NO,PL.PART_NAME ";
                    qry += "ORDER BY E.PART_NO";
                   
                    reportPath = $"{this._webHostEnv.WebRootPath}\\Reports\\RepPSWiseElectors.rdlc";
                }
                else if (panmun=="M")
                {
                    qry = "SELECT P.WARD_NAME AS TNAME,E.PART_NO,PL.PART_NAME,SUM(CASE GENDER WHEN 'M' THEN 1 ";
                    qry += "ELSE 0 END) AS MALE,SUM(CASE GENDER WHEN 'F' THEN 1 ELSE 0 END) AS FEMALE,SUM(CASE GENDER WHEN 'T' ";
                    qry += "THEN 1 ELSE 0 END) AS TG FROM SE_EROLL.DBO.E_DETAIL AS E JOIN SE_EROLL.DBO.PARTLIST AS PL ON ";
                    qry += "E.PART_NO=PL.PART_NO AND E.PAN_MUN=PL.PAN_MUN JOIN MUN_WARD AS P ON PL.PCODE=P.WARD_NO JOIN ";
                    qry += "TEHSIL AS T ON P.TCODE=T.TCODE  WHERE E.PAN_MUN='M' ";
                    qry += "AND PL.AREA_TYPE='M' AND E.REVISIONNO<=" + dataUpto + " GROUP BY P.WARD_NAME,E.PART_NO,PL.PART_NAME ";
                    qry += "ORDER BY E.PART_NO";
                    reportPath = $"{this._webHostEnv.WebRootPath}\\Reports\\RepPSWiseElectorsMunicipal.rdlc";
                }
            }
            else if (id==2)
            {
                if (panmun=="P")
                {
                    qry = "SELECT D.DIST_CODE,D.DIST_NAME,T.TNAME,T.TNO,E.PART_NO,PL.PART_NAME,SUM(CASE GENDER WHEN 'M' THEN 1 ";
                    qry += "ELSE 0 END) AS MALE,SUM(CASE GENDER WHEN 'F' THEN 1 ELSE 0 END) AS FEMALE,SUM(CASE GENDER WHEN 'T' ";
                    qry += "THEN 1 ELSE 0 END) AS TG FROM SE_EROLL.DBO.E_DETAIL AS E JOIN SE_EROLL.DBO.PARTLIST AS PL ON ";
                    qry += "E.PART_NO=PL.PART_NO AND E.PAN_MUN=PL.PAN_MUN JOIN PANCHAYAT AS P ON PL.PCODE=P.PCODE JOIN ";
                    qry += "TEHSIL AS T ON P.TCODE=T.TCODE JOIN DISTRICT AS D ON T.DIST_CODE=D.DIST_CODE WHERE E.PAN_MUN='P' ";
                    qry += "AND PL.AREA_TYPE='P' AND D.DIST_CODE=" + keyValue + " AND E.REVISIONNO<=" + dataUpto + " GROUP BY D.DIST_CODE,D.DIST_NAME,T.TNO,T.TNAME,E.PART_NO,PL.PART_NAME ";
                    qry += "ORDER BY E.PART_NO";
                    reportPath = $"{this._webHostEnv.WebRootPath}\\Reports\\RepDistrictPSWiseElectors.rdlc";
                }
                else if (panmun=="M")
                {
                    qry = "SELECT P.WARD_NAME AS TNAME,E.PART_NO,PL.PART_NAME,SUM(CASE GENDER WHEN 'M' THEN 1 ";
                    qry += "ELSE 0 END) AS MALE,SUM(CASE GENDER WHEN 'F' THEN 1 ELSE 0 END) AS FEMALE,SUM(CASE GENDER WHEN 'T' ";
                    qry += "THEN 1 ELSE 0 END) AS TG FROM SE_EROLL.DBO.E_DETAIL AS E JOIN SE_EROLL.DBO.PARTLIST AS PL ON ";
                    qry += "E.PART_NO=PL.PART_NO AND E.PAN_MUN=PL.PAN_MUN JOIN MUN_WARD AS P ON PL.PCODE=P.WARD_NO JOIN ";
                    qry += "TEHSIL AS T ON P.TCODE=T.TCODE  WHERE E.PAN_MUN='M' ";
                    qry += "AND PL.AREA_TYPE='M' AND E.REVISIONNO<=" + dataUpto + " GROUP BY P.WARD_NAME,E.PART_NO,PL.PART_NAME ";
                    qry += "ORDER BY E.PART_NO";
                    reportPath = $"{this._webHostEnv.WebRootPath}\\Reports\\RepPSWiseElectorsMunicipal.rdlc";
                }
            }
            else if (id == 3)
            {
                if (panmun == "P")
                {
                    qry = "SELECT D.DIST_CODE,T.TNAME AS DIST_NAME,P.PAN_NAME AS TNAME,T.TNO,E.PART_NO,PL.PART_NAME,SUM(CASE GENDER WHEN 'M' THEN 1 ";
                    qry += "ELSE 0 END) AS MALE,SUM(CASE GENDER WHEN 'F' THEN 1 ELSE 0 END) AS FEMALE,SUM(CASE GENDER WHEN 'T' ";
                    qry += "THEN 1 ELSE 0 END) AS TG FROM SE_EROLL.DBO.E_DETAIL AS E JOIN SE_EROLL.DBO.PARTLIST AS PL ON ";
                    qry += "E.PART_NO=PL.PART_NO AND E.PAN_MUN=PL.PAN_MUN JOIN PANCHAYAT AS P ON PL.PCODE=P.PCODE JOIN ";
                    qry += "TEHSIL AS T ON P.TCODE=T.TCODE JOIN DISTRICT AS D ON T.DIST_CODE=D.DIST_CODE WHERE E.PAN_MUN='P' ";
                    qry += "AND PL.AREA_TYPE='P' AND T.TCODE=" + keyValue + " AND E.REVISIONNO<=" + dataUpto + " GROUP BY D.DIST_CODE,D.DIST_NAME,T.TNO,T.TNAME,P.PAN_NAME,E.PART_NO,PL.PART_NAME ";
                    qry += "ORDER BY E.PART_NO";
                    reportPath = $"{this._webHostEnv.WebRootPath}\\Reports\\RepTehsilPSWiseElectors.rdlc";
                }
                else if (panmun == "M")
                {
                    qry = "SELECT P.WARD_NAME AS TNAME,E.PART_NO,PL.PART_NAME,SUM(CASE GENDER WHEN 'M' THEN 1 ";
                    qry += "ELSE 0 END) AS MALE,SUM(CASE GENDER WHEN 'F' THEN 1 ELSE 0 END) AS FEMALE,SUM(CASE GENDER WHEN 'T' ";
                    qry += "THEN 1 ELSE 0 END) AS TG FROM SE_EROLL.DBO.E_DETAIL AS E JOIN SE_EROLL.DBO.PARTLIST AS PL ON ";
                    qry += "E.PART_NO=PL.PART_NO AND E.PAN_MUN=PL.PAN_MUN JOIN MUN_WARD AS P ON PL.PCODE=P.WARD_NO JOIN ";
                    qry += "TEHSIL AS T ON P.TCODE=T.TCODE  WHERE E.PAN_MUN='M' ";
                    qry += "AND PL.AREA_TYPE='M' AND E.REVISIONNO<=" + dataUpto + " GROUP BY P.WARD_NAME,E.PART_NO,PL.PART_NAME ";
                    qry += "ORDER BY E.PART_NO";
                    reportPath = $"{this._webHostEnv.WebRootPath}\\Reports\\RepPSWiseElectorsMunicipal.rdlc";
                }
            }
            else if (id == 4)
            {
                if (panmun == "P")
                {
                    qry = "SELECT D.DIST_CODE,T.TNAME AS DIST_NAME,P.PAN_NAME AS TNAME,T.TNO,E.PART_NO,PL.PART_NAME,SUM(CASE GENDER WHEN 'M' THEN 1 ";
                    qry += "ELSE 0 END) AS MALE,SUM(CASE GENDER WHEN 'F' THEN 1 ELSE 0 END) AS FEMALE,SUM(CASE GENDER WHEN 'T' ";
                    qry += "THEN 1 ELSE 0 END) AS TG FROM SE_EROLL.DBO.E_DETAIL AS E JOIN SE_EROLL.DBO.PARTLIST AS PL ON ";
                    qry += "E.PART_NO=PL.PART_NO AND E.PAN_MUN=PL.PAN_MUN JOIN PANCHAYAT AS P ON PL.PCODE=P.PCODE JOIN ";
                    qry += "TEHSIL AS T ON P.TCODE=T.TCODE JOIN DISTRICT AS D ON T.DIST_CODE=D.DIST_CODE WHERE E.PAN_MUN='P' ";
                    qry += "AND PL.AREA_TYPE='P' AND P.PCODE=" + keyValue + " AND E.REVISIONNO<=" + dataUpto + " GROUP BY D.DIST_CODE,D.DIST_NAME,T.TNO,T.TNAME,P.PAN_NAME,E.PART_NO,PL.PART_NAME ";
                    qry += "ORDER BY E.PART_NO";
                    reportPath = $"{this._webHostEnv.WebRootPath}\\Reports\\RepPanchayatPSWiseElectors.rdlc";
                }
                else if (panmun == "M")
                {
                    qry = "SELECT P.WARD_NAME AS TNAME,E.PART_NO,PL.PART_NAME,SUM(CASE GENDER WHEN 'M' THEN 1 ";
                    qry += "ELSE 0 END) AS MALE,SUM(CASE GENDER WHEN 'F' THEN 1 ELSE 0 END) AS FEMALE,SUM(CASE GENDER WHEN 'T' ";
                    qry += "THEN 1 ELSE 0 END) AS TG FROM SE_EROLL.DBO.E_DETAIL AS E JOIN SE_EROLL.DBO.PARTLIST AS PL ON ";
                    qry += "E.PART_NO=PL.PART_NO AND E.PAN_MUN=PL.PAN_MUN JOIN MUN_WARD AS P ON PL.PCODE=P.WARD_NO JOIN ";
                    qry += "TEHSIL AS T ON P.TCODE=T.TCODE  WHERE E.PAN_MUN='M' ";
                    qry += "AND PL.AREA_TYPE='M' AND E.REVISIONNO<=" + dataUpto + " GROUP BY P.WARD_NAME,E.PART_NO,PL.PART_NAME ";
                    qry += "ORDER BY E.PART_NO";
                    reportPath = $"{this._webHostEnv.WebRootPath}\\Reports\\RepPSWiseElectorsMunicipal.rdlc";
                }
            }
            else
            {
                return RedirectToAction("Login","Home");
            }
            ds = dm.create_dataset(qry);
            // var report = new LocalReport(reportPath);
            //report.AddDataSource("PSLIST", ds.Tables[0]);
            // report.AddDataSource("PARAMETER", dt);
            //Stream reportDefinition; // your RDLC from file or resource
            //IEnumerable dataSource(); // your datasource for the report
           
            Microsoft.Reporting.NETCore.LocalReport report = new Microsoft.Reporting.NETCore.LocalReport();
            report.ReportPath= reportPath;
            report.DataSources.Add(new ReportDataSource("PSLIST", ds.Tables[0]));
            report.DataSources.Add(new ReportDataSource("PARAMETER", dt));
           // report.SetParameters(new[] { new ReportParameter("Parameter1", "Parameter value") });
            byte[] pdf = report.Render("PDF");
            //var result = report.Execute(RenderType.Pdf, extension, null, "application/pdf");
            // return File(result.MainStream, mimeType);
            Stream strm = new MemoryStream(pdf);
            var result = new FileStreamResult(strm, "application/pdf");
           // return File(pdf, mimeType, "myreport.pdf", false);
            return File(strm, mimeType);
        }
        #endregion

        #region PANCHAYAT-WISE ELECTOR DETAILS
        public IActionResult PanchayatWiseElectors(int id, string panmun, int revYear, int dataUpto, int keyValue)
        {
            System.Data.DataSet ds = new System.Data.DataSet();
            System.Data.DataTable dt = new System.Data.DataTable();
            System.Data.DataColumn col1 = new System.Data.DataColumn("REVYEAR");
            System.Data.DataColumn col2 = new System.Data.DataColumn("HEADER");
            dt.Columns.Add(col1);
            dt.Columns.Add(col2);
            System.Data.DataRow dr = dt.NewRow();
            string format = "PDF";
            int extension = (int)(DateTime.Now.Ticks >> 10);
            string mimeType = "application/pdf";

            // Dictionary<string, string> parameters = new Dictionary<string, string>();
            string reportPath = "";
            if (panmun == "P")
            {
                if (dataUpto == 0)
                {
                    dr["HEADER"] = "AS PER DRAFT PANCHAYAT ROLL";
                }
                else if (dataUpto == 1)
                {
                    dr["HEADER"] = "AS PER FINAL PANCHAYAT ROLL";
                }
            }
            else if (panmun == "M")
            {
                if (dataUpto == 0)
                {
                    dr["HEADER"] = "AS PER DRAFT MUNICIPAL ROLL";
                }
                else if (dataUpto == 1)
                {
                    dr["HEADER"] = "AS PER FINAL MUNICIPAL ROLL";
                }
            }
            dr["REVYEAR"] = revYear.ToString();
            dt.Rows.Add(dr);
            if (id == 1)
            {
                if (panmun == "P")
                {
                    qry = "SELECT D.DIST_CODE,D.DIST_NAME,T.TNAME,T.TNO,P.PAN_NAME AS PART_NAME,P.PCODE as PART_NO,SUM(CASE GENDER WHEN 'M' THEN 1 ";
                    qry += "ELSE 0 END) AS MALE,SUM(CASE GENDER WHEN 'F' THEN 1 ELSE 0 END) AS FEMALE,SUM(CASE GENDER WHEN 'T' ";
                    qry += "THEN 1 ELSE 0 END) AS TG FROM SE_EROLL.DBO.E_DETAIL AS E JOIN SE_EROLL.DBO.PARTLIST AS PL ON ";
                    qry += "E.PART_NO=PL.PART_NO AND E.PAN_MUN=PL.PAN_MUN JOIN PANCHAYAT AS P ON PL.PCODE=P.PCODE JOIN ";
                    qry += "TEHSIL AS T ON P.TCODE=T.TCODE JOIN DISTRICT AS D ON T.DIST_CODE=D.DIST_CODE WHERE E.PAN_MUN='P' ";
                    qry += "AND PL.AREA_TYPE='P' AND E.REVISIONNO<=" + dataUpto + " GROUP BY D.DIST_CODE,D.DIST_NAME,T.TNO,T.TNAME,P.PCODE,P.PAN_NAME ";
                    qry += "ORDER BY P.PCODE";

                    reportPath = $"{this._webHostEnv.WebRootPath}\\Reports\\RepPanchayatWiseElectorsSummary.rdlc";
                }
                else if (panmun == "M")
                {
                    qry = "SELECT P.WARD_NAME AS TNAME,P.WARD_NO AS PART_NO,P.WARD_NAME AS PART_NAME,SUM(CASE GENDER WHEN 'M' THEN 1 ";
                    qry += "ELSE 0 END) AS MALE,SUM(CASE GENDER WHEN 'F' THEN 1 ELSE 0 END) AS FEMALE,SUM(CASE GENDER WHEN 'T' ";
                    qry += "THEN 1 ELSE 0 END) AS TG FROM SE_EROLL.DBO.E_DETAIL AS E JOIN SE_EROLL.DBO.PARTLIST AS PL ON ";
                    qry += "E.PART_NO=PL.PART_NO AND E.PAN_MUN=PL.PAN_MUN JOIN MUN_WARD AS P ON PL.PCODE=P.WARD_NO JOIN ";
                    qry += "TEHSIL AS T ON P.TCODE=T.TCODE  WHERE E.PAN_MUN='M' ";
                    qry += "AND PL.AREA_TYPE='M' AND E.REVISIONNO<=" + dataUpto + " GROUP BY P.WARD_NO,P.WARD_NAME ";
                    qry += "ORDER BY P.WARD_NO";
                    reportPath = $"{this._webHostEnv.WebRootPath}\\Reports\\RepWardWiseElectorsMunicipal.rdlc";
                }
            }
            else if (id == 2)
            {
                if (panmun == "P")
                {
                    qry = "SELECT D.DIST_CODE,D.DIST_NAME,T.TNAME,T.TNO,P.PCODE AS PART_NO,P.PAN_NAME AS PART_NAME,SUM(CASE GENDER WHEN 'M' THEN 1 ";
                    qry += "ELSE 0 END) AS MALE,SUM(CASE GENDER WHEN 'F' THEN 1 ELSE 0 END) AS FEMALE,SUM(CASE GENDER WHEN 'T' ";
                    qry += "THEN 1 ELSE 0 END) AS TG FROM SE_EROLL.DBO.E_DETAIL AS E JOIN SE_EROLL.DBO.PARTLIST AS PL ON ";
                    qry += "E.PART_NO=PL.PART_NO AND E.PAN_MUN=PL.PAN_MUN JOIN PANCHAYAT AS P ON PL.PCODE=P.PCODE JOIN ";
                    qry += "TEHSIL AS T ON P.TCODE=T.TCODE JOIN DISTRICT AS D ON T.DIST_CODE=D.DIST_CODE WHERE E.PAN_MUN='P' ";
                    qry += "AND PL.AREA_TYPE='P' AND D.DIST_CODE=" + keyValue + " AND E.REVISIONNO<=" + dataUpto + " GROUP BY D.DIST_CODE,D.DIST_NAME,T.TNO,T.TNAME,P.PCODE,P.PAN_NAME ";
                    qry += "ORDER BY P.PCODE";
                    reportPath = $"{this._webHostEnv.WebRootPath}\\Reports\\RepDistrictPanchayatWiseElectors.rdlc";
                }
                else if (panmun == "M")
                {
                    qry = "SELECT P.WARD_NAME AS TNAME,P.WARD_NO AS PART_NO,P.WARD_NAME AS PART_NAME,SUM(CASE GENDER WHEN 'M' THEN 1 ";
                    qry += "ELSE 0 END) AS MALE,SUM(CASE GENDER WHEN 'F' THEN 1 ELSE 0 END) AS FEMALE,SUM(CASE GENDER WHEN 'T' ";
                    qry += "THEN 1 ELSE 0 END) AS TG FROM SE_EROLL.DBO.E_DETAIL AS E JOIN SE_EROLL.DBO.PARTLIST AS PL ON ";
                    qry += "E.PART_NO=PL.PART_NO AND E.PAN_MUN=PL.PAN_MUN JOIN MUN_WARD AS P ON PL.PCODE=P.WARD_NO JOIN ";
                    qry += "TEHSIL AS T ON P.TCODE=T.TCODE  WHERE E.PAN_MUN='M' ";
                    qry += "AND PL.AREA_TYPE='M' AND E.REVISIONNO<=" + dataUpto + " GROUP BY P.WARD_NO,P.WARD_NAME ";
                    qry += "ORDER BY P.WARD_NO";
                    reportPath = $"{this._webHostEnv.WebRootPath}\\Reports\\RepWardWiseElectorsMunicipal.rdlc";
                }
            }
            else if (id == 3)
            {
                if (panmun == "P")
                {
                    qry = "SELECT D.DIST_CODE,T.TNAME AS DIST_NAME,P.PAN_NAME PART_NAME,P.PCODE AS PART_NO,SUM(CASE GENDER WHEN 'M' THEN 1 ";
                    qry += "ELSE 0 END) AS MALE,SUM(CASE GENDER WHEN 'F' THEN 1 ELSE 0 END) AS FEMALE,SUM(CASE GENDER WHEN 'T' ";
                    qry += "THEN 1 ELSE 0 END) AS TG FROM SE_EROLL.DBO.E_DETAIL AS E JOIN SE_EROLL.DBO.PARTLIST AS PL ON ";
                    qry += "E.PART_NO=PL.PART_NO AND E.PAN_MUN=PL.PAN_MUN JOIN PANCHAYAT AS P ON PL.PCODE=P.PCODE JOIN ";
                    qry += "TEHSIL AS T ON P.TCODE=T.TCODE JOIN DISTRICT AS D ON T.DIST_CODE=D.DIST_CODE WHERE E.PAN_MUN='P' ";
                    qry += "AND PL.AREA_TYPE='P' AND T.TCODE=" + keyValue + " AND E.REVISIONNO<=" + dataUpto + " GROUP BY D.DIST_CODE,D.DIST_NAME,T.TNO,T.TNAME,P.PAN_NAME,P.PCODE ";
                    qry += "ORDER BY P.PCODE";
                    reportPath = $"{this._webHostEnv.WebRootPath}\\Reports\\RepTehsilPanchayatWiseElectors.rdlc";
                }
                else if (panmun == "M")
                {
                    qry = "SELECT P.WARD_NAME AS TNAME,P.WARD_NO AS PART_NO,P.WARD_NAME AS PART_NAME,SUM(CASE GENDER WHEN 'M' THEN 1 ";
                    qry += "ELSE 0 END) AS MALE,SUM(CASE GENDER WHEN 'F' THEN 1 ELSE 0 END) AS FEMALE,SUM(CASE GENDER WHEN 'T' ";
                    qry += "THEN 1 ELSE 0 END) AS TG FROM SE_EROLL.DBO.E_DETAIL AS E JOIN SE_EROLL.DBO.PARTLIST AS PL ON ";
                    qry += "E.PART_NO=PL.PART_NO AND E.PAN_MUN=PL.PAN_MUN JOIN MUN_WARD AS P ON PL.PCODE=P.WARD_NO JOIN ";
                    qry += "TEHSIL AS T ON P.TCODE=T.TCODE  WHERE E.PAN_MUN='M' ";
                    qry += "AND PL.AREA_TYPE='M' AND E.REVISIONNO<=" + dataUpto + " GROUP BY P.WARD_NO,P.WARD_NAME ";
                    qry += "ORDER BY P.WARD_NO";
                    reportPath = $"{this._webHostEnv.WebRootPath}\\Reports\\RepWardWiseElectorsMunicipal.rdlc";
                }
            }
            
            else
            {
                return RedirectToAction("Login", "Home");
            }
            ds = dm.create_dataset(qry);
            // var report = new LocalReport(reportPath);
            //report.AddDataSource("PSLIST", ds.Tables[0]);
            // report.AddDataSource("PARAMETER", dt);
            //Stream reportDefinition; // your RDLC from file or resource
            //IEnumerable dataSource(); // your datasource for the report

            Microsoft.Reporting.NETCore.LocalReport report = new Microsoft.Reporting.NETCore.LocalReport();
            report.ReportPath = reportPath;
            report.DataSources.Add(new ReportDataSource("PSLIST", ds.Tables[0]));
            report.DataSources.Add(new ReportDataSource("PARAMETER", dt));
            // report.SetParameters(new[] { new ReportParameter("Parameter1", "Parameter value") });
            byte[] pdf = report.Render("PDF");
            //var result = report.Execute(RenderType.Pdf, extension, null, "application/pdf");
            // return File(result.MainStream, mimeType);
            Stream strm = new MemoryStream(pdf);
            var result = new FileStreamResult(strm, "application/pdf");
            // return File(pdf, mimeType, "myreport.pdf", false);
            return File(strm, mimeType);
        }
        #endregion

        #region PS-WISE ELECTOR CHANGES (ADDITIONS & DELETIONS)
        public IActionResult PSWiseElectorChanges(int id, string panmun, int revYear, int dataUpto, int keyValue)
        {
            System.Data.DataSet ds = new System.Data.DataSet();
            System.Data.DataTable dt = new System.Data.DataTable();
            System.Data.DataColumn col1 = new System.Data.DataColumn("REVYEAR");
            System.Data.DataColumn col2 = new System.Data.DataColumn("HEADER");
            dt.Columns.Add(col1);
            dt.Columns.Add(col2);
            System.Data.DataRow dr = dt.NewRow();
            string format = "PDF";
            int extension = (int)(DateTime.Now.Ticks >> 10);
            string mimeType = "application/pdf";

            // Dictionary<string, string> parameters = new Dictionary<string, string>();
            string reportPath = "";
            if (panmun == "P")
            {
                if (dataUpto == 0)
                {
                    dr["HEADER"] = "AS PER DRAFT PANCHAYAT ROLL";
                }
                else if (dataUpto == 1)
                {
                    dr["HEADER"] = "AS PER FINAL PANCHAYAT ROLL";
                }
            }
            else if (panmun == "M")
            {
                if (dataUpto == 0)
                {
                    dr["HEADER"] = "AS PER DRAFT MUNICIPAL ROLL";
                }
                else if (dataUpto == 1)
                {
                    dr["HEADER"] = "AS PER FINAL MUNICIPAL ROLL";
                }
            }
            dr["REVYEAR"] = revYear.ToString();
            dt.Rows.Add(dr);
            if (id == 1)
            {
                if (panmun == "P")
                {
                    qry = "SELECT D.DIST_CODE,D.DIST_NAME,T.TNAME,T.TNO,E.PART_NO,PL.PART_NAME, ";
                    qry += "MALE,FEMALE,TG,ISNULL(AMALE,0) AS AMALE,ISNULL(AFEMALE,0) AS AFEMALE,ISNULL(ATG,0) AS ATG, ";
                    qry += "ISNULL(DMALE,0) AS DMALE,ISNULL(DFEMALE,0) AS DFEMALE,ISNULL(DTG,0) AS DTG FROM ";
                    qry += "DRAFT_VW AS E JOIN SE_EROLL.DBO.PARTLIST AS PL ON E.PART_NO=PL.PART_NO AND E.PAN_MUN=";
                    qry += "PL.PAN_MUN JOIN PANCHAYAT AS P ON PL.PCODE=P.PCODE JOIN TEHSIL AS T ON P.TCODE=T.TCODE JOIN ";
                    qry += "DISTRICT AS D ON T.DIST_CODE=D.DIST_CODE LEFT JOIN ADDITIONS_VW AS A ON A.PART_NO=E.PART_NO AND A.PAN_MUN=";
                    qry += "E.PAN_MUN LEFT JOIN DELETIONS_VW AS DV ON DV.PART_NO=E.PART_NO AND DV.PAN_MUN=E.PAN_MUN ";
                    qry += "WHERE E.PAN_MUN='P' AND PL.AREA_TYPE='P' ORDER BY E.PART_NO";

                    reportPath = $"{this._webHostEnv.WebRootPath}\\Reports\\RepPSWiseElectorChanges.rdlc";
                }
                else if (panmun == "M")
                {
                    qry = "SELECT D.DIST_CODE,D.DIST_NAME,P.WARD_NAME AS TNAME,E.PART_NO,PL.PART_NAME, ";
                    qry += "MALE,FEMALE,TG,ISNULL(AMALE,0) AS AMALE,ISNULL(AFEMALE,0) AS AFEMALE,ISNULL(ATG,0) AS ATG, ";
                    qry += "ISNULL(DMALE,0) AS DMALE,ISNULL(DFEMALE,0) AS DFEMALE,ISNULL(DTG,0) AS DTG FROM ";
                    qry += "DRAFT_VW AS E JOIN SE_EROLL.DBO.PARTLIST AS PL ON E.PART_NO=PL.PART_NO AND E.PAN_MUN=";
                    qry += "PL.PAN_MUN JOIN MUN_WARD AS P ON PL.PCODE=P.WARD_NO JOIN TEHSIL AS T ON P.TCODE=T.TCODE JOIN ";
                    qry += "DISTRICT AS D ON T.DIST_CODE=D.DIST_CODE LEFT JOIN ADDITIONS_VW AS A ON A.PART_NO=E.PART_NO AND A.PAN_MUN=";
                    qry += "E.PAN_MUN LEFT JOIN DELETIONS_VW AS DV ON DV.PART_NO=E.PART_NO AND DV.PAN_MUN=E.PAN_MUN ";
                    qry += "WHERE E.PAN_MUN='M' AND PL.AREA_TYPE='M' ORDER BY E.PART_NO";
                    reportPath = $"{this._webHostEnv.WebRootPath}\\Reports\\RepPSWiseElectorChangesMunicipal.rdlc";
                }
            }
            else if (id == 2)
            {
                if (panmun == "P")
                {
                    qry = "SELECT D.DIST_CODE,D.DIST_NAME,T.TNAME,T.TNO,E.PART_NO,PL.PART_NAME, ";
                    qry += "MALE,FEMALE,TG,ISNULL(AMALE,0) AS AMALE,ISNULL(AFEMALE,0) AS AFEMALE,ISNULL(ATG,0) AS ATG, ";
                    qry += "ISNULL(DMALE,0) AS DMALE,ISNULL(DFEMALE,0) AS DFEMALE,ISNULL(DTG,0) AS DTG FROM ";
                    qry += "DRAFT_VW AS E JOIN SE_EROLL.DBO.PARTLIST AS PL ON E.PART_NO=PL.PART_NO AND E.PAN_MUN=";
                    qry += "PL.PAN_MUN JOIN PANCHAYAT AS P ON PL.PCODE=P.PCODE JOIN TEHSIL AS T ON P.TCODE=T.TCODE JOIN ";
                    qry += "DISTRICT AS D ON T.DIST_CODE=D.DIST_CODE LEFT JOIN ADDITIONS_VW AS A ON A.PART_NO=E.PART_NO AND A.PAN_MUN=";
                    qry += "E.PAN_MUN LEFT JOIN DELETIONS_VW AS DV ON DV.PART_NO=E.PART_NO AND DV.PAN_MUN=E.PAN_MUN ";
                    qry += "WHERE E.PAN_MUN='P' AND PL.AREA_TYPE='P' AND D.DIST_CODE=" + keyValue + " ORDER BY E.PART_NO";
                    reportPath = $"{this._webHostEnv.WebRootPath}\\Reports\\RepDistrictPSWiseElectorChanges.rdlc";
                }
                else if (panmun == "M")
                {
                    qry = "SELECT D.DIST_CODE,D.DIST_NAME,P.WARD_NAME AS TNAME,E.PART_NO,PL.PART_NAME, ";
                    qry += "MALE,FEMALE,TG,ISNULL(AMALE,0) AS AMALE,ISNULL(AFEMALE,0) AS AFEMALE,ISNULL(ATG,0) AS ATG, ";
                    qry += "ISNULL(DMALE,0) AS DMALE,ISNULL(DFEMALE,0) AS DFEMALE,ISNULL(DTG,0) AS DTG FROM ";
                    qry += "DRAFT_VW AS E JOIN SE_EROLL.DBO.PARTLIST AS PL ON E.PART_NO=PL.PART_NO AND E.PAN_MUN=";
                    qry += "PL.PAN_MUN JOIN MUN_WARD AS P ON PL.PCODE=P.WARD_NO JOIN TEHSIL AS T ON P.TCODE=T.TCODE JOIN ";
                    qry += "DISTRICT AS D ON T.DIST_CODE=D.DIST_CODE LEFT JOIN ADDITIONS_VW AS A ON A.PART_NO=E.PART_NO AND A.PAN_MUN=";
                    qry += "E.PAN_MUN LEFT JOIN DELETIONS_VW AS DV ON DV.PART_NO=E.PART_NO AND DV.PAN_MUN=E.PAN_MUN ";
                    qry += "WHERE E.PAN_MUN='M' AND PL.AREA_TYPE='M' ORDER BY E.PART_NO";
                    reportPath = $"{this._webHostEnv.WebRootPath}\\Reports\\RepPSWiseElectorChangesMunicipal.rdlc";
                }
            }
            else if (id == 3)
            {
                if (panmun == "P")
                {
                    qry = "SELECT D.DIST_CODE,D.DIST_NAME,P.PAN_NAME AS TNAME,T.TNO,E.PART_NO,PL.PART_NAME, ";
                    qry += "MALE,FEMALE,TG,ISNULL(AMALE,0) AS AMALE,ISNULL(AFEMALE,0) AS AFEMALE,ISNULL(ATG,0) AS ATG, ";
                    qry += "ISNULL(DMALE,0) AS DMALE,ISNULL(DFEMALE,0) AS DFEMALE,ISNULL(DTG,0) AS DTG FROM ";
                    qry += "DRAFT_VW AS E JOIN SE_EROLL.DBO.PARTLIST AS PL ON E.PART_NO=PL.PART_NO AND E.PAN_MUN=";
                    qry += "PL.PAN_MUN JOIN PANCHAYAT AS P ON PL.PCODE=P.PCODE JOIN TEHSIL AS T ON P.TCODE=T.TCODE JOIN ";
                    qry += "DISTRICT AS D ON T.DIST_CODE=D.DIST_CODE LEFT JOIN ADDITIONS_VW AS A ON A.PART_NO=E.PART_NO AND A.PAN_MUN=";
                    qry += "E.PAN_MUN LEFT JOIN DELETIONS_VW AS DV ON DV.PART_NO=E.PART_NO AND DV.PAN_MUN=E.PAN_MUN ";
                    qry += "WHERE E.PAN_MUN='P' AND PL.AREA_TYPE='P' AND T.TCODE=" + keyValue + " ORDER BY E.PART_NO";
                    reportPath = $"{this._webHostEnv.WebRootPath}\\Reports\\RepTehsilPSWiseElectorChanges.rdlc";
                }
                else if (panmun == "M")
                {
                    qry = "SELECT D.DIST_CODE,D.DIST_NAME,P.WARD_NAME AS TNAME,E.PART_NO,PL.PART_NAME, ";
                    qry += "MALE,FEMALE,TG,ISNULL(AMALE,0) AS AMALE,ISNULL(AFEMALE,0) AS AFEMALE,ISNULL(ATG,0) AS ATG, ";
                    qry += "ISNULL(DMALE,0) AS DMALE,ISNULL(DFEMALE,0) AS DFEMALE,ISNULL(DTG,0) AS DTG FROM ";
                    qry += "DRAFT_VW AS E JOIN SE_EROLL.DBO.PARTLIST AS PL ON E.PART_NO=PL.PART_NO AND E.PAN_MUN=";
                    qry += "PL.PAN_MUN JOIN MUN_WARD AS P ON PL.PCODE=P.WARD_NO JOIN TEHSIL AS T ON P.TCODE=T.TCODE JOIN ";
                    qry += "DISTRICT AS D ON T.DIST_CODE=D.DIST_CODE LEFT JOIN ADDITIONS_VW AS A ON A.PART_NO=E.PART_NO AND A.PAN_MUN=";
                    qry += "E.PAN_MUN LEFT JOIN DELETIONS_VW AS DV ON DV.PART_NO=E.PART_NO AND DV.PAN_MUN=E.PAN_MUN ";
                    qry += "WHERE E.PAN_MUN='M' AND PL.AREA_TYPE='M' ORDER BY E.PART_NO";
                    reportPath = $"{this._webHostEnv.WebRootPath}\\Reports\\RepPSWiseElectorChangesMunicipal.rdlc";
                }
            }
            else if (id == 4)
            {
                if (panmun == "P")
                {
                    qry = "SELECT P.PCODE AS DIST_CODE,P.PAN_NAME AS DIST_NAME,T.TNAME,T.TNO,E.PART_NO,PL.PART_NAME, ";
                    qry += "MALE,FEMALE,TG,ISNULL(AMALE,0) AS AMALE,ISNULL(AFEMALE,0) AS AFEMALE,ISNULL(ATG,0) AS ATG, ";
                    qry += "ISNULL(DMALE,0) AS DMALE,ISNULL(DFEMALE,0) AS DFEMALE,ISNULL(DTG,0) AS DTG FROM ";
                    qry += "DRAFT_VW AS E JOIN SE_EROLL.DBO.PARTLIST AS PL ON E.PART_NO=PL.PART_NO AND E.PAN_MUN=";
                    qry += "PL.PAN_MUN JOIN PANCHAYAT AS P ON PL.PCODE=P.PCODE JOIN TEHSIL AS T ON P.TCODE=T.TCODE JOIN ";
                    qry += "DISTRICT AS D ON T.DIST_CODE=D.DIST_CODE LEFT JOIN ADDITIONS_VW AS A ON A.PART_NO=E.PART_NO AND A.PAN_MUN=";
                    qry += "E.PAN_MUN LEFT JOIN DELETIONS_VW AS DV ON DV.PART_NO=E.PART_NO AND DV.PAN_MUN=E.PAN_MUN ";
                    qry += "WHERE E.PAN_MUN='P' AND PL.AREA_TYPE='P' AND P.PCODE=" + keyValue + " ORDER BY E.PART_NO";
                    reportPath = $"{this._webHostEnv.WebRootPath}\\Reports\\RepPanchayatPSWiseElectorChanges.rdlc";
                }
                else if (panmun == "M")
                {
                    qry = "SELECT P.WARD_NAME AS TNAME,E.PART_NO,PL.PART_NAME,SUM(CASE GENDER WHEN 'M' THEN 1 ";
                    qry += "ELSE 0 END) AS MALE,SUM(CASE GENDER WHEN 'F' THEN 1 ELSE 0 END) AS FEMALE,SUM(CASE GENDER WHEN 'T' ";
                    qry += "THEN 1 ELSE 0 END) AS TG FROM SE_EROLL.DBO.E_DETAIL AS E JOIN SE_EROLL.DBO.PARTLIST AS PL ON ";
                    qry += "E.PART_NO=PL.PART_NO AND E.PAN_MUN=PL.PAN_MUN JOIN MUN_WARD AS P ON PL.PCODE=P.WARD_NO JOIN ";
                    qry += "TEHSIL AS T ON P.TCODE=T.TCODE  WHERE E.PAN_MUN='M' ";
                    qry += "AND PL.AREA_TYPE='M' AND E.REVISIONNO<=" + dataUpto + " GROUP BY P.WARD_NAME,E.PART_NO,PL.PART_NAME ";
                    qry += "ORDER BY E.PART_NO";
                    reportPath = $"{this._webHostEnv.WebRootPath}\\Reports\\RepPSWiseElectorsMunicipal.rdlc";
                }
            }
            else
            {
                return RedirectToAction("Login", "Home");
            }
            ds = dm.create_dataset(qry);
            // var report = new LocalReport(reportPath);
            //report.AddDataSource("PSLIST", ds.Tables[0]);
            // report.AddDataSource("PARAMETER", dt);
            //Stream reportDefinition; // your RDLC from file or resource
            //IEnumerable dataSource(); // your datasource for the report

            Microsoft.Reporting.NETCore.LocalReport report = new Microsoft.Reporting.NETCore.LocalReport();
            report.ReportPath = reportPath;
            report.DataSources.Add(new ReportDataSource("PSLIST", ds.Tables[0]));
            report.DataSources.Add(new ReportDataSource("PARAMETER", dt));
            // report.SetParameters(new[] { new ReportParameter("Parameter1", "Parameter value") });
            byte[] pdf = report.Render("PDF");
            //var result = report.Execute(RenderType.Pdf, extension, null, "application/pdf");
            // return File(result.MainStream, mimeType);
            Stream strm = new MemoryStream(pdf);
            var result = new FileStreamResult(strm, "application/pdf");
            // return File(pdf, mimeType, "myreport.pdf", false);
            return File(strm, mimeType);
        }
        #endregion
    }
}
