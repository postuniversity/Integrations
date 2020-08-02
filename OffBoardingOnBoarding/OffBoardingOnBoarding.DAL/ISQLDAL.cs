using System.Data.SqlClient;

namespace OffBoardingOnBoarding.DAL
{
    public interface ISQLDAL
    {
        int SaveOffBoardOnBoardStatusReport(string source, string starttime, string endtime, string comments, string status, string successfulruntime, int totalrecordcount, string outputfilename, string outputfilelocation, int userid);
        int UpdateOffBoardOnBoardStatusReport(string status, string successfultime, string endtime, int id);
        SqlDataReader GetOffBoardOnBoardStudents();
    }
}
