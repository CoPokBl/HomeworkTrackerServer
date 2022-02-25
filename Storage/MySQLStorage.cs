using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using MySql.Data.MySqlClient;

namespace HomeworkTrackerServer.Storage {
    
    // TODO: Test it
    public class MySQLStorage : IStorageMethod {

        private MySqlConnection _connection;  // MySQL Connection Object

        public List<Dictionary<string, string>> GetTasks(string username) {
            
            using var cmd = new MySqlCommand("SELECT * FROM hw_tasks WHERE owner = @user", _connection);
            cmd.Parameters.AddWithValue("@user", username);
            using MySqlDataReader rdr = cmd.ExecuteReader();

            List<Dictionary<string, string>> tasks = new List<Dictionary<string, string>>();
            while (rdr.Read()) {
                Dictionary<string, string> task = new Dictionary<string, string> {
                    {"class", rdr.GetString("class")},
                    {"classColour", rdr.GetString("classColour")},
                    {"task", rdr.GetString("task")},
                    {"type", rdr.GetString("ttype")},
                    {"typeColour", rdr.GetString("typeColour")},
                    {"dueDate", rdr.GetString("dueDate")},
                    {"id", rdr.GetString("id")}
                };
                tasks.Add(task);
                break;
            }

            return tasks;
        }

        public bool AuthUser(string username, string password) {
            
            Program.Debug($"Authenticating user: {username}");
            
            using var cmd = new MySqlCommand("SELECT * FROM hw_users WHERE username = @user", _connection);
            cmd.Parameters.AddWithValue("@user", username);
            using MySqlDataReader rdr = cmd.ExecuteReader();

            string correctPass = null;
            bool exists = false;
            while (rdr.Read()) {
                correctPass = rdr.GetString("password") ?? "";
                exists = true;
                break;
            }
            
            if (!exists) {
                Program.Debug($"User failed Authentication with username '{username}' because that name doesn't exist"); 
                return false;
            }
            if (password == correctPass) {
                Program.Debug($"User '{username}' succeeded authentication");
                return true;
            }
            Program.Debug($"User failed Authentication with username '{username}' because the password is wrong");
            return false;

        }

        public bool CreateUser(string username, string password) {
            using var cmd = new MySqlCommand("SELECT * FROM hw_users WHERE username = @user", _connection);
            cmd.Parameters.AddWithValue("@user", username);
            using MySqlDataReader rdr = cmd.ExecuteReader();

            bool exists = false;
            while (rdr.Read()) {
                exists = true;
                break;
            }
            
            if (exists) {
                Program.Debug($"Failed to create user {username} because that name is taken");
                return false;
            }
            
            using var cmd2 = new MySqlCommand("INSERT INTO hw_users (username, password) VALUES (@user, @pass)", _connection);
            cmd2.Parameters.AddWithValue("@user", username);
            cmd2.Parameters.AddWithValue("@pass", password);
            cmd2.ExecuteNonQuery();
            Program.Debug($"Created user {username}");
            return true;
        }

        public void RemoveUser(string username) {
            using var cmd2 = new MySqlCommand("DELETE FROM hw_users WHERE username=@user", _connection);
            cmd2.Parameters.AddWithValue("@user", username);
            cmd2.ExecuteNonQuery();
            Program.Debug($"Removed User: {username}");
        }

