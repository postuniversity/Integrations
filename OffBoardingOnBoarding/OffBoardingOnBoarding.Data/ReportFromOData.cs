using log4net;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;

namespace OffBoardingOnBoarding.Data
{
    /// <summary>
    /// This class generates Student data report from OData Query
    /// </summary>
    public class ReportFromOData : IOData
    {
        //Declaring logger
        public static readonly ILog infoLogger = LogManager.GetLogger("log4net-default-repository", "InfoLogFile");
        public static readonly ILog errorLogger = LogManager.GetLogger("log4net-default-repository", "ErrorLogFile");

        public string CNSApiKey { get; set; }
        public string ODataQueryURL { get; set; }

        public string FileFolder { get; set; }

        public string FileName { get; set; }

        public string Delimeter { get; set; }

        public string ConnectionString { get; set; }
        public string UpdateSuccessfulRunTimeQuery { get; set; }
        public string GetLastSuccessfulRunTimeQuery { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public ReportFromOData()
        {
            CNSApiKey = ConfigurationManager.AppSettings["CNSApiKey"].ToString();
            ODataQueryURL = ConfigurationManager.AppSettings["OdataQuery"].ToString();
            FileFolder = ConfigurationManager.AppSettings["FileFolder"].ToString();
            FileName = ConfigurationManager.AppSettings["FileName"].ToString();
            Delimeter = ConfigurationManager.AppSettings["Delimiter"].ToString();
            ConnectionString = ConfigurationManager.ConnectionStrings["SQLConnectionString"].ToString();
            UpdateSuccessfulRunTimeQuery = ConfigurationManager.AppSettings["UpdateSuccessfulRunTimeQuery"].ToString();
            GetLastSuccessfulRunTimeQuery = ConfigurationManager.AppSettings["GetLastSuccessfulRunTimeQuery"].ToString();
        }
        /// <summary>
        /// Generate Student Status report from OData query
        /// </summary>
        /// <returns></returns>
        public int Generate()
        {
            try
            {
                bool successfulRun;
                var odataResult = string.Empty;
                var successfulRunTime = DateTime.Now;
                var sqlFormattedSuccessfulRunTime = successfulRunTime.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.CreateSpecificCulture("en-US"));
                var filefolderformattted = String.Format(FileFolder + FileName, successfulRunTime.ToString("yyyyMMddHHmmss", CultureInfo.CreateSpecificCulture("en-US")));
                //http call to get odata query
                odataResult = GetHttpResponse(CNSApiKey, string.Format(ODataQueryURL, GetFromDate(), successfulRunTime.ToUniversalTime().ToString("o")));
                infoLogger.Info("HTTP Response retrieved in  OData class...");
                //Get all root values in dictionary as results[Deserializing json string] 
                var jsonData = getDataFromJSON(odataResult);
                infoLogger.Info("Http response parsed into JSON in OData class...");
                //Write the dictionary data to file
                successfulRun = SaveReport(jsonData, filefolderformattted);
                if (successfulRun == true)
                {
                    UpdateSuccessfulRun(sqlFormattedSuccessfulRunTime);
                }
                return 0;
            }
            catch (Exception ex)
            {
                errorLogger.Info(String.Format("{0} -INNER EXCEPTION: {1}", ex, ex.InnerException));
                return -1;
            }
        }
        /// <summary>
        /// Get FromDate Search Value from customer schema  
        /// </summary>
        /// <returns></returns>
        private string GetFromDate()
        {
            string fromDate = "";
            try
            {
                infoLogger.Info("Get LastSuccessfulRunTime search value from table started!");
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    SqlCommand cmd = new SqlCommand(GetLastSuccessfulRunTimeQuery, con);

                    cmd.Connection.Open();
                    //Get fromdate search value from customer.si_Configurations table
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        fromDate = Convert.ToDateTime(reader.GetValue(0)).ToUniversalTime().ToString("o");
                    }
                    infoLogger.Info("Get LastSuccessfulRunTime search value  from table completed!");
                }
            }
            catch (Exception ex)
            {
                errorLogger.Info(String.Format("{0} -INNER EXCEPTION: {1}", ex, ex.InnerException));
            }
            return fromDate;
        }
        /// <summary>
        /// Get Http Response from OData query
        /// </summary>
        /// <param name="CNSApikey"></param>
        /// <param name="OdataQueryURl"></param>
        /// <returns></returns>
        private static string GetHttpResponse(string CNSApikey, string OdataQueryURl)
        {
            // string odataResult;
            HttpClient client = new HttpClient();
            //add apikey to headers
            client.DefaultRequestHeaders.Add("ApiKey", CNSApikey);
            //Get response from Odata Query
            infoLogger.Info("Generate started in GetHttpResponse of  OData class...");
            var httpResponse = client.GetAsync(OdataQueryURl).Result;
            httpResponse.EnsureSuccessStatusCode();
            //Output from OdataQuery
            return httpResponse.Content.ReadAsStringAsync().Result;
        }

        /// <summary>
        /// Save Report
        /// </summary>
        /// <param name="results"></param>
        private bool SaveReport(Dictionary<string, string> results, string filefolderformattted)
        {
            bool successfulRun = false;
            try
            {
                infoLogger.Info("SaveReport started in OData class...");
                String fileHeader = string.Empty;
                String fileData = string.Empty;
                int j = 0;
                if (results.Values.First() != "[]")
                {
                    if (!Directory.Exists(FileFolder))
                        Directory.CreateDirectory(FileFolder);
                    using (FileStream fs = new FileStream(filefolderformattted, FileMode.CreateNew, FileAccess.ReadWrite))
                    {
                        using (StreamWriter sw = new StreamWriter(fs))
                        {
                            //Header(1st row)
                            foreach (string headers in results.Keys.Where(headers => headers.Contains("value[0]")))
                            {
                                fileHeader = fileHeader + Delimeter + headers.Substring(9);
                            }
                            fileHeader = fileHeader.Substring(1) + Environment.NewLine;
                            sw.Write(fileHeader);
                            //Values
                            foreach (KeyValuePair<string, string> result in results)
                            {
                                if (result.Key == results.Keys.First())
                                {
                                    fileData = result.Value;
                                }
                                else if (result.Key.Contains(String.Format("value[{0}]", j)))
                                {
                                    fileData = fileData + Delimeter + result.Value.ToString();
                                }
                                else
                                {
                                    fileData = fileData + Environment.NewLine + result.Value.ToString();
                                    sw.Write(fileData);
                                    fileData = "";
                                    j++;
                                }
                                
                            }
                            sw.Write(fileData);
                        }
                    }
                }
                successfulRun = true;
                infoLogger.Info("Is Successful Run? " + successfulRun.ToString());
                infoLogger.Info("Save report completed in OData class...");
            }
            catch (Exception ex)
            {
                errorLogger.Info(String.Format("{0} -INNER EXCEPTION: {1}", ex, ex.InnerException));
            }
            return successfulRun;
        }

        /// <summary>
        /// Get Data from JSON as key values
        /// </summary>
        /// <param name="odataResult"></param>
        /// <returns></returns>
        private static Dictionary<string, string> getDataFromJSON(string odataResult)
        {
            infoLogger.Info("GetDataFromJSON started in OData class...");
            JObject jsonObject = JObject.Parse(odataResult);

            IEnumerable<JToken> jTokens = jsonObject.Descendants().Where(p => p.Count() == 0);

            //get json nodes as Key,Values
            Dictionary<string, string> results = jTokens.Aggregate(new Dictionary<string, string>(), (properties, jToken) =>
            {
                properties.Add(jToken.Path, jToken.ToString());
                return properties;
            });
            //remove first value which is not usefule @odatacontext....
            results.Remove(results.Keys.First());
            infoLogger.Info("GetDataFromJSON completed in OData class...");
            return results;
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
