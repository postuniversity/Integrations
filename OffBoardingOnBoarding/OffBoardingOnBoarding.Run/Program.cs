using log4net;
using OffBoardingOnBoarding.DAL;
using OffBoardingOnBoarding.Data;
using OffBoardingOnBoarding.Lib;

namespace OffBoardingOnBoardingProcess
{
    class Program
    {
        //Declaring logger
        private static readonly ILog infoLogger = LogManager.GetLogger("InfoLogFile");
        
        static void Main(string[] args)
        {
            //Init log
            log4net.Config.XmlConfigurator.Configure();
            infoLogger.Info("OffBoardingOnBoarding report generation started!");

            FileGenerator fileGenerator = new FileGenerator(new ReportFromOData(), new ReportFromSQL(new OffBoardOnBaordDAL()));
            fileGenerator.Generate();

            infoLogger.Info("OffBoardingOnBoarding report generation completed!");
        }
    }
}