        public void AddTask(string username, Dictionary<string, string> values) {
            
            Program.Debug("Adding task for " + username);

            string classText = "None";
            string classColour = "-1.-1.-1";
            string task = "None";
            string typeText = "None";
            string typeColour = "-1.-1.-1";
            long dueDate = 0;

            if (values.ContainsKey("class")) { classText = values["class"]; }
            if (values.ContainsKey("classColour")) { classColour = values["classColour"]; }
            if (values.ContainsKey("task")) { task = values["task"]; }
            if (values.ContainsKey("type")) { typeText = values["type"]; }
            if (values.ContainsKey("typeColour")) { typeColour = values["typeColour"]; }
            if (values.ContainsKey("dueDate")) 
                if (long.Parse(values["dueDate"]) != 0) { dueDate = DateTime.FromBinary(long.Parse(values["dueDate"])).ToBinary(); }

            FromStr(classColour);
            FromStr(typeColour);

            var outData = new Dictionary<string, string> {
                { "class", classText },
                { "classColour", classColour },
                { "task", task },
                { "type", typeText },
                { "typeColour", typeColour },
                { "dueDate", dueDate.ToString() },
                { "id", Guid.NewGuid().ToString() }
            };
            using var cmd2 = new MySqlCommand(
                "INSERT INTO hw_tasks (owner, class, classColour, task, ttype, typeColour, dueDate, id) " +
                "VALUES (@user, @class, @cc, @task, @ttype, @tc, @due, @id)", _connection);
            cmd2.Parameters.AddWithValue("@user", username);
            cmd2.Parameters.AddWithValue("@class", outData["class"]);
            cmd2.Parameters.AddWithValue("@cc", outData["classColour"]);
            cmd2.Parameters.AddWithValue("@task", outData["task"]);
            cmd2.Parameters.AddWithValue("@ttype", outData["type"]);
            cmd2.Parameters.AddWithValue("@tc", outData["typeColour"]);
            cmd2.Parameters.AddWithValue("@due", outData["dueDate"]);
            cmd2.Parameters.AddWithValue("@id", outData["id"]);
            cmd2.ExecuteNonQuery();
        }

        public bool RemoveTask(string username /* This is needed for RAMManager because of the way it is setup */, string id) {
            using var cmd2 = new MySqlCommand("DELETE FROM hw_tasks WHERE id=@id", _connection);
            cmd2.Parameters.AddWithValue("@id", id);
            cmd2.ExecuteNonQuery();
            Program.Debug($"Removed: {id} from {username}'s tasks");
            return true;
        }
        
        public bool EditTask(string username, string id, string field, string newValue) {
            if (field == "id" || field == "owner") { throw new ArgumentException($"The field '{field}' cannot be edited"); }
            if (field == "classColour" || field == "typeColour") { FromStr(newValue); }
            if (field == "dueDate") { DateTime.FromBinary(long.Parse(newValue)); }
            
            using var cmd2 = new MySqlCommand("UPDATE hw_tasks SET @field=@value WHERE id=@id", _connection);
            cmd2.Parameters.AddWithValue("@field", field);
            cmd2.Parameters.AddWithValue("@value", newValue);
            cmd2.Parameters.AddWithValue("@id", id);
            cmd2.ExecuteNonQuery();
            Program.Debug($"Edited {id} from {username}'s tasks");
            return true;
        }

        public void Init() {
            Program.Info("Connecting to MySQL...");
            string cs = $"server={Program.Config["mysql_ip"]};" +
                        $"userid={Program.Config["mysql_user"]};" +
                        $"password={Program.Config["mysql_password"]};" +
                        $"database={Program.Config["mysql_database"]}";

            try {
                _connection = new MySqlConnection(cs);
                _connection.Open();
            }
            catch (Exception e) {
                Program.Debug(e.ToString());
                throw new Exception("Failed to connect to MySQL");
            }
            Program.Info("Connected MySQL");
            Program.Debug($"MySQL Version: {_connection.ServerVersion}");
            Program.Info("Creating tables in MySQL...");
            CreateTables();
            Program.Info("Created MySQL tables");
        }
        
        private static Color FromStr(string str) {
            if (str == "-1.-1.-1") {
                return Color.Empty;
            }
            string[] strs = str.Split(".");
            return Color.FromArgb(255, Convert.ToInt32(strs[0]), Convert.ToInt32(strs[1]), Convert.ToInt32(strs[2]));
        }

        private void CreateTables() {
            SendMySqlStatement(@"CREATE TABLE IF NOT EXISTS hw_users(username VARCHAR(255), password VARCHAR(64))");
            SendMySqlStatement(@"CREATE TABLE IF NOT EXISTS hw_tasks(" +
                               "owner VARCHAR(255), " +
                               "class VARCHAR(255), " +
                               "classColour VARCHAR(11), " +
                               "task VARCHAR(255), " +
                               "ttype VARCHAR(255)), " + 
                               "typeColour VARCHAR(11), " + 
                               "dueDate VARCHAR(64), " + 
                               "id VARCHAR(64)");
        }

        private void SendMySqlStatement(string statement) {
            using var cmd = new MySqlCommand();
            cmd.Connection = _connection;
            cmd.CommandText = statement;
            cmd.ExecuteNonQuery();
        }

    }
}
