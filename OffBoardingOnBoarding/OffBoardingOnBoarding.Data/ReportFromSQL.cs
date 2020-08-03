using log4net;
using OffBoardingOnBoarding.DAL;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;

namespace OffBoardingOnBoarding.Data
{
    /// <summary>
    /// 
    /// </summary>
    public class ReportFromSQL : ISql
    {
        //Declaring logger
        public static readonly ILog infoLogger = LogManager.GetLogger("log4net-default-repository", "InfoLogFile");
        public static readonly ILog errorLogger = LogManager.GetLogger("log4net-default-repository", "ErrorLogFile");
        private static readonly string Okay = "OK";

        /// <summary>
        /// 
        /// </summary>
        public string ConnectionString { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string FileFolder { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ReportQuery { get; set; }
        /// <summary>
        /// /
        /// </summary>
        public string Delimeter { get; set; }
        /// <summary>
        /// /
        /// </summary>
        public string DataSource { get; set; }
        /// <summary>
        /// /
        /// </summary>
        public string NextRunAvailableFromPreviousRunQuery { get; set; }
        /// <summary>
        /// /
        /// </summary>
        public string SaveSuccessfulRunQuery { get; set; }
        public ISQLDAL Isqldal { get; }

        /// <summary>
        /// 
        /// </summary>
        public ReportFromSQL( ISQLDAL isqldal)
        {
            //infoLogger.Info("Get Sql Config Values...");
            DataSource = ConfigurationManager.AppSettings["DataSourceKey"].ToString();
            ConnectionString = ConfigurationManager.ConnectionStrings["SQLConnectionString"].ToString();
            FileFolder = ConfigurationManager.AppSettings["FileFolder"].ToString();
            FileName = ConfigurationManager.AppSettings["FileName"].ToString();
            ReportQuery = ConfigurationManager.AppSettings["StudentStatusReportQuery"].ToString();
            Delimeter = ConfigurationManager.AppSettings["Delimiter"].ToString();
            //latest next runtime (from previous successful run)
            NextRunAvailableFromPreviousRunQuery = ConfigurationManager.AppSettings["PreviousSuccessfulRunTimeQuery"].ToString();
            // save successful run time on report generation
            SaveSuccessfulRunQuery = ConfigurationManager.AppSettings["InsertSuccessfulRunTimeQuery"].ToString();
            Isqldal = isqldal;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>

        public int Generatev2()
        {

            //save offboardstudent status report inprogress
            //getstudnets and save report
            //update offboardstudent status report completed
            try
            {
                var reportgenerationtime = DateTime.Now;
                var filename = String.Format(FileName, reportgenerationtime.ToString("yyyyMMddHHmmss", CultureInfo.CreateSpecificCulture("en-US")));
                var outputFile = String.Format(FileFolder + filename);
                infoLogger.Info("saving OffBoardOnBoardStatusReport");
                var runId = Isqldal.SaveOffBoardOnBoardStatusReport("sql", DateTime.Now.ToString(), string.Empty, "", "in-progress", string.Empty, 0 ,string.Empty,string.Empty, 1);
                infoLogger.Info("saved OffBoardOnBoardStatusReport, runid :" + runId);
                int totalRecordCount;
                string generateReportOutput;
                //get count from reader and save
                //generate and save the report
                if (runId > 0)
                {                    
                    var reportsaved = GenerateReport(outputFile,out totalRecordCount,out generateReportOutput);

                    if (!reportsaved)
                    {
                        Isqldal.UpdateOffBoardOnBoardStatusReport("Error", generateReportOutput, reportgenerationtime.ToString(),0, filename, FileFolder, DateTime.Now.ToString(), runId);
                        errorLogger.Error(String.Format("Something went wrong in saving generating report : RUNID: {0}", runId));
                        return -1;
                    }
                    //update status
                    Isqldal.UpdateOffBoardOnBoardStatusReport("completed","Report Generated Successfully!", reportgenerationtime.ToString(), totalRecordCount, filename, FileFolder, DateTime.Now.ToString(), runId);
                    return 0;
                }
                errorLogger.Error(String.Format("Something went wrong in saving StudentStatusReport: RUNID: {0}",runId));
                return -1;
            }
            catch(Exception ex)
            {
                errorLogger.Error(String.Format("Something went wrong in Generatev2: Exception {0}", ex.Message));
                return -1;
            }            
        }


        public bool GenerateReport(string filename, out int totalRecordCount, out string generateReportOutput)
        {
            infoLogger.Info("GenerateReport started!");
            string fileHeader = string.Empty;
            var fileData = string.Empty;
            totalRecordCount = 0;
            generateReportOutput = string.Empty;
            try
            {
                if (!Directory.Exists(FileFolder))
                    Directory.CreateDirectory(FileFolder);
                
                //get data from DB
                var reader = Isqldal.GetOffBoardOnBoardStudents();
                if (reader != null)
                {
                    using (FileStream fs = new FileStream(filename, FileMode.CreateNew, FileAccess.ReadWrite))
                    {
                        using (StreamWriter sw = new StreamWriter(fs))
                        {
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                fileHeader = fileHeader + reader.GetName(i) + Delimeter;
                            }

                            fileHeader = fileHeader.Remove(fileHeader.Length - 1, 1) + Environment.NewLine;
                            
                            sw.Write(fileHeader);
                            //Insert header(once) in the output file
                            while (reader.Read())
                            {
                                //Write each line into the file [String cannot hold large memory]
                                fileData = string.Empty;
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    //Check for advanced datatypes
                                    if (reader.GetFieldType(i) == typeof(Byte[]) && reader.IsDBNull(i) == false)
                                    {
                                        byte[] byteArray = (Byte[])reader.GetValue(i);
                                        string byteString = Convert.ToBase64String(byteArray);
                                        fileData = fileData + byteString + Delimeter;
                                    }
                                    else
                                        fileData = fileData + reader.GetValue(i) + Delimeter;
                                }
                                //Write every row by removing last delimiter and move to next line
                                fileData = fileData.Remove(fileData.Length - 1, 1) + Environment.NewLine;
                                totalRecordCount++;
                                sw.Write(fileData);
                            }
                        }
                    }
                    infoLogger.Info(string.Format("GenerateReport completed!, #Total records Count : {0}", totalRecordCount));
                    return true;
                }
                generateReportOutput = "GenerateReport reader has no rows!";
                infoLogger.Info(generateReportOutput);
                return  false;
            }
            catch (Exception ex)
            {
                generateReportOutput = ex.Message.ToString() ;
                errorLogger.Error(String.Format("Something went wrong in GenerateReport: Exception {0}", ex.ToString()));
                return false;
            }

        }
                
        public int Generate()
        {
            bool reportGenerated;
            //set successfulruntime to current date
            var successfulRunTime = DateTime.Now;
            //time in est
            var sqlFormattedSuccessfulRunTime = successfulRunTime.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.CreateSpecificCulture("en-US"));
            // add timestamp to file name
            var filefolderformattted = String.Format(FileFolder + FileName, successfulRunTime.ToString("yyyyMMddHHmmss", CultureInfo.CreateSpecificCulture("en-US")));
            //fetch students for status report
            var reportQuery = GetQuery(ReportQuery);
            //
            var previousSuccessfulRuntimeQuery = GetQuery(NextRunAvailableFromPreviousRunQuery);
            try
            {
                //log the query being executed
                infoLogger.Info(string.Format(" Started Generating report using Sql query {0}", reportQuery));
                //Run to Generate Report
                reportGenerated = RunReport(filefolderformattted, reportQuery);
                //
                var previousSuccessfulRuntime = GetPreviousSuccessfulRuntime(previousSuccessfulRuntimeQuery, sqlFormattedSuccessfulRunTime);
                //
                if (reportGenerated)
                {
                    //
                    return SaveLastRunTimeOnSuccess(sqlFormattedSuccessfulRunTime, reportQuery, previousSuccessfulRuntime);
                }
                return SaveLastRunTimeOnError(sqlFormattedSuccessfulRunTime, reportQuery, previousSuccessfulRuntime);
            }
            catch (Exception ex)
            {
                errorLogger.Error(String.Format("{0} -INNER EXCEPTION: {1}", ex, ex.InnerException));
                return -1;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sqlFormattedSuccessfulRunTime"></param>
        /// <param name="reportQuery"></param>
        /// <param name="previousSuccessfulRuntime"></param>
        /// <returns></returns>
        private int SaveLastRunTimeOnError(string sqlFormattedSuccessfulRunTime, string reportQuery, string previousSuccessfulRuntime)
        {
            //Report Generation Failed, report error , set "NextRunTime to previousruntime", so report generation happens from previous run 
            InsertSuccessfulRun(sqlFormattedSuccessfulRunTime, DataSource, string.Empty, reportQuery.Replace("'", "''"), "Error", previousSuccessfulRuntime, previousSuccessfulRuntime/*NextSuccessfulRunTime*/);
            infoLogger.Error(" Generate report using Sql query completed with errors!");
            return -1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sqlFormattedSuccessfulRunTime"></param>
        /// <param name="reportQuery"></param>
        /// <param name="previousSuccessfulRuntime"></param>
        /// <returns></returns>
        private int SaveLastRunTimeOnSuccess(string sqlFormattedSuccessfulRunTime, string reportQuery, string previousSuccessfulRuntime)
        {
            //
            InsertSuccessfulRun(sqlFormattedSuccessfulRunTime, DataSource, string.Empty/* odataquery*/, reportQuery.Replace("'", "''"), "OK", previousSuccessfulRuntime, sqlFormattedSuccessfulRunTime);
            //infoLogger.Info("End file generation using Sql query...");
            infoLogger.Info(" Generate using Sql query completed!");
            return 0;
        }

        /// <summary>
        /// Insert Successful runtime in customer schema
        /// </summary>
        private void InsertSuccessfulRun(string RunDate, string ReportSource, string ODataQuery, string SqlQuery, string Status, string PreviousSuccessfulRunTime, string NextSuccessfulRunTime)
        {
            //Insert SuccessfulRunTime in Table
            infoLogger.Info("Inserting successful runtime");
            try
            {
                var insertSuccessfulRunTimeQuery = GetQuery(SaveSuccessfulRunQuery);
                string formattedInsertQuery = string.Format(insertSuccessfulRunTimeQuery, RunDate, ReportSource, ODataQuery, SqlQuery, Status, PreviousSuccessfulRunTime, NextSuccessfulRunTime);
                infoLogger.Info(string.Format(" Insert SuccessfulRunTime started - {0}!", formattedInsertQuery));
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    SqlCommand cmd = new SqlCommand(formattedInsertQuery, con);

                    cmd.Connection.Open();
                    //update field
                    cmd.ExecuteNonQuery();

                    infoLogger.Info(" Insert SuccessfulRunTime completed!");
                }
            }
            catch (Exception ex)
            {
                errorLogger.Info(String.Format("{0} -INNER EXCEPTION: {1}", ex, ex.InnerException));
            }
        }

        /// <summary>
        /// Get Data from SQL Server and save data as a file
        /// </summary>
        /// <param name="filefolderformattted"></param>
        /// <param name="sqlstringformatted"></param>
        /// <returns></returns>
        private bool RunReport(string filefolderformattted, string reportQuery)
        {
            infoLogger.Info(" RunReport using Sql started!");
            bool successfulRun = false;
            try
            {
                if (!Directory.Exists(FileFolder))
                    Directory.CreateDirectory(FileFolder);

                using (FileStream fs = new FileStream(filefolderformattted, FileMode.CreateNew, FileAccess.ReadWrite))
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        infoLogger.Info("Connecting to Sql Server...");
                        //Connecting to the server
                        using (SqlConnection con = new SqlConnection(ConnectionString))
                        {
                            con.Open();
                            //Get the SQL query
                            SqlCommand cmd = new SqlCommand(reportQuery, con);
                            string fileHeader = string.Empty;

                            //Used datareader to execute the query
                            SqlDataReader reader = cmd.ExecuteReader();

                            //write headers
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                fileHeader = fileHeader + reader.GetName(i) + Delimeter;
                            }

                            fileHeader = fileHeader.Remove(fileHeader.Length - 1, 1) + Environment.NewLine;

                            sw.Write(fileHeader);
                            //write Data
                            SaveReport(sw, reader);

                        }
                        successfulRun = true;
                        infoLogger.Info("Is Successful Run? " + successfulRun.ToString());
                        infoLogger.Info(" RunReport using Sql completed!");
                    }
                }
            }
            catch (Exception ex)
            {
                errorLogger.Info(String.Format("{0} -INNER EXCEPTION: {1}", ex, ex.InnerException));
            }
            return successfulRun;
        }


