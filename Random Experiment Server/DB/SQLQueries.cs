using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.IO;

namespace Random_Experiment_Server.DB
{
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
/*
        #region Add
        public string AddInstitution(DB_Institution institution)
        {
            if (InstitutionExists(institution.Slug))
                return "error: institution exists";
            else
                return InsertInstitution(institution);
        }

        public string AddUser(DB_User user)
        {
            if (UserExists(user.Username))
                return "error: user exists";
            else
                return InsertUser(user);
        }

        public string AddAccount(DB_Account acc)
        {
            if (AccountExists(acc.Institution, acc.AccountNumber))
                return "error: account exists";
            else
                return InsertAccount(acc);
        }

        public string AddUserInstitution(DB_UserInstitution usrInst)
        {
            if (UserInstitutionExists(usrInst.Username, usrInst.InstitutionSlug))
                return "error: account exists";
            else
                return InsertUserInstitution(usrInst);
        }

        public string AddTransaction(DB_Transaction trans)
        {
            if (TransactionExists(trans.Account_Id, trans.TransactionDateTime, trans.Amount, trans.Balance))
                return "info: transaction exists";
            else
                return InsertTransaction(trans);
        }

        public string AddUpdateBudgetItem(DB_BudgetItem item)
        {
            if (BudgetItemExists(item.UniqueId))
                return UpdateBudgetItem(item);
            else
                return InsertBudgetItem(item);
        }

        public string AddUpdateRuleItem(DB_RuleItem item)
        {
            if (RuleItemExists(item.Reference))
                return UpdateRuleItem(item);
            else
                return InsertRuleItem(item);
        }

        public string AddUpdateRuleMatch(DB_RuleMatch item)
        {
            if (!RuleItemExists(item.RuleReference))
                return "error: parent rule doesn't exist";
            else if (RuleMatchExists(item.MatchID, item.RuleReference))
                return UpdateRuleMatch(item);
            else
                return InsertRuleMatch(item);
        }

        public string AddReport(DB_Report item)
        {
            if (ReportExists(item.Reference))
                return "info: report exists";
            else
                return InsertReport(item);
        }

        public string AddUpdateReportItem(DB_ReportItem item)
        {
            if (ReportItemExists(item.ReportReference, item.BudgetReference))
                return UpdateReportItem(item);
            else
                return InsertReportItem(item);
        }

        public string AddReportTransaction(DB_ReportTransaction item)
        {
            if (ReportTransactionExists(item.TransactionId, item.ReportItemId))
                return "success";
            else
                return InsertReportTransaction(item);
        }
        #endregion

        #region Exists
        public bool InstitutionExists(string slug)
        {
            int result = 0;

            slug = slug.ToLower();

            string cmdStr = "SELECT COUNT(*) AS count FROM Institutions " +
                            "WHERE LOWER(slug) = @slug ;";

            SqlCommand command = new SqlCommand(cmdStr, SQLraw);
            command.Parameters.AddWithValue("@slug", slug);

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

        public bool ReportExists(string reference)
        {
            int result = 0;

            reference = reference.ToLower();

            string cmdStr = "SELECT COUNT(*) AS count FROM Reports " +
                            "WHERE LOWER(reference) = @reference ;";

            SqlCommand command = new SqlCommand(cmdStr, SQLraw);
            command.Parameters.AddWithValue("@reference", reference);

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

        public bool ReportItemExists(string reportReference, string budgetReference)
        {
            int result = 0;

            reportReference = reportReference.ToLower();
            budgetReference = budgetReference.ToLower();

            string cmdStr = "SELECT COUNT(*) AS count FROM ReportItem " +
                            "WHERE LOWER(report_reference) = @reportReference AND LOWER(budget_reference) = @budgetReference ;";

            SqlCommand command = new SqlCommand(cmdStr, SQLraw);
            command.Parameters.AddWithValue("@reportReference", reportReference);
            command.Parameters.AddWithValue("@budgetReference", budgetReference);

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

        public bool ReportTransactionExists(long transID, long reportID)
        {
            int result = 0;

            string cmdStr = "SELECT COUNT(*) AS count FROM ReportTransaction " +
                            "WHERE transaction_id = @transID AND report_item_id = @reportID;";

            SqlCommand command = new SqlCommand(cmdStr, SQLraw);
            command.Parameters.AddWithValue("@transID", transID);
            command.Parameters.AddWithValue("@reportID", reportID);

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

        public bool TransactionExists(long accId, DateTime dateTime, double amount, double balance)
        {
            int result = 0;
            string cmdStr = "SELECT COUNT(*) AS count FROM Transactions " +
                            "WHERE Account_Id = @accId AND date_time = @datetime AND amount = @amount AND balance = @balance;";

            SqlCommand command = new SqlCommand(cmdStr, SQLraw);
            command.Parameters.AddWithValue("@accId", accId);
            command.Parameters.AddWithValue("@datetime", dateTime);
            command.Parameters.AddWithValue("@amount", amount);
            command.Parameters.AddWithValue("@balance", balance);

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

        public bool UserExists(string username)
        {
            if (username == null) return false;

            int result = 0;

            username = username.ToLower();

            string cmdStr = "SELECT COUNT(*) AS count FROM Users " +
                            "WHERE LOWER(username) = @username ;";

            SqlCommand command = new SqlCommand(cmdStr, SQLraw);
            command.Parameters.AddWithValue("@username", username);

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

        public bool AccountExists(string slug, string accNo)
        {
            int result = 0;

            //instSlug = instSlug.ToLower();
            accNo = accNo.ToLower();

            string cmdStr = " SELECT COUNT(*) AS count FROM Accounts " +
                            " WHERE LOWER(account_number) = @acc AND LOWER(institution_slug) = @slug; ";

            SqlCommand command = new SqlCommand(cmdStr, SQLraw);
            command.Parameters.AddWithValue("@slug", slug);
            command.Parameters.AddWithValue("@acc", accNo);

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

        public bool UserInstitutionExists(string username, string slug)
        {
            int result = 0;

            username = username.ToLower();
            slug = slug.ToLower();

            string cmdStr = " SELECT COUNT(*) AS count FROM UserInstitution " +
                            " WHERE LOWER(username) = @username AND LOWER(institution_slug) = @slug;";

            SqlCommand command = new SqlCommand(cmdStr, SQLraw);
            command.Parameters.AddWithValue("@slug", slug);
            command.Parameters.AddWithValue("@username", username);

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

        public bool BudgetItemExists(string uniqueID)
        {
            if (uniqueID == null) return false;

            int result = 0;

            uniqueID = uniqueID.ToLower();

            string cmdStr = " SELECT COUNT(*) AS count FROM BudgetItem " +
                            " WHERE LOWER(unique_id) = @UniqueID;";

            SqlCommand command = new SqlCommand(cmdStr, SQLraw);
            command.Parameters.AddWithValue("@UniqueID", uniqueID);

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

        public bool RuleItemExists(string reference)
        {
            if (reference == null) return false;

            int result = 0;

            reference = reference.ToLower();

            string cmdStr = " SELECT COUNT(*) AS count FROM RuleItem " +
                            " WHERE LOWER(reference) = @reference;";

            SqlCommand command = new SqlCommand(cmdStr, SQLraw);
            command.Parameters.AddWithValue("@reference", reference);

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

        public bool RuleMatchExists(string match_id, string reference)
        {
            if (reference == null || match_id == null) return false;

            int result = 0;

            reference = reference.ToLower();
            match_id = match_id.ToLower();

            string cmdStr = " SELECT COUNT(*) AS count FROM RuleMatch " +
                            " WHERE LOWER(rule_reference) = @rule_reference AND LOWER(match_id) = @match_id;";

            SqlCommand command = new SqlCommand(cmdStr, SQLraw);
            command.Parameters.AddWithValue("@rule_reference", reference);
            command.Parameters.AddWithValue("@match_id", match_id);

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
        private string InsertInstitution(DB_Institution institution)
        {
            string cmdStr = " INSERT INTO Institutions (slug,name,max_days,updated_at,searchVal)" +
                            " VALUES (@slug , @name , @max_days, @updated_at, @searchVal);";

            SqlCommand command = new SqlCommand(cmdStr, SQLraw);
            command.Parameters.AddWithValue("@slug", institution.Slug);
            command.Parameters.AddWithValue("@name", institution.Name);
            command.Parameters.AddWithValue("@max_days", institution.MaxDays);
            command.Parameters.AddWithValue("@updated_at", institution.UpdatedAt);
            command.Parameters.AddWithValue("@searchVal", institution.SearchValue);

            try { command.ExecuteNonQuery(); }
            catch (Exception e)
            {
                Supporting.WriteLog(e.Message);
                return "error adding institution";
            }

            return "info: created institution";
        }

        private string InsertReport(DB_Report report)
        {
            string cmdStr = " INSERT INTO Reports (reference,username,start_date,end_date) " +
                            " VALUES (@reference , @username , @start_date, @end_date) ; ";

            SqlCommand command = new SqlCommand(cmdStr, SQLraw);
            command.Parameters.AddWithValue("@reference", report.Reference);
            command.Parameters.AddWithValue("@username", report.Username);
            command.Parameters.AddWithValue("@start_date", report.StartDate);
            command.Parameters.AddWithValue("@end_date", report.EndDate);

            try { command.ExecuteNonQuery(); }
            catch (Exception e)
            {
                Supporting.WriteLog(e.Message);
                return "error adding report";
            }

            return "info: created report";
        }

        private string InsertReportItem(DB_ReportItem item)
        {
            string cmdStr = " INSERT INTO ReportItem (report_reference,budget_reference,amount) " +
                            " VALUES (@report_reference , @budget_reference , @amount) ; ";

            //SqlTransaction transaction;
            //transaction = SQLraw.BeginTransaction("InsertReportItem");

            SqlCommand command = new SqlCommand(cmdStr, SQLraw);
            command.Parameters.AddWithValue("@report_reference", item.ReportReference);
            command.Parameters.AddWithValue("@budget_reference", item.BudgetReference);
            command.Parameters.AddWithValue("@amount", item.Amount);
            //command.Transaction = transaction;

            try { command.ExecuteNonQuery(); }
            catch (Exception e)
            {
                Supporting.WriteLog(e.Message);
                return "error adding report item";
            }

            //transaction.Commit();

            return "info: created report item";
        }

        private string InsertReportTransaction(DB_ReportTransaction item)
        {
            string cmdStr = " INSERT INTO ReportTransaction (transaction_id,report_item_id) " +
                            " VALUES (@transaction_id , @report_item_id ) ; ";

            SqlCommand command = new SqlCommand(cmdStr, SQLraw);
            command.Parameters.AddWithValue("@transaction_id", item.TransactionId);
            command.Parameters.AddWithValue("@report_item_id", item.ReportItemId);

            try { command.ExecuteNonQuery(); }
            catch (Exception e)
            {
                Supporting.WriteLog(e.Message);
                return "error adding report transaction";
            }

            return "info: created report transaction";
        }

        private string InsertTransaction(DB_Transaction trans)
        {
            string cmdStr = " INSERT INTO Transactions (account_Id,date_time,description,amount,balance,type,tags)" +
                            " VALUES (@accId , @datetime , @description, @amount, @balance, @type, @tags);";

            SqlCommand command = new SqlCommand(cmdStr, SQLraw);
            command.Parameters.AddWithValue("@accId", trans.Account_Id);
            command.Parameters.AddWithValue("@datetime", trans.TransactionDateTime);
            command.Parameters.AddWithValue("@description", trans.Description);
            command.Parameters.AddWithValue("@amount", trans.Amount);
            command.Parameters.AddWithValue("@balance", trans.Balance);
            command.Parameters.AddWithValue("@type", trans.Type ?? "");
            command.Parameters.AddWithValue("@tags", trans.Tags ?? "");

            try { command.ExecuteNonQuery(); }
            catch (Exception e)
            {
                Supporting.WriteLog(e.Message);
                return "error adding transaction";
            }

            return "info: created transaction";
        }

        private string InsertUser(DB_User user)
        {
            string cmdStr = " INSERT INTO Users (username, name, email, contact_no, address)" +
                            " VALUES (@username , @name , @email , @contact_no , @address);";

            SqlCommand command = new SqlCommand(cmdStr, SQLraw);
            command.Parameters.AddWithValue("@username", user.Username);
            command.Parameters.AddWithValue("@name", user.Name);
            command.Parameters.AddWithValue("@email", user.Email);
            command.Parameters.AddWithValue("@contact_no", user.ContactNo);
            command.Parameters.AddWithValue("@address", user.Address ?? "");

            try { command.ExecuteNonQuery(); }
            catch (Exception e)
            {
                Supporting.WriteLog(e.Message);
                return "error adding user";
            }

            return "info: created user";
        }

        private string InsertUserInstitution(DB_UserInstitution userInst)
        {
            string cmdStr = " INSERT INTO UserInstitution (username, institution_slug, user_address, bf_customerId, bf_encryptionKey, with_mfa)" +
                            " VALUES (@username, @institution_slug, @address, @bf_customerId, @bf_encryptionKey, @with_mfa);";

            SqlCommand command = new SqlCommand(cmdStr, SQLraw);
            command.Parameters.AddWithValue("@username", userInst.Username);
            command.Parameters.AddWithValue("@institution_slug", userInst.InstitutionSlug);
            command.Parameters.AddWithValue("@address", userInst.UserAddress ?? "");
            command.Parameters.AddWithValue("@bf_customerId", userInst.bfCustomerID ?? "");
            command.Parameters.AddWithValue("@bf_encryptionKey", userInst.bfEncryptionKey ?? "");
            command.Parameters.AddWithValue("@with_mfa", userInst.WithMfa);

            try { command.ExecuteNonQuery(); }
            catch (Exception e)
            {
                Supporting.WriteLog(e.Message);
                return "error adding user institution";
            }

            return "info: created user institution";
        }

        private string InsertAccount(DB_Account acc)
        {
            if (acc.AccountHolder == null) acc.AccountHolder = "unspecified";

            string cmdStr = " INSERT INTO Accounts (username,institution_slug,bank_name,bf_id,account_type,account_holder,account_number,current_balance,available_balance,bsb)" +
                            " VALUES (@username , @institution_slug , @bank_name , @bf_id, @account_type , @account_holder, @account_number, @current_balance, @available_balance, @bsb);";

            SqlCommand command = new SqlCommand(cmdStr, SQLraw);
            command.Parameters.AddWithValue("@username", acc.Username);
            command.Parameters.AddWithValue("@institution_slug", acc.Institution);
            command.Parameters.AddWithValue("@bank_name", acc.BankName);
            command.Parameters.AddWithValue("@bf_id", acc.AccIdBF);
            command.Parameters.AddWithValue("@account_type", acc.AccountType);
            command.Parameters.AddWithValue("@account_holder", acc.AccountHolder);
            command.Parameters.AddWithValue("@account_number", acc.AccountNumber);
            command.Parameters.AddWithValue("@current_balance", acc.CurrentBalance);
            command.Parameters.AddWithValue("@available_balance", acc.AvailableBalance);
            command.Parameters.AddWithValue("@bsb", acc.BSB ?? "");

            try { command.ExecuteNonQuery(); }
            catch (Exception e)
            {
                Supporting.WriteLog(e.Message);
                return "error adding user";
            }

            return "info: created user";
        }

        private string InsertBudgetItem(DB_BudgetItem item)
        {
            string cmdStr = " INSERT INTO BudgetItem (unique_id, parent_uid, name, description, sub_index, active)" +
                            " VALUES (@unique_id, @parent_uid, @name, @description, @sub_index, @active);";

            SqlCommand command = new SqlCommand(cmdStr, SQLraw);
            command.Parameters.AddWithValue("@unique_id", item.UniqueId);
            command.Parameters.AddWithValue("@parent_uid", item.ParentUID ?? "");
            command.Parameters.AddWithValue("@name", item.Name ?? "");
            command.Parameters.AddWithValue("@description", item.Description ?? "");
            command.Parameters.AddWithValue("@sub_index", item.SubIndex);
            command.Parameters.AddWithValue("@active", item.Active);

            try { command.ExecuteNonQuery(); }
            catch (Exception e)
            {
                Supporting.WriteLog(e.Message);
                return "error adding user institution";
            }

            return "info: created user institution";
        }

        private string InsertRuleItem(DB_RuleItem item)
        {
            string cmdStr = " INSERT INTO RuleItem (name, reference, budget_reference)" +
                            " VALUES (@name, @reference, @budget_reference);";

            SqlCommand command = new SqlCommand(cmdStr, SQLraw);
            command.Parameters.AddWithValue("@name", item.Name ?? "");
            command.Parameters.AddWithValue("@reference", item.Reference);
            command.Parameters.AddWithValue("@budget_reference", item.BudgetReference ?? "");

            try { command.ExecuteNonQuery(); }
            catch (Exception e)
            {
                Supporting.WriteLog(e.Message);
                return "error adding rule item";
            }

            return "info: created rule item";
        }

        private string InsertRuleMatch(DB_RuleMatch item)
        {
            string cmdStr = " INSERT INTO RuleMatch (match_id, rule_reference, match_item, match_type, match)" +
                            " VALUES (@match_id, @rule_reference, @match_item, @match_type, @match);";

            SqlCommand command = new SqlCommand(cmdStr, SQLraw);
            command.Parameters.AddWithValue("@match_id", item.MatchID);
            command.Parameters.AddWithValue("@rule_reference", item.RuleReference);
            command.Parameters.AddWithValue("@match_item", item.MatchItem ?? "");
            command.Parameters.AddWithValue("@match_type", item.MatchType ?? "");
            command.Parameters.AddWithValue("@match", item.MatchText ?? "");

            try { command.ExecuteNonQuery(); }
            catch (Exception e)
            {
                Supporting.WriteLog(e.Message);
                return "error adding rule match";
            }

            return "info: created rule match";
        }
        #endregion

        #region Get
        public DB_Institution GetInstitution(string slug)
        {
            DB_Institution result = new DB_Institution();

            string cmdStr = " SELECT * FROM Institutions " +
                            " WHERE slug = @slug ;";

            SqlCommand command = new SqlCommand(cmdStr, SQLraw);
            command.Parameters.AddWithValue("@slug", slug);

            SqlDataReader reader = null;
            try
            {
                DB_Institution current = new DB_Institution();

                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    current.Id = Convert.ToInt64(reader["id"]);
                    current.Slug = Convert.ToString(reader["slug"]);
                    current.Name = Convert.ToString(reader["name"]);
                    current.MaxDays = Convert.ToInt64(reader["max_days"]);
                    current.UpdatedAt = Convert.ToDateTime(reader["updated_at"]);
                    current.SearchValue = Convert.ToString(reader["searchVal"]);
                }
                reader.Close();
                return current;
            }
            catch (Exception e)
            {
                Supporting.WriteLog(e.Message);
                return null;
            }
        }

        public DB_ReportItem GetReportItem(string reportReference, string budgetReference)
        {
            string cmdStr = " SELECT * FROM ReportItem " +
                            " WHERE report_reference = @reportReference AND budget_reference = @budgetReference;";

            SqlCommand command = new SqlCommand(cmdStr, SQLraw);
            command.Parameters.AddWithValue("@reportReference", reportReference);
            command.Parameters.AddWithValue("@budgetReference", budgetReference);

            SqlDataReader reader = null;
            try
            {
                DB_ReportItem current = null;
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    current = new DB_ReportItem();
                    current.Id = Convert.ToInt64(reader["id"]);
                    current.ReportReference = Convert.ToString(reader["report_reference"]);
                    current.BudgetReference = Convert.ToString(reader["budget_reference"]);
                    current.Amount = Convert.ToDouble(reader["amount"]);
                }
                reader.Close();
                return current;
            }
            catch (Exception e)
            {
                Supporting.WriteLog(e.Message);
                return null;
            }
        }

        public DB_User GetUser(string username)
        {
            List<DB_User> result = new List<DB_User>();

            string cmdStr = " SELECT * FROM Users " +
                            " WHERE username = @username ;";

            SqlCommand command = new SqlCommand(cmdStr, SQLraw);
            command.Parameters.AddWithValue("@username", username);

            SqlDataReader reader = null;
            try
            {
                DB_User current = new DB_User();
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    current.Id = Convert.ToInt64(reader["id"]);
                    current.Username = Convert.ToString(reader["username"]);
                    current.Name = Convert.ToString(reader["name"]);
                    current.Email = Convert.ToString(reader["email"]);
                    current.ContactNo = Convert.ToString(reader["contact_no"]);
                    current.Address = Convert.ToString(reader["address"]);
                }
                reader.Close();
                return current;
            }
            catch (Exception e)
            {
                Supporting.WriteLog(e.Message);
                return null;
            }
        }
        public DB_UserInstitution GetUserInstitution(string username, string slug)
        {
            string cmdStr = " SELECT * FROM UserInstitution " +
                            " WHERE username = @username AND institution_slug = @slug ;";

            SqlCommand command = new SqlCommand(cmdStr, SQLraw);
            command.Parameters.AddWithValue("@username", username);
            command.Parameters.AddWithValue("@slug", slug);

            SqlDataReader reader = null;
            try
            {
                DB_UserInstitution current = new DB_UserInstitution();
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    current.Id = Convert.ToInt64(reader["id"]);
                    current.Username = Convert.ToString(reader["username"]);
                    current.InstitutionSlug = Convert.ToString(reader["institution_slug"]);
                    current.UserAddress = Convert.ToString(reader["user_address"]);
                    current.bfCustomerID = Convert.ToString(reader["bf_customerId"]);
                    current.bfEncryptionKey = Convert.ToString(reader["bf_encryptionKey"]);
                    current.WithMfa = Convert.ToBoolean(reader["with_mfa"]);
                }
                reader.Close();
                return current;
            }
            catch (Exception e)
            {
                Supporting.WriteLog(e.Message);
                return null;
            }
        }

        public DB_Account GetAccount(string username, string accNo, string slug)
        {
            DB_Account result = new DB_Account();

            string cmdStr = " SELECT * FROM Accounts " +
                            " WHERE username = @username AND account_number = @acc_no AND institution_slug = @slug;";

            SqlCommand command = new SqlCommand(cmdStr, SQLraw);
            command.Parameters.AddWithValue("@username", username);
            command.Parameters.AddWithValue("@acc_no", accNo);
            command.Parameters.AddWithValue("@slug", slug);

            SqlDataReader reader = null;
            try
            {
                DB_Account current = new DB_Account();

                reader = command.ExecuteReader();
                while (reader.Read())
                {
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
                }
                reader.Close();
                return current;
            }
            catch (Exception e)
            {
                Supporting.WriteLog(e.Message);
                return null;
            }
        }

        public DB_BudgetItem GetBudgetItem(string uniqueID)
        {
            DB_BudgetItem result = new DB_BudgetItem();

            string cmdStr = " SELECT * FROM BudgetItem " +
                            " WHERE unique_id = @unique_id;";

            SqlCommand command = new SqlCommand(cmdStr, SQLraw);

            SqlDataReader reader = null;
            try
            {
                DB_BudgetItem current = new DB_BudgetItem();
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    current.Id = Convert.ToInt64(reader["Id"]);
                    current.Name = Convert.ToString(reader["name"]);
                    current.UniqueId = Convert.ToString(reader["unique_id"]);
                    current.ParentUID = Convert.ToString(reader["parent_uid"]);
                    current.SubIndex = Convert.ToInt32(reader["sub_index"]);
                    current.Description = Convert.ToString(reader["description"]);
                    current.Active = Convert.ToBoolean(reader["active"]);
                }
                reader.Close();
                return current;
            }
            catch (Exception e)
            {
                Supporting.WriteLog(e.Message);
                return null;
            }
        }


        #endregion

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

        #region Update
        public string UpdateUser(DB_User user)
        {
            string cmdStr = " UPDATE Users " +
                            " SET email = @email, name = @name, contact_no = @contact_no, address = @address " +
                            " WHERE username = @username ;";

            SqlCommand command = new SqlCommand(cmdStr, SQLraw);
            command.Parameters.AddWithValue("@username", user.Username);
            command.Parameters.AddWithValue("@name", user.Name);
            command.Parameters.AddWithValue("@email", user.Email);
            command.Parameters.AddWithValue("@contact_no", user.ContactNo);
            command.Parameters.AddWithValue("@address", user.Address);

            try { command.ExecuteNonQuery(); }
            catch (Exception e)
            {
                Supporting.WriteLog(e.ToString());
                return "error changing user details";
            }

            return "info: transaction successful";
        }

        public string UpdateReportItem(DB_ReportItem item)
        {
            string cmdStr = " UPDATE ReportItem " +
                            " SET amount = @amount " +
                            " WHERE report_reference = @report_reference AND budget_reference = @budget_reference;";

            SqlCommand command = new SqlCommand(cmdStr, SQLraw);
            command.Parameters.AddWithValue("@report_reference", item.ReportReference);
            command.Parameters.AddWithValue("@budget_reference", item.BudgetReference);
            command.Parameters.AddWithValue("@amount", item.Amount);

            try { command.ExecuteNonQuery(); }
            catch (Exception e)
            {
                Supporting.WriteLog(e.ToString());
                return "error changing report item";
            }

            return "info: successful report item change";
        }

        public string UpdateUserInstitution(DB_UserInstitution userInst)
        {
            string cmdStr = " UPDATE UserInstitution " +
                            " SET user_address = @user_address, bf_customerId = @bf_customerId, bf_encryptionKey = @bf_encryptionKey, with_mfa = @with_mfa " +
                            " WHERE username = @username AND institution_slug = @slug;";

            SqlCommand command = new SqlCommand(cmdStr, SQLraw);
            command.Parameters.AddWithValue("@username", userInst.Username);
            command.Parameters.AddWithValue("@slug", userInst.InstitutionSlug);
            command.Parameters.AddWithValue("@user_address", userInst.UserAddress);
            command.Parameters.AddWithValue("@bf_customerId", userInst.bfCustomerID);
            command.Parameters.AddWithValue("@bf_encryptionKey", userInst.bfEncryptionKey);
            command.Parameters.AddWithValue("@with_mfa", userInst.WithMfa);

            try { command.ExecuteNonQuery(); }
            catch (Exception e)
            {
                Supporting.WriteLog(e.ToString());
                return "error changing user institution details";
            }

            return "info: transaction successful";
        }

        public string UpdateAccount(DB_Account acc)
        {
            string cmdStr = " UPDATE Accounts " +
                            " SET username = @username , institution_slug = @institution_slug , bank_name = @bank_name, bf_id = @bf_id, " +
                            " account_type = @account_type , account_holder = @account_holder, account_number = @account_number, " +
                            " current_balance = @current_balance ,available_balance = @available_balance ,bsb = @bsb " +
                            " WHERE username = @username AND account_number = @account_number;";

            SqlCommand command = new SqlCommand(cmdStr, SQLraw);
            command.Parameters.AddWithValue("@username", acc.Username);
            command.Parameters.AddWithValue("@institution_slug", acc.Institution);
            command.Parameters.AddWithValue("@bf_id", acc.AccIdBF);
            command.Parameters.AddWithValue("@bank_name", acc.BankName);
            command.Parameters.AddWithValue("@account_type", acc.AccountType);
            command.Parameters.AddWithValue("@account_holder", acc.AccountHolder);
            command.Parameters.AddWithValue("@account_number", acc.AccountNumber);
            command.Parameters.AddWithValue("@current_balance", acc.CurrentBalance);
            command.Parameters.AddWithValue("@available_balance", acc.AvailableBalance);
            command.Parameters.AddWithValue("@bsb", acc.BSB ?? "");

            try { command.ExecuteNonQuery(); }
            catch (Exception e)
            {
                Supporting.WriteLog(e.ToString());
                return "error editing account";
            }

            return "info: account edited";
        }

        public string UpdateBudgetItem(DB_BudgetItem item)
        {
            string cmdStr = " UPDATE BudgetItem " +
                            " SET parent_uid = @parent_uid , name = @name , description = @description, sub_index = @sub_index, " +
                            " active = @active " +
                            " WHERE unique_id = @unique_id;";

            SqlCommand command = new SqlCommand(cmdStr, SQLraw);
            command.Parameters.AddWithValue("@unique_id", item.UniqueId);
            command.Parameters.AddWithValue("@parent_uid", item.ParentUID);
            command.Parameters.AddWithValue("@name", item.Name);
            command.Parameters.AddWithValue("@description", item.Description);
            command.Parameters.AddWithValue("@sub_index", item.SubIndex);
            command.Parameters.AddWithValue("@active", item.Active);

            try { command.ExecuteNonQuery(); }
            catch (Exception e)
            {
                Supporting.WriteLog(e.ToString());
                return "error editing account";
            }

            return "info: item edited";
        }

        public string UpdateRuleItem(DB_RuleItem item)
        {
            string cmdStr = " UPDATE RuleItem " +
                            " SET name = @name , budget_reference = @budget_reference " +
                            " WHERE reference = @reference;";

            SqlCommand command = new SqlCommand(cmdStr, SQLraw);
            command.Parameters.AddWithValue("@name", item.Name);
            command.Parameters.AddWithValue("@budget_reference", item.BudgetReference);
            command.Parameters.AddWithValue("@reference", item.Reference);

            try { command.ExecuteNonQuery(); }
            catch (Exception e)
            {
                Supporting.WriteLog(e.ToString());
                return "error editing rule item";
            }

            return "info: rule item edited";
        }

        public string UpdateRuleMatch(DB_RuleMatch item)
        {
            string cmdStr = " UPDATE RuleMatch " +
                            " SET match_item = @match_item , match_type = @match_type, match = @match " +
                            " WHERE rule_reference = @rule_reference AND match_id = @match_id;";

            SqlCommand command = new SqlCommand(cmdStr, SQLraw);
            command.Parameters.AddWithValue("@match_item", item.MatchItem);
            command.Parameters.AddWithValue("@match_type", item.MatchType);
            command.Parameters.AddWithValue("@match", item.MatchText);
            command.Parameters.AddWithValue("@rule_reference", item.RuleReference);
            command.Parameters.AddWithValue("@match_id", item.MatchID);

            try { command.ExecuteNonQuery(); }
            catch (Exception e)
            {
                Supporting.WriteLog(e.ToString());
                return "error editing rule match";
            }

            return "info: rule match edited";
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
