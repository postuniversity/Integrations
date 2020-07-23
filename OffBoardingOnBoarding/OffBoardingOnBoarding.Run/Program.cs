using OffBoardingOnBoarding.Data;
using OffBoardingOnBoarding.Lib;
using System;
using log4net;

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
            infoLogger.Info("Start OffBoarding OnBoarding Process.");

            FileGenerator fileGenerator = new FileGenerator(new ReportFromOData(), new ReportFromSQL());
            fileGenerator.Generate();

            infoLogger.Info("End OffBoarding OnBoarding Process.");
        }
    }
}