        /// <summary>
        /// Save data as Report
        /// </summary>
        /// <param name="sw"></param>
        /// <param name="reader"></param>
        private void SaveReport(StreamWriter sw, SqlDataReader reader)
        {
            try
            {
                infoLogger.Info(string.Format(" Save Report using Sql started, #records in Reader : {0}", reader.Cast<object>().Count()));

                while (reader.Read())
                {
                    var fileData = string.Empty;

                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        //Check for advanced datatypes
                        if (reader.GetFieldType(i) == typeof(Byte[]) && reader.IsDBNull(i) == false)
                        {
                            byte[] byteArray = (Byte[])reader.GetValue(i);
                            string byteString = Convert.ToBase64String(byteArray);
                            fileData = fileData + byteString + Delimeter;
                        }
                        else
                            fileData = fileData + reader.GetValue(i) + Delimeter;
                    }
                    //Write every row by removing last delimiter and move to next line
                    fileData = fileData.Remove(fileData.Length - 1, 1) + Environment.NewLine;
                    sw.Write(fileData);
                }

                infoLogger.Info(" Save Report using Sql completed!");
            }
            catch (Exception ex)
            {
                errorLogger.Info(String.Format("{0} -INNER EXCEPTION: {1}", ex, ex.InnerException));
            }
        }


        /// <summary>
        /// Update Get Report Query in Table
        /// </summary>
        private string GetQuery(string configQuery)
        {
            string query = string.Empty;
            try
            {
                infoLogger.Info("Get SQL Query from Config table started!");
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    SqlCommand cmd = new SqlCommand(configQuery, con);

                    cmd.Connection.Open();
                    //Get Query from customer.si_Configurations table
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        query = reader.GetValue(0).ToString();
                    }
                    infoLogger.Info("Get SQL Query from Config table completed!");
                }
            }
            catch (Exception ex)
            {
                errorLogger.Info(String.Format("{0} -INNER EXCEPTION: {1}", ex, ex.InnerException));
            }
            return query;
        }
        /// <summary>
        /// Get Previous Successful Runtime
        /// </summary>
        private string GetPreviousSuccessfulRuntime(string previousSuccessfulRuntimeQuery, string sqlFormattedSuccessfulRunTime)
        {
            string previousSuccessfulRuntime = "";
            try
            {
                infoLogger.Info("Get previousSuccessfulRuntime started!");
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    SqlCommand cmd = new SqlCommand(previousSuccessfulRuntimeQuery, con);

                    cmd.Connection.Open();
                    //Get Query from customer.si_Configurations table
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        previousSuccessfulRuntime = Convert.ToDateTime(reader.GetValue(0)).ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.CreateSpecificCulture("en-US"));
                    }

                    infoLogger.Info(string.Format("Get previousSuccessfulRuntime completed!-{0}", previousSuccessfulRuntime));
                }
            }
            catch (Exception ex)
            {
                errorLogger.Info(String.Format("{0} -INNER EXCEPTION: {1}", ex, ex.InnerException));
            }
            return previousSuccessfulRuntime;
        }
    }
}
