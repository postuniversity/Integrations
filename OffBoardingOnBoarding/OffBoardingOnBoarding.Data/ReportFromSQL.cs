using log4net;
using System;
using System.Configuration;
using System.Data.SqlClient;
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
        public string SqlQUery { get; set; }
        /// <summary>
        /// /
        /// </summary>
        public string Delimeter { get; set; }
        /// <summary>
        /// /
        /// </summary>
        public string UpdateSuccessfulRunTimeQuery { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public ReportFromSQL()
        {
            //infoLogger.Info("Get Sql Config Values...");
            ConnectionString = ConfigurationManager.ConnectionStrings["SQLConnectionString"].ToString();
            FileFolder = ConfigurationManager.AppSettings["FileFolder"].ToString();
            FileName = ConfigurationManager.AppSettings["FileName"].ToString();
            SqlQUery = ConfigurationManager.AppSettings["SqlQuery"].ToString();
            Delimeter = ConfigurationManager.AppSettings["Delimiter"].ToString();
            UpdateSuccessfulRunTimeQuery = ConfigurationManager.AppSettings["UpdateSuccessfulRunTimeQuery"].ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int Generate()
        {
            bool successfulRun;
            var successfulRunTime = DateTime.Now;
            var sqlFormattedSuccessfulRunTime = successfulRunTime.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.CreateSpecificCulture("en-US"));
            var filefolderformattted = String.Format(FileFolder + FileName, successfulRunTime.ToString("yyyyMMddHHmmss", CultureInfo.CreateSpecificCulture("en-US")));
            var sqlstringformatted = string.Format(SqlQUery, sqlFormattedSuccessfulRunTime);
            try
            {
                infoLogger.Info(" Generate using Sql query started!");
                //Run to Generate Report
                successfulRun = RunReport(filefolderformattted, sqlstringformatted);
                if (successfulRun == true)
                {
                    UpdateSuccessfulRun(sqlFormattedSuccessfulRunTime);
                }
                //infoLogger.Info("End file generation using Sql query...");
                infoLogger.Info(" Generate using Sql query completed!");
                return 0;
            }
            catch (Exception ex)
            {
                errorLogger.Error(String.Format("{0} -INNER EXCEPTION: {1}", ex, ex.InnerException));
                return -1;
            }
        }

        /// <summary>
        /// Update Successful runtime in customer schema si_configurations
        /// </summary>
        /// <param name="sqlFormattedSuccessfulRunTime"></param>
        private void UpdateSuccessfulRun(string sqlFormattedSuccessfulRunTime)
        {
            var UpdateSuccessfulRunTimeQueryformat = string.Format(UpdateSuccessfulRunTimeQuery, sqlFormattedSuccessfulRunTime);
            //Update SuccessfulRunTime in Table
            UpdateSuccessfulRunTime(UpdateSuccessfulRunTimeQueryformat);
        }

        /// <summary>
        /// Get Data from SQL Server and save data as a file
        /// </summary>
        /// <param name="filefolderformattted"></param>
        /// <param name="sqlstringformatted"></param>
        /// <returns></returns>
        private bool RunReport(string filefolderformattted, string sqlstringformatted)
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
                            SqlCommand cmd = new SqlCommand(sqlstringformatted, con);
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
                infoLogger.Info(" Save Report using Sql started!");

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
        /// Update SuccessfulRunTime in Table
        /// </summary>
        /// <param name="UpdateSuccessfulRunTimeQueryformat"></param>
        private void UpdateSuccessfulRunTime(string UpdateSuccessfulRunTimeQueryformat)
        {
            try
            {
                infoLogger.Info(" UpdateSuccessfulRunTime started!");
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    SqlCommand cmd = new SqlCommand(UpdateSuccessfulRunTimeQueryformat, con);

                    cmd.Connection.Open();
                    //update field
                    cmd.ExecuteNonQuery();

                    infoLogger.Info(" UpdateSuccessfulRunTime completed!");
                }
            }
            catch (Exception ex)
            {
                errorLogger.Info(String.Format("{0} -INNER EXCEPTION: {1}", ex, ex.InnerException));
            }
        }
    }
}
