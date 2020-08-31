using System.Data.SqlClient;

namespace OffBoardingOnBoarding.DAL
{
    public interface ISQLDAL
    {
        int SaveOffBoardOnBoardStatusReport(string source, string starttime, string endtime, string comments, string status, string reportGenerationToDate, int totalrecordcount, string outputfilename, string outputfilelocation, int userid);
        int UpdateOffBoardOnBoardStatusReport(string status, string comments, string reportGenerationToDate, int totalRecordCount, string outputfilename, string outputfilelocation, string endtime, int id);
        SqlDataReader GetOffBoardOnBoardStudents(string reportGenerationToDate);
    }
}
