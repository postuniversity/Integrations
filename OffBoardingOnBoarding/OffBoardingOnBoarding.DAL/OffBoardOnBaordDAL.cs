using log4net;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace OffBoardingOnBoarding.DAL
{
    /// <summary>
    /// 
    /// </summary>
    public class OffBoardOnBaordDAL : ISQLDAL
    {
        public static readonly ILog infoLogger = LogManager.GetLogger("log4net-default-repository", "InfoLogFile");
        public static readonly ILog errorLogger = LogManager.GetLogger("log4net-default-repository", "ErrorLogFile");

        public string ConnectionString { get; set; }
        public string Delimeter { get; set; }

        public OffBoardOnBaordDAL()
        {
            ConnectionString = ConfigurationManager.ConnectionStrings["SQLConnectionString"].ToString();
            Delimeter = ConfigurationManager.AppSettings["Delimiter"].ToString();
        }


        public int SaveOffBoardOnBoardStatusReport(string source, string starttime, string endtime, string comments, string status, string successfulruntime, int totalrecordcount, string outputfilename, string outputfilelocation, int userid)
        {
            //Insert SuccessfulRunTime in Table
            infoLogger.Info("Saving OffBoardOnBoardStatusReport started!");
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    SqlCommand cmd = new SqlCommand("customer.SaveOffBoardOnBoardStatusReport",con);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;                    
                    cmd.Parameters.AddWithValue("@Source", source);
                    cmd.Parameters.AddWithValue("@StartTime", starttime);
                    cmd.Parameters.AddWithValue("@EndTime", endtime);
                    cmd.Parameters.AddWithValue("@Comments", comments);
                    cmd.Parameters.AddWithValue("@Status", status);
                    cmd.Parameters.AddWithValue("@SuccessfulRuntime", successfulruntime);
                    cmd.Parameters.AddWithValue("@TotalRecordCount", totalrecordcount);
                    cmd.Parameters.AddWithValue("@OutputFileName", outputfilename);
                    cmd.Parameters.AddWithValue("@OutputFileLocation", outputfilelocation);
                    cmd.Parameters.AddWithValue("@UserId", userid);
                    cmd.Parameters.Add("@id", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd.Connection.Open();
                    //update field
                    cmd.ExecuteNonQuery();
                    int reportId = Convert.ToInt32(cmd.Parameters["@id"].Value);
                    cmd.Connection.Close();                    
                    infoLogger.Info("Saving OffBoardOnBoardStatusReport completed!");
                    return reportId;
                }
            }
            catch (Exception ex)
            {
                errorLogger.Info(String.Format("{0} -INNER EXCEPTION: {1}", ex, ex.ToString()));
                return -1;
            }
        }

        /// <summary>
        /// @id int       
        /// </summary>
        /// <returns></returns>
        public int UpdateOffBoardOnBoardStatusReport(string status, string successfultime, string endtime, int id)
        {
            infoLogger.Info("Update OffBoardOnBoardStatusReport started!");
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    SqlCommand cmd = new SqlCommand("customer.UpdateOffBoardOnBoardStatusReport",con);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    //cmd.CommandText = "";
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@status", status);
                    cmd.Parameters.AddWithValue("@successfulruntime", successfultime);
                    cmd.Parameters.AddWithValue("@endtime", endtime);
                    cmd.Connection.Open();
                    cmd.ExecuteNonQuery();
                }

                infoLogger.Info("Update OffBoardOnBoardStatusReport completed!");

                return 0;
            }
            catch(Exception ex)
            {
                errorLogger.Info(String.Format("{0} -INNER EXCEPTION: {1}", ex, ex.ToString()));
                return -1;
            }
        }

        public SqlDataReader GetOffBoardOnBoardStudents()
        {
            infoLogger.Info("GetOffBoardOnBoardStudents started!");

            string fileHeader = string.Empty;
            try
            {
                SqlConnection con = new SqlConnection(ConnectionString);
                using (SqlCommand cmd = new SqlCommand("customer.uspGetOffBoardOnBoardStudents", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    //cmd.CommandText = "customer.uspGetOffBoardOnBoardStudents";
                    cmd.Connection.Open();
                    var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                    infoLogger.Info("GetOffBoardOnBoardStudents completed!");
                    return reader;
                }
            }
            catch (Exception ex)
            {
                errorLogger.Info(String.Format("{0} -INNER EXCEPTION: {1}", ex, ex.ToString()));
                return null;
            }
        }
    }
}
