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
            string cmdStr = " INSERT INTO data (user,time_zone,time,count,mean,median,std_dev)" +
                            " VALUES (@user , @time_zone, @time , @count, @mean, @median, @std_dev);";

            SqlCommand command = new SqlCommand(cmdStr, SQLraw);
            command.Parameters.AddWithValue("@user", data.User);
            command.Parameters.AddWithValue("@time_zone", data.TimeZone); 
            command.Parameters.AddWithValue("@time", data.Time);
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

        /*
        #region GetList
        public List<DB_Institution> GetInstitutionList()
                {
                    List<DB_Institution> result = new List<DB_Institution>();

                    string cmdStr = " SELECT * FROM Institutions; ";

                    SqlCommand command = new SqlCommand(cmdStr, SQLraw);

                    SqlDataReader reader = null;
                    try
                    {
                        reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            DB_Institution current = new DB_Institution();

                            current.Id = Convert.ToInt64(reader["id"]);
                            current.Slug = Convert.ToString(reader["slug"]);
                            current.Name = Convert.ToString(reader["name"]);
                            current.MaxDays = Convert.ToInt64(reader["max_days"]);
                            current.UpdatedAt = Convert.ToDateTime(reader["updated_at"]);
                            current.SearchValue = Convert.ToString(reader["searchVal"]);
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

                public List<DB_Report> GetReportList(string username)
                {
                    List<DB_Report> result = new List<DB_Report>();

                    string cmdStr = " SELECT * FROM Reports " +
                                    " WHERE username = @username ;";

                    SqlCommand command = new SqlCommand(cmdStr, SQLraw);
                    command.Parameters.AddWithValue("@username", username);

                    SqlDataReader reader = null;
                    try
                    {
                        reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            DB_Report current = new DB_Report();

                            current.Id = Convert.ToInt64(reader["id"]);
                            current.Reference = Convert.ToString(reader["reference"]);
                            current.Username = Convert.ToString(reader["username"]);
                            current.StartDate = Convert.ToDateTime(reader["start_date"]);
                            current.EndDate = Convert.ToDateTime(reader["end_date"]);
                            result.Add(current);
                        }
                        reader.Close();
                    }
                    catch (Exception e)
                    {
                        Supporting.WriteLog(e.Message);
                        return null;
                    }

                    return result;
                }

                public List<DB_ReportItem> GetReportItemList(string reportReference)
                {
                    List<DB_ReportItem> result = new List<DB_ReportItem>();

                    string cmdStr = " SELECT * FROM ReportItem " +
                                    " WHERE report_reference = @report_reference ;";

                    SqlCommand command = new SqlCommand(cmdStr, SQLraw);
                    command.Parameters.AddWithValue("@report_reference", reportReference);

                    SqlDataReader reader = null;
                    try
                    {
                        reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            DB_ReportItem current = new DB_ReportItem();

                            current.Id = Convert.ToInt64(reader["id"]);
                            current.ReportReference = Convert.ToString(reader["report_reference"]);
                            current.BudgetReference = Convert.ToString(reader["budget_reference"]);
                            current.Amount = Convert.ToDouble(reader["amount"]);
                            result.Add(current);
                        }
                        reader.Close();
                    }
                    catch (Exception e)
                    {
                        Supporting.WriteLog(e.Message);
                        return null;
                    }

                    return result;
                }

                public List<DB_Transaction> GetReportTransactionsList(long reportItemID)
                {
                    List<DB_Transaction> result = new List<DB_Transaction>();

                    string cmdStr = " SELECT * FROM Transactions " +
                                    " JOIN ReportTransaction ON Transactions.id = ReportTransaction.transaction_id " +
                                    " WHERE ReportTransaction.report_item_id = @reportID ;";

                    SqlCommand command = new SqlCommand(cmdStr, SQLraw);
                    command.Parameters.AddWithValue("@reportID", reportItemID);

                    SqlDataReader reader = null;
                    try
                    {
                        reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            DB_Transaction current = new DB_Transaction();

                            current.Id = Convert.ToInt64(reader["id"]);
                            current.Account_Id = Convert.ToInt64(reader["account_Id"]);
                            current.TransactionDateTime = Convert.ToDateTime(reader["date_time"]);
                            current.Description = Convert.ToString(reader["description"]);
                            current.Amount = Convert.ToDouble(reader["amount"]);
                            current.Balance = Convert.ToDouble(reader["balance"]);
                            current.Type = Convert.ToString(reader["type"]);
                            current.Tags = Convert.ToString(reader["tags"]);
                            result.Add(current);
                        }
                        reader.Close();
                    }
                    catch (Exception e)
                    {
                        Supporting.WriteLog(e.Message);
                        return null;
                    }

                    return result;
                }

                public List<DB_BudgetItem> GetBudgetItemList()
                {
                    List<DB_BudgetItem> result = new List<DB_BudgetItem>();

                    string cmdStr = " SELECT * FROM BudgetItem; ";

                    SqlCommand command = new SqlCommand(cmdStr, SQLraw);

                    SqlDataReader reader = null;
                    try
                    {
                        reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            DB_BudgetItem current = new DB_BudgetItem();

                            current.Id = Convert.ToInt64(reader["Id"]);
                            current.Name = Convert.ToString(reader["name"]);
                            current.UniqueId = Convert.ToString(reader["unique_id"]);
                            current.ParentUID = Convert.ToString(reader["parent_uid"]);
                            current.SubIndex = Convert.ToInt32(reader["sub_index"]);
                            current.Description = Convert.ToString(reader["description"]);
                            current.Active = Convert.ToBoolean(reader["active"]);
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

                public List<DB_Transaction> GetTransactionList(long accID, TimeSpan timeSpan)
                {
                    List<DB_Transaction> result = new List<DB_Transaction>();

                    string cmdStr = " SELECT * FROM Transactions " +
                                    " WHERE account_Id = @accID AND date_time > @datetime;";

                    SqlCommand command = new SqlCommand(cmdStr, SQLraw);
                    command.Parameters.AddWithValue("@accID", accID);
                    command.Parameters.AddWithValue("@datetime", DateTime.UtcNow - timeSpan);

                    SqlDataReader reader = null;
                    try
                    {
                        reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            DB_Transaction current = new DB_Transaction();

                            current.Id = Convert.ToInt64(reader["id"]);
                            current.Account_Id = Convert.ToInt64(reader["account_Id"]);
                            current.TransactionDateTime = Convert.ToDateTime(reader["date_time"]);
                            current.Description = Convert.ToString(reader["description"]);
                            current.Amount = Convert.ToDouble(reader["amount"]);
                            current.Balance = Convert.ToDouble(reader["balance"]);
                            current.Type = Convert.ToString(reader["type"]);
                            current.Tags = Convert.ToString(reader["tags"]);
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

                public List<DB_Transaction> GetTransactionDateList(long accID, DateTime startDate, DateTime endDate)
                {
                    List<DB_Transaction> result = new List<DB_Transaction>();

                    string cmdStr = " SELECT * FROM Transactions " +
                                    " WHERE account_Id = @accID AND date_time > @startDate AND date_time < @endDate;";

                    SqlCommand command = new SqlCommand(cmdStr, SQLraw);
                    command.Parameters.AddWithValue("@accID", accID);
                    command.Parameters.AddWithValue("@startDate", startDate);
                    command.Parameters.AddWithValue("@endDate", endDate);

                    SqlDataReader reader = null;
                    try
                    {
                        reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            DB_Transaction current = new DB_Transaction();

                            current.Id = Convert.ToInt64(reader["id"]);
                            current.Account_Id = Convert.ToInt64(reader["account_Id"]);
                            current.TransactionDateTime = Convert.ToDateTime(reader["date_time"]);
                            current.Description = Convert.ToString(reader["description"]);
                            current.Amount = Convert.ToDouble(reader["amount"]);
                            current.Balance = Convert.ToDouble(reader["balance"]);
                            current.Type = Convert.ToString(reader["type"]);
                            current.Tags = Convert.ToString(reader["tags"]);
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

                public List<DB_User> GetUserList()
                {
                    List<DB_User> result = new List<DB_User>();

                    string cmdStr = " SELECT * FROM Users; ";

                    SqlCommand command = new SqlCommand(cmdStr, SQLraw);

                    SqlDataReader reader = null;
                    try
                    {
                        reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            DB_User current = new DB_User();

                            current.Id = Convert.ToInt64(reader["id"]);
                            current.Username = Convert.ToString(reader["username"]);
                            current.Name = Convert.ToString(reader["name"]);
                            current.Email = Convert.ToString(reader["email"]);
                            current.ContactNo = Convert.ToString(reader["contact_no"]);
                            current.Address = Convert.ToString(reader["address"]);
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

                public List<DB_UserInstitution> GetUserInstitutionList(string username)
                {
                    List<DB_UserInstitution> result = new List<DB_UserInstitution>();

                    string cmdStr = " SELECT * FROM UserInstitution " +
                                    " WHERE username = @username; ";

                    SqlCommand command = new SqlCommand(cmdStr, SQLraw);
                    command.Parameters.AddWithValue("@username", username);

                    SqlDataReader reader = null;
                    try
                    {
                        reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            DB_UserInstitution current = new DB_UserInstitution();

                            current.Id = Convert.ToInt64(reader["id"]);
                            current.Username = Convert.ToString(reader["username"]);
                            current.InstitutionSlug = Convert.ToString(reader["institution_slug"]);
                            current.UserAddress = Convert.ToString(reader["user_address"]);
                            current.bfCustomerID = Convert.ToString(reader["bf_customerId"]);
                            current.bfEncryptionKey = Convert.ToString(reader["bf_encryptionKey"]);
                            current.WithMfa = Convert.ToBoolean(reader["with_mfa"]);

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

                public List<DB_Account> GetAccountList(string username)
                {
                    List<DB_Account> result = new List<DB_Account>();

                    string cmdStr = " SELECT * FROM Accounts " +
                                    " WHERE username = @username ;";

                    SqlCommand command = new SqlCommand(cmdStr, SQLraw);
                    command.Parameters.AddWithValue("@username", username);

                    SqlDataReader reader = null;
                    try
                    {
                        reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            DB_Account current = new DB_Account();

                            current.Id = Convert.ToInt64(reader["id"]);
                            current.Username = Convert.ToString(reader["username"]);
                            current.Institution = Convert.ToString(reader["institution_slug"]);
                            current.AccIdBF = Convert.ToInt32(reader["bf_id"]);
                            current.BankName = Convert.ToString(reader["bank_name"]);
                            current.AccountType = Convert.ToString(reader["account_type"]);
                            current.AccountHolder = Convert.ToString(reader["account_holder"]);
                            current.AccountNumber = Convert.ToString(reader["account_number"]);
                            current.CurrentBalance = (float)Convert.ToDouble(reader["current_balance"]);
                            current.AvailableBalance = (float)Convert.ToDouble(reader["available_balance"]);
                            current.BSB = Convert.ToString(reader["bsb"]);

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

                public List<DB_Account> GetAccountListInstitution(string username, string slug)
                {
                    List<DB_Account> result = new List<DB_Account>();

                    string cmdStr = " SELECT * FROM Accounts " +
                                    " WHERE username = @username AND institution_slug = @slug;";

                    SqlCommand command = new SqlCommand(cmdStr, SQLraw);
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@slug", slug);

                    SqlDataReader reader = null;
                    try
                    {
                        reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            DB_Account current = new DB_Account();

                            current.Id = Convert.ToInt64(reader["id"]);
                            current.Username = Convert.ToString(reader["username"]);
                            current.Institution = Convert.ToString(reader["institution_slug"]);
                            current.AccIdBF = Convert.ToInt32(reader["bf_id"]);
                            current.BankName = Convert.ToString(reader["bank_name"]);
                            current.AccountType = Convert.ToString(reader["account_type"]);
                            current.AccountHolder = Convert.ToString(reader["account_holder"]);
                            current.AccountNumber = Convert.ToString(reader["account_number"]);
                            current.CurrentBalance = (float)Convert.ToDouble(reader["current_balance"]);
                            current.AvailableBalance = (float)Convert.ToDouble(reader["available_balance"]);
                            current.BSB = Convert.ToString(reader["bsb"]);

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

                public List<DB_RuleItem> GetFullRuleItemList()
                {
                    List<DB_RuleItem> result = new List<DB_RuleItem>();

                    string cmdStr = " SELECT * FROM RuleItem ";

                    SqlCommand command = new SqlCommand(cmdStr, SQLraw);

                    SqlDataReader reader = null;
                    try
                    {
                        reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            DB_RuleItem current = new DB_RuleItem();

                            current.Id = Convert.ToInt64(reader["id"]);
                            current.Name = Convert.ToString(reader["name"]);
                            current.Reference = Convert.ToString(reader["reference"]);
                            current.BudgetReference = Convert.ToString(reader["budget_reference"]);

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

                public List<DB_RuleMatch> GetFullRuleMatchList()
                {
                    List<DB_RuleMatch> result = new List<DB_RuleMatch>();

                    string cmdStr = " SELECT * FROM RuleMatch ";

                    SqlCommand command = new SqlCommand(cmdStr, SQLraw);

                    SqlDataReader reader = null;
                    try
                    {
                        reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            DB_RuleMatch current = new DB_RuleMatch();

                            current.Id = Convert.ToInt64(reader["id"]);
                            current.MatchID = Convert.ToString(reader["match_id"]);
                            current.RuleReference = Convert.ToString(reader["rule_reference"]);
                            current.MatchItem = Convert.ToString(reader["match_item"]);
                            current.MatchType = Convert.ToString(reader["match_type"]);
                            current.MatchText = Convert.ToString(reader["match"]);

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

                public string DeleteReportSQL(string reportReference)
                {
                    string cmdStr = " DELETE * FROM ReportTransaction " +
                                    " JOIN ReportItem ON ReportTransaction.report_item_id = ReportItem.id " +
                                    " JOIN Reports ON ReportItem.report_reference = Reports.reference " +
                                    " WHERE Reports.reference = @reference ;" +

                                    " DELETE * FROM ReportItem " +
                                    " WHERE budget_reference = @reference ;" +

                                    " DELETE * FROM Reports " +
                                    " WHERE reference = @reference ;";

                    SqlCommand command = new SqlCommand(cmdStr, SQLraw);
                    command.Parameters.AddWithValue("@reference", reportReference);

                    try { command.ExecuteNonQuery(); }
                    catch (Exception e)
                    {
                        Supporting.WriteLog(e.ToString());
                        return "error: delete report failed";
                    }

                    return "info: success";
                }

                public string DeleteAccountSQL(DB_Account acc)
                {
                    string cmdStr = " DELETE FROM Transactions " +
                                    " WHERE Id = @acc_id;" +
                                    " DELETE FROM Accounts " +
                                    " WHERE username = @username AND institution_slug = @slug AND account_number = @acc_no;";

                    SqlCommand command = new SqlCommand(cmdStr, SQLraw);
                    command.Parameters.AddWithValue("@username", acc.Username);
                    command.Parameters.AddWithValue("@slug", acc.Institution);
                    command.Parameters.AddWithValue("@acc_no", acc.AccountNumber);
                    command.Parameters.AddWithValue("@acc_id", acc.Id);

                    try { command.ExecuteNonQuery(); }
                    catch (Exception e)
                    {
                        Supporting.WriteLog(e.ToString());
                        return "error: delete failed";
                    }

                    return "info: success";
                }

                public string DeleteBudgetItem(string uniqueID)
                {
                    string cmdStr = " DELETE FROM BudgetItem " +
                                    " WHERE unique_id = @unique_id;";

                    SqlCommand command = new SqlCommand(cmdStr, SQLraw);
                    command.Parameters.AddWithValue("@unique_id", uniqueID);

                    try { command.ExecuteNonQuery(); }
                    catch (Exception e)
                    {
                        Supporting.WriteLog(e.ToString());
                        return "error: delete failed";
                    }

                    return "info: success";
                }

                public string DeleteRuleItem(string reference)
                {
                    string cmdStr = " DELETE FROM RuleMatch " +
                                    " WHERE rule_reference = @reference;" +
                                    " DELETE FROM RuleItem " +
                                    " WHERE reference = @reference;";

                    SqlCommand command = new SqlCommand(cmdStr, SQLraw);
                    command.Parameters.AddWithValue("@reference", reference);

                    try { command.ExecuteNonQuery(); }
                    catch (Exception e)
                    {
                        Supporting.WriteLog(e.ToString());
                        return "error: delete failed";
                    }

                    return "info: success";
                }

                public string DeleteRuleMatch(string match_id, string reference)
                {
                    string cmdStr = " DELETE FROM RuleMatch " +
                                    " WHERE match_id = @match_id AND rule_reference = @reference;";

                    SqlCommand command = new SqlCommand(cmdStr, SQLraw);
                    command.Parameters.AddWithValue("@match_id", match_id);
                    command.Parameters.AddWithValue("@reference", reference);

                    try { command.ExecuteNonQuery(); }
                    catch (Exception e)
                    {
                        Supporting.WriteLog(e.ToString());
                        return "error: delete failed";
                    }

                    return "info: success";
                }
                #endregion

                /*

                #region Consistency Deletes

                public string ConsistencyDeleteEssbaseSQL(string server_alias)
                {
                    string cmdStr = " DELETE " +
                                   " FROM T1 " +
                                   " FROM Assignment AS T1 " +
                                   " JOIN Import_mappings ON Import_mappings.IMP_alias = T1.IMP_alias " +
                                   " WHERE Import_mappings.ESS_server_alias = @server_alias ";

                    SqlCommand command = new SqlCommand(cmdStr, SQLraw);
                    command.Parameters.AddWithValue("@server_alias", server_alias);

                    try { command.ExecuteNonQuery(); }
                    catch { return "error during consistency cleanup"; }

                    cmdStr = " DELETE FROM Import_mappings " +
                             " WHERE ESS_server_alias = @server_alias ;";

                    command = new SqlCommand(cmdStr, SQLraw);
                    command.Parameters.AddWithValue("@server_alias", server_alias);

                    try { command.ExecuteNonQuery(); }
                    catch { return "error during consistency cleanup"; }

                    return "info: success";
                }

                public string ConsistencyDeleteDBSQL(string DB_alias)
                {
                    string cmdStr = " DELETE " +
                                   " FROM T1 " +
                                   " FROM Assignment AS T1 " +
                                   " JOIN Import_mappings ON Import_mappings.IMP_alias = T1.IMP_alias " +
                                   " WHERE Import_mappings.ESS_DB_alias = @DB_alias; ";

                    cmdStr += " DELETE FROM Import_mappings " +
                              " WHERE ESS_DB_alias = @DB_alias ;";

                    SqlCommand command = new SqlCommand(cmdStr, SQLraw);
                    command.Parameters.AddWithValue("@DB_alias", DB_alias);

                    try { command.ExecuteNonQuery(); }
                    catch { return "error during consistency cleanup"; }

                    return "info: success";
                }

                public string ConsistencyDeleteSQLSQL(string SQL_alias)
                {
                    string cmdStr = " DELETE " +
                                   " FROM T1 " +
                                   " FROM Assignment AS T1 " +
                                   " JOIN Import_mappings ON Import_mappings.IMP_alias = T1.IMP_alias " +
                                   " WHERE Import_mappings.SQL_server_alias = @SQL_alias; ";

                    cmdStr += " DELETE FROM Import_mappings " +
                              " WHERE SQL_server_alias = @SQL_alias ;";

                    SqlCommand command = new SqlCommand(cmdStr, SQLraw);
                    command.Parameters.AddWithValue("@SQL_alias", SQL_alias);

                    try { command.ExecuteNonQuery(); }
                    catch { }

                    return "info: success";
                }
                #endregion


                public Maxl.ImportCommand GetImportCommand(string imp_alias)
                {
                    Maxl.ImportCommand result = new Maxl.ImportCommand();

                    string cmdStr = "SELECT Import_mappings.*, Essbase_servers.*, Essbase_DBs.*, SQL_servers.* " +
                                    "FROM Import_mappings " +
                                    "JOIN Essbase_servers ON Essbase_servers.ESS_alias = Import_mappings.ESS_server_alias " +
                                    "JOIN Essbase_DBs ON Essbase_DBs.DB_alias = Import_mappings.ESS_DB_alias " +
                                    "JOIN SQL_servers ON SQL_servers.SQL_alias = Import_mappings.SQL_server_alias " +
                                    "WHERE Import_mappings.IMP_alias = @imp_alias";

                    SqlCommand command = new SqlCommand(cmdStr, SQLraw);
                    command.Parameters.AddWithValue("@imp_alias", imp_alias);

                    SqlDataReader reader = null;
                    try
                    {
                        reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            result.ESS_server = Convert.ToString(reader["ESS_server"]);
                            result.ESS_user = Convert.ToString(reader["ESS_user"]);
                            result.ESS_pass = Convert.ToString(reader["ESS_pass"]);
                            result.ESS_APP = Convert.ToString(reader["ESS_application"]);
                            result.ESS_DB = Convert.ToString(reader["ESS_database"]);

                            result.SQL_user = Convert.ToString(reader["SQL_user"]);
                            result.SQL_pass = Convert.ToString(reader["SQL_pass"]);
                            result.SQL_rule = Convert.ToString(reader["SQL_rules"]);
                            result.IMP_type = Convert.ToString(reader["IMP_type"]);
                            result.IMP_alias = imp_alias;
                        }
                        reader.Close();
                    }
                    catch (Exception e)
                    { return new Maxl.ImportCommand(); }

                    return result;
                }

                public List<DB_ClientImport> GetClientImportSQL(string username)
                {
                    List<DB_ClientImport> result = new List<DB_ClientImport>();

                    string cmdStr = " SELECT Import_mappings.*, Users.BusinessUnit " +
                                    " FROM Import_mappings " +
                                    " JOIN Assignment ON Assignment.IMP_alias = Import_mappings.IMP_alias " +
                                    " JOIN Users ON Assignment.UserName = Users.UserName " +
                                    " WHERE Assignment.UserName = @username; ";

                    SqlCommand command = new SqlCommand(cmdStr, SQLraw);
                    command.Parameters.AddWithValue("@username", username);

                    SqlDataReader reader = null;
                    try
                    {
                        reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            DB_ClientImport current = new DB_ClientImport();

                            current.Business_unit = Convert.ToString(reader["BusinessUnit"]);
                            current.Import_name = Convert.ToString(reader["IMP_alias"]);
                            current.ESS_server = Convert.ToString(reader["ESS_server_alias"]);
                            current.ESS_db = Convert.ToString(reader["ESS_DB_alias"]);
                            current.SQL_server = Convert.ToString(reader["SQL_server_alias"]);
                            current.SQL_rule = Convert.ToString(reader["SQL_rules"]);
                            current.Import_type = Convert.ToString(reader["IMP_type"]);

                            result.Add(current);
                        }
                        reader.Close();
                    }
                    catch (Exception e)
                    { return null; }

                    return result;
                }

                */
    }

}
