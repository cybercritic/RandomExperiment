using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.IO;

namespace Random_Experiment_Server.DB
{
    public class SQLData
    {
        public string User { get; set; }
        public int TimeZone { get; set; }
        public DateTime Time { get; set; }
        public bool Active { get; set; }
        public int Count { get; set; }
        public double Mean { get; set; }
        public double Median { get; set; }
        public double StdDev { get; set; }
    }

    public class SQLQueries
    {
        //cmd sqllocaldb start/stop
        public SqlConnection SQLraw = new SqlConnection();

        public SQLQueries()
        {
            try
            {
                SqlConnectionStringBuilder str = new SqlConnectionStringBuilder();
                str.DataSource = @"(LocalDB)\MSSQLLocalDB";
                str.AttachDBFilename = Directory.GetCurrentDirectory() + @"\DB\random_db.mdf";
                str.IntegratedSecurity = true;//false
                str.MultipleActiveResultSets = true;
                //str.UserID = "username";
                //str.Password = "password";
                SQLraw.ConnectionString = str.ConnectionString;
                SQLraw.Open();
            }
            catch (Exception e)
            { Console.WriteLine(e.Message); }
        }

        public void CloseConnection()
        {
            if (SQLraw.State == System.Data.ConnectionState.Open)
                SQLraw.Close();
        }

        #region Add
        public string AddData(SQLData data)
        {
            if (DataExists(data))
                return "error: data exists";
            else
                return InsertData(data);
        }

        #endregion

        #region Exists
        public bool DataExists(SQLData data)
        {
            int result = 0;

            string cmdStr = "SELECT COUNT(*) AS count FROM data " +
                            "WHERE LOWER(user) = @user AND time = @time AND (mean = @mean OR median = @median OR std_dev = @std_dev OR count = @count) ;";

            SqlCommand command = new SqlCommand(cmdStr, SQLraw);
            command.Parameters.AddWithValue("@user", data.User);
            command.Parameters.AddWithValue("@time", data.Time);
            command.Parameters.AddWithValue("@mean", data.Mean);
            command.Parameters.AddWithValue("@median", data.Median);
            command.Parameters.AddWithValue("@std_dev", data.StdDev);
            command.Parameters.AddWithValue("@count", data.Count);

            SqlDataReader reader = null;
            try
            {
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    result = Convert.ToInt32(reader["count"]);
                }
                reader.Close();
            }
            catch (Exception e)
            {
                Supporting.WriteLog(e.Message);
                return true;
            }

            return (result == 0 ? false : true);
        }

        #endregion

        #region Insert
        private string InsertData(SQLData data)
        {
            string cmdStr = " INSERT INTO data (user,time_zone,time,active,count,mean,median,std_dev)" +
                            " VALUES (@user , @time_zone, @time , @count, @mean, @median, @std_dev);";

            SqlCommand command = new SqlCommand(cmdStr, SQLraw);
            command.Parameters.AddWithValue("@user", data.User);
            command.Parameters.AddWithValue("@time_zone", data.TimeZone); 
            command.Parameters.AddWithValue("@time", data.Time);
            command.Parameters.AddWithValue("@active", data.Active);
            command.Parameters.AddWithValue("@count", data.Count);
            command.Parameters.AddWithValue("@mean", data.Mean);
            command.Parameters.AddWithValue("@median", data.Median);
            command.Parameters.AddWithValue("@std_dev", data.StdDev);

            try { command.ExecuteNonQuery(); }
            catch (Exception e)
            {
                Supporting.WriteLog(e.Message);
                return "error: adding data";
            }

            return "info: added data";
        }
        #endregion

        #region GetList
        public List<SQLData> GetDataListUser(string user, int days)
        {
            List<SQLData> result = new List<SQLData>();

            string cmdStr = " SELECT * FROM data " +
                            " WHERE user = @user AND time > @start_time";

            DateTime start_time = DateTime.UtcNow.AddDays(-days);

            SqlCommand command = new SqlCommand(cmdStr, SQLraw);
            command.Parameters.AddWithValue("@user", user);
            command.Parameters.AddWithValue("@start_time", start_time);
            
            SqlDataReader reader = null;
            try
            {
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    SQLData current = new SQLData();

                    current.User = Convert.ToString(reader["user"]);
                    current.TimeZone = Convert.ToInt32(reader["time_zone"]);
                    current.Time = Convert.ToDateTime(reader["time"]);
                    current.Active = Convert.ToBoolean(reader["active"]);
                    current.Count = Convert.ToInt32(reader["count"]);
                    current.Mean = Convert.ToDouble(reader["mean"]);
                    current.Median = Convert.ToDouble(reader["median"]);
                    current.StdDev = Convert.ToDouble(reader["std_dev"]);
                    result.Add(current);
                }
                reader.Close();
            }
            catch (Exception e)
            {
                Supporting.WriteLog(e.Message);
            }

            return result;
        }

        public List<SQLData> GetDataListTimeZone(int zone, int days)
        {
            List<SQLData> result = new List<SQLData>();

            string cmdStr = " SELECT * FROM data " +
                            " WHERE time_zone = @zone AND time > @start_time";

            DateTime start_time = DateTime.UtcNow.AddDays(-days);

            SqlCommand command = new SqlCommand(cmdStr, SQLraw);
            command.Parameters.AddWithValue("@zone", zone);
            command.Parameters.AddWithValue("@start_time", start_time);

            SqlDataReader reader = null;
            try
            {
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    SQLData current = new SQLData();

                    current.User = Convert.ToString(reader["user"]);
                    current.TimeZone = Convert.ToInt32(reader["time_zone"]);
                    current.Time = Convert.ToDateTime(reader["time"]);
                    current.Active = Convert.ToBoolean(reader["active"]);
                    current.Count = Convert.ToInt32(reader["count"]);
                    current.Mean = Convert.ToDouble(reader["mean"]);
                    current.Median = Convert.ToDouble(reader["median"]);
                    current.StdDev = Convert.ToDouble(reader["std_dev"]);
                    result.Add(current);
                }
                reader.Close();
            }
            catch (Exception e)
            {
                Supporting.WriteLog(e.Message);
            }

            return result;
        }
        #endregion

        #region Delete
        public string DeleteUserSQL(string username)
        {
            string cmdStr = " DELETE a FROM Transactions a " +
                            " INNER JOIN Accounts b ON a.Account_Id = b.Id " +
                            " WHERE b.username = @username;" +
                            " DELETE FROM Accounts " +
                            " WHERE username = @username;" +
                            " DELETE FROM UserInstitution " +
                            " WHERE username = @username;" +
                            " DELETE FROM Users " +
                            " WHERE username = @username;";

            SqlCommand command = new SqlCommand(cmdStr, SQLraw);
            command.Parameters.AddWithValue("@username", username);

            try { command.ExecuteNonQuery(); }
            catch (Exception e)
            {
                Supporting.WriteLog(e.ToString());
                return "error: delete failed";
            }

            return "info: success";
        }
        #endregion
    }
}
