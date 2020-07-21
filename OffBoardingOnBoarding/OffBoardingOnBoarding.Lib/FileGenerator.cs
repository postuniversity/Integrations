using OffBoardingOnBoarding.Data;
using System.Configuration;
using System.IO;

namespace OffBoardingOnBoarding.Lib
{
    /// <summary>
    /// 
    /// </summary>
    public class FileGenerator
    {        
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
                //get appconfig settings
                DataSource = ConfigurationManager.AppSettings["Query"].ToString().ToUpper();
                DestinationFolder = ConfigurationManager.AppSettings["FileFolder"].ToString();              
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
                //Create Destination Folder in case not exists
                if (!Directory.Exists(DestinationFolder))
                    Directory.CreateDirectory(DestinationFolder);

                //Generate csv file using Odata query
                var status = (DataSource == ODATAQUERY ? ReportFromOdata.Generate() : ReportFromSQL.Generate());
                //ensure file generated with success status(0), error = -1
                if (status == -1)
                {
                    //log unsuccessful emessage
                    //rollback lastsuccessful runtime
                }
                //Log successfull message
            }
        }    
}
