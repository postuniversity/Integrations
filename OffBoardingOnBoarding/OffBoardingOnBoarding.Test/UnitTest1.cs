﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using OffBoardingOnBoarding.Data;
using OffBoardingOnBoarding.Lib;

namespace OffBoardingOnBoarding.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            FileGenerator f = new FileGenerator(new ReportFromOData(), new ReportFromSQL());
            f.Generate();
        }
    }
}
