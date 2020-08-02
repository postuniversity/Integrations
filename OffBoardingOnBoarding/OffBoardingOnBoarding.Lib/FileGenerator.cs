using OffBoardingOnBoarding.Data;
using System.Configuration;
using log4net;

namespace OffBoardingOnBoarding.Lib
{
    /// <summary>
    /// 
    /// </summary>
    public class FileGenerator
    {
        //Declaring logger
        public static readonly ILog infoLogger = LogManager.GetLogger("log4net-default-repository", "InfoLogFile");
        /// <summary>
        /// 
        /// </summary>
        private const string ODATAQUERY = "ODATAQUERY";
        /// <summary>
        /// /
        /// </summary>
        public string DataSource { get; set; }
        /// <summary>
        /// /
        /// </summary>
        public string DestinationFolder { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CNSApikey { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string OdataQueryURl { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="odataQuery"></param>
        /// <param name="sqlQuery"></param>
        public FileGenerator(IOData odataQuery, ISql sqlQuery)
        {
            ReportFromOdata = odataQuery;
            ReportFromSQL = sqlQuery;
            DataSource = ConfigurationManager.AppSettings["DataSourceKey"];
            
        }
        /// <summary>
        /// 
        /// </summary>
        public IOData ReportFromOdata { get; }
        /// <summary>
        /// 
        /// </summary>
        public ISql ReportFromSQL { get; }
        /// <summary>
        /// 
        /// </summary>
        public void Generate()
        {
            infoLogger.Info(string.Format("Report being genereted from : {0}", DataSource));
            var status = (DataSource == ODATAQUERY ? ReportFromOdata.Generate() : ReportFromSQL.Generatev2());

        }
    }
}
