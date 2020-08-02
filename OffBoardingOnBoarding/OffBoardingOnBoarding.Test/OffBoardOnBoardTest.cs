using Microsoft.VisualStudio.TestTools.UnitTesting;
using OffBoardingOnBoarding.Data;
using OffBoardingOnBoarding.Lib;
using log4net;
using OffBoardingOnBoarding.DAL;

namespace OffBoardingOnBoarding.Test
{
    [TestClass]
    public class OffBoardOnBoardTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            FileGenerator f = new FileGenerator(new ReportFromOData(), new ReportFromSQL(new OffBoardOnBaordDAL()));
            f.Generate();            
        }
    }
}
