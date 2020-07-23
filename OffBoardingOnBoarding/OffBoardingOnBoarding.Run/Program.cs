using OffBoardingOnBoarding.Data;
using OffBoardingOnBoarding.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OffBoardingOnBoardingProcess
{
    class Program
    {
        static void Main(string[] args)
        {
            FileGenerator f = new FileGenerator(new ReportFromOData(), new ReportFromSQL());
            f.Generate();
            Console.WriteLine("Completed");
        }
    }
}
