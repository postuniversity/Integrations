using log4net;
using OffBoardingOnBoarding.DAL;
using System;
using System.Configuration;
using System.Globalization;
using System.IO;

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
        private static readonly string OK = "OK";
        private static readonly string ERROR = "Error";
        private static readonly string INPROGRESS = "In-Progress";


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
     
        /// <summary>
        /// /
        /// </summary>
        public string Delimeter { get; set; }
        /// <summary>
        /// /
        /// </summary>
        public string DataSource { get; set; }
        /// </summary>
        public ISQLDAL ISQLDAL { get; }

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
            Delimeter = ConfigurationManager.AppSettings["Delimiter"].ToString();
            ISQLDAL = isqldal;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>

        public int Generate()
        {          
            try
            {
                var reportgenerationtime = DateTime.Now;
                var successfulRunTime= reportgenerationtime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CreateSpecificCulture("en-US"));
                var outputFileName = String.Format(FileName, reportgenerationtime.ToString("yyyyMMddHHmmss", CultureInfo.CreateSpecificCulture("en-US")));
                var outputFileLocation = String.Format(FileFolder + outputFileName);
                int totalRecordCount;
                string generateReportOutput;

                //step 1: save initial processing info
                infoLogger.Info("saving OffBoardOnBoardStatusReport");
                var runId = ISQLDAL.SaveOffBoardOnBoardStatusReport("sql", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CreateSpecificCulture("en-US")), string.Empty, "", INPROGRESS, string.Empty, 0 ,string.Empty,string.Empty, 1);
                infoLogger.Info("saved OffBoardOnBoardStatusReport, runid :" + runId);                
                
                //
                if (runId > 0)
                { 
                    //step 2: Generate and Save report in folder
                    var reportsaved = SaveReport(successfulRunTime, outputFileLocation,out totalRecordCount,out generateReportOutput);
                    //step 3: ensure report is saved ad update info accordingly
                    if (!reportsaved)
                    {
                        //step 3.a
                        ISQLDAL.UpdateOffBoardOnBoardStatusReport(ERROR, generateReportOutput, successfulRunTime,totalRecordCount, outputFileName, FileFolder, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CreateSpecificCulture("en-US")), runId);
                        errorLogger.Error(String.Format("Something went wrong in saving generating report : RUNID: {0}", runId));
                        return -1;
                    }
                    //step 3.b update status
                    ISQLDAL.UpdateOffBoardOnBoardStatusReport(OK, "Report Generated Successfully!", successfulRunTime, totalRecordCount, outputFileName, FileFolder, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CreateSpecificCulture("en-US")), runId);
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


        public bool SaveReport(string successfulRunTime, string filename, out int totalRecordCount, out string generateReportOutput)
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
                var reader = ISQLDAL.GetOffBoardOnBoardStudents(successfulRunTime);

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
     
    }
}
