using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
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
        public string CNSApiKey { get; set; }
        public string ODataQueryURL { get; set; }

        public string FileFolder { get; set; }

        public string FileName { get; set; }

        public string Delimeter { get; set; }
        public ReportFromOData()
        {
            //Log.info("Generate started in OData class...");
            CNSApiKey = ConfigurationManager.AppSettings["CNSApiKey"].ToString();
            ODataQueryURL = ConfigurationManager.AppSettings["OdataQuery"].ToString();
            FileFolder = ConfigurationManager.AppSettings["FileFolder"].ToString();
            FileName = ConfigurationManager.AppSettings["FileName"].ToString();
            Delimeter = ConfigurationManager.AppSettings["Delimiter"].ToString();

        }
        /// <summary>
        /// Generate Student Status report from OData query
        /// </summary>
        /// <returns></returns>
        public int Generate()
        {          
            try
            {
                var odataResult = string.Empty;
                //http call to get odata query
                odataResult = GetHttpResponse(CNSApiKey, ODataQueryURL);
                //Log.info("HTTP Response retrieved in  OData class...");
                //Get all root values in dictionary as results[Deserializing json string] 
                var jsonData = getDataFromJSON(odataResult);
                ////Log.info("Http response parsed into JSON in OData class...");
                //Write the dictionary data to file
                SaveReport(jsonData);
                //Log.info("Save report completed in OData class...");
                //log.Message("GenerateFileFromOdataQuery", "File Generated Successfully", "Info", "", null);
                return 0;
            }
            catch (Exception ex)
            {
                ////Log.info("Exception ex in OData class...");
                return -1;
                //throw ;
                //log.Message("GenerateFileFromOdataQuery", "", "Error", "", ex);                
            }
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
            //Log.info("Generate started in GetHttpResponse of  OData class...");
            var httpResponse = client.GetAsync(OdataQueryURl).Result;
            httpResponse.EnsureSuccessStatusCode();
            //Output from OdataQuery
            return httpResponse.Content.ReadAsStringAsync().Result;
        }

        /// <summary>
        /// Save Report
        /// </summary>
        /// <param name="results"></param>
        private void SaveReport(Dictionary<string, string> results)
        {
            if (!Directory.Exists(FileFolder))
                Directory.CreateDirectory(FileFolder);

            //Log.info("SaveReport started in OData class...");
            String fileHeader = string.Empty;
            String fileData = string.Empty;
            int j = 0;
            string fileFormatted = FileFolder + FileName;

            using (FileStream fs = new FileStream(String.Format(fileFormatted, DateTime.Now.ToString("yyyyMMddHHmmss")), FileMode.CreateNew, FileAccess.ReadWrite))
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
                            j++;
                        }
                    }
                    sw.Write(fileData);
                }
            }
            //Log.info("SaveReport completed in OData class...");
        }

        /// <summary>
        /// Get Data from JSON as key values
        /// </summary>
        /// <param name="odataResult"></param>
        /// <returns></returns>
        private static Dictionary<string, string> getDataFromJSON(string odataResult)
        {
            //Log.info("GetDataFromJSON started in OData class...");
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
            //Log.info("GetDataFromJSON completed in OData class...");
            return results;
        }
    }
}
