using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System;
using System.Collections;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;

namespace apireport
{
    public partial class Default : Page
    {
        Hashtable ht = null;
        string keyname, Param1, Param2, Param3, Param4, Param5 = "";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.Params["Param1"] != null)
            {
                Param1 = Request.Params["Param1"];
            }

            if (Request.Params["Param2"] != null)
            {
                Param2 = Request.Params["Param2"];
            }

            if (Request.Params["Param3"] != null)
            {
                Param3 = Request.Params["Param3"];
            }
            if (Request.Params["Param4"] != null)
            {
                Param4 = Request.Params["Param4"];
            }
            if (Request.Params["Param5"] != null)
            {
                Param5 = Request.Params["Param5"];
            }

            //prepare report
            if (Request.Params["keyname"] != null)
            {
                keyname = Request.Params["keyname"];
                ht = GetReportByKeyName(keyname);


                ReportDocument reportDocument = new ReportDocument();

                TableLogOnInfo logOnInfo = new TableLogOnInfo();
                ConnectionInfo ConnectionInfo = new ConnectionInfo();

                reportDocument.Load(HttpContext.Current.Server.MapPath("~/Reports/" + ht[keyname].ToString()));

                foreach (CrystalDecisions.CrystalReports.Engine.Table table in reportDocument.Database.Tables)
                {
                    logOnInfo = table.LogOnInfo;
                    ConnectionInfo = logOnInfo.ConnectionInfo;
                    //Set the Connection parameters.
                    ConnectionInfo.DatabaseName = ConfigurationManager.AppSettings["Database"];
                    ConnectionInfo.ServerName = ConfigurationManager.AppSettings["ServerIp"];

                    ConnectionInfo.Password = ConfigurationManager.AppSettings["Password"];
                    ConnectionInfo.UserID = ConfigurationManager.AppSettings["Username"];


                    table.ApplyLogOnInfo(logOnInfo);
                }

                // cryRpt.SetDatabaseLogon(userid, upwd, datasource, catalog, true);
                if (!string.IsNullOrEmpty(Param1))
                {
                    reportDocument.ParameterFields["Param1"].CurrentValues.AddValue(Param1);
                }
                if (!string.IsNullOrEmpty(Param2))
                {
                    reportDocument.ParameterFields["Param2"].CurrentValues.AddValue(Param2);
                }
                if (!string.IsNullOrEmpty(Param3))
                {
                    reportDocument.ParameterFields["Param3"].CurrentValues.AddValue(Param3);
                }
                if (!string.IsNullOrEmpty(Param4))
                {
                    reportDocument.ParameterFields["Param4"].CurrentValues.AddValue(Param4);
                }

                if (!string.IsNullOrEmpty(Param5))
                {
                    reportDocument.ParameterFields["Param5"].CurrentValues.AddValue(Param5);
                }
                //?keyname=InvoiceForTheMonth&Param1=LAGOS&Param2=01/01/2013
                //export report to stream
                System.IO.Stream stream = reportDocument.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                reportDocument.Dispose();
                byte[] PDFByteArray = new Byte[stream.Length];
                stream.Position = 0;
                stream.Read(PDFByteArray, 0, Convert.ToInt32(stream.Length));
                Context.Response.ClearContent();
                Context.Response.ClearHeaders();
                Context.Response.AddHeader("content-disposition", "filename=Report.pdf");
                Context.Response.ContentType = "application/pdf";
                Context.Response.AddHeader("content-length", PDFByteArray.Length.ToString());
                Context.Response.BinaryWrite(PDFByteArray);
                Context.Response.Flush();
                Context.Response.End();
            }
        }

        private Hashtable GetReportByKeyName(string keyname)
        {
            string SERVER, DATABASE, USERID, PASSWORD = "";
            SERVER = ConfigurationManager.AppSettings["ServerIp"];
            DATABASE = ConfigurationManager.AppSettings["Database"];
            USERID = ConfigurationManager.AppSettings["Username"];
            PASSWORD = ConfigurationManager.AppSettings["Password"];

            string ConString = string.Format("Data Source={0};Initial Catalog={1};User Id={2};Password={3};MultipleActiveResultSets=True;", SERVER, DATABASE, USERID, PASSWORD);
            Hashtable ht = new Hashtable();
            using (SqlConnection con = new SqlConnection(ConString))
            {
                string SQL = string.Format("select * from ReportMetaData where keyname = '{0}'", keyname);
                con.Open();
                SqlCommand cmd = new SqlCommand(SQL,con);
                cmd.CommandTimeout = 0;
                SqlDataReader r = cmd.ExecuteReader();
                while (r.Read())
                {
                  ht[keyname] = (r["ReportName"] is DBNull) ? "": r["ReportName"].ToString();
                }
                r.Close();
                con.Close();
                return ht;
            }
        }
    }
}