using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using HomeworkTrackerServer.Objects;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using RayKeys.Misc;

namespace HomeworkTrackerServer.Storage {
    
    public class MySQLStorage : IStorageMethod {

        private MySqlConnection _connection;  // MySQL Connection Object
        private string _connectString;


        private static string Hash(string str) {
            StringBuilder builder = new StringBuilder();
            foreach (byte t in SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(str))) {
                builder.Append(t.ToString("x2"));
            }

            return builder.ToString();
        }

        public string GetUserPassword(string id) {
            using MySqlCommand cmd = new MySqlCommand("SELECT * FROM hw_users WHERE id = @user", _connection);
            cmd.Parameters.AddWithValue("@user", id);
            using MySqlDataReader rdr = cmd.ExecuteReader();

            string correctPass = null;
            while (rdr.Read()) {
                correctPass = rdr.GetString("password") ?? "";
                break;
            }
            rdr.Close();

            return correctPass;
        }

        public void ChangePassword(string id, string newPassword) {
            using MySqlCommand cmd = new MySqlCommand(
                "UPDATE hw_users SET password=@value WHERE id=@user", _connection);
            cmd.Parameters.AddWithValue("@user", id);
            cmd.Parameters.AddWithValue("@value", Hash(newPassword));
            cmd.ExecuteNonQuery();
        }
        
        public void ChangeUsername(string id, string newUsername) {
            using MySqlCommand cmd = new MySqlCommand(
                "UPDATE hw_users SET username=@value WHERE id=@user", _connection);
            cmd.Parameters.AddWithValue("@user", id);
            cmd.Parameters.AddWithValue("@value", newUsername);
            cmd.ExecuteNonQuery();
        }

        public User[] GetAllUsers() {
            using MySqlCommand cmd = new MySqlCommand("SELECT * FROM hw_users", _connection);
            using MySqlDataReader rdr = cmd.ExecuteReader();

            List<User> users = new List<User>();
            while (rdr.Read()) {
                User usr = new User {
                    Username = rdr.GetString("Username"),
                    Password = rdr.GetString("Password"),
                    CreationDate = long.Parse(rdr.GetString("creationDate")),
                    Guid = rdr.GetString("id")
                };
                users.Add(usr);
            }
            rdr.Close();

            return users.ToArray();
        }

        public User GetUser(string userId) {
            using MySqlCommand cmd = new MySqlCommand("SELECT * FROM hw_users WHERE id = @user", _connection);
            cmd.Parameters.AddWithValue("@user", userId);
            using MySqlDataReader rdr = cmd.ExecuteReader();

            User userObj = new User();
            while (rdr.Read()) {
                userObj = new User {
                    Guid = rdr.GetString("id"),
                    CreationDate = long.Parse(rdr.GetString("CreationDate")),
                    Password = rdr.GetString("Password"),
                    Username = rdr.GetString("Username")
                };
                break;
            }
            rdr.Close();

            return userObj;
        }

        public string GetUserId(string username) {
            using MySqlCommand cmd = new MySqlCommand("SELECT * FROM hw_users WHERE username=@user", _connection);
            cmd.Parameters.AddWithValue("@user", username);
            using MySqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read()) rdr.Close(); return rdr.GetString("id");
#pragma warning disable CS0162
            rdr.Close();

            return null;
#pragma warning restore CS0162
        }

        public HomeworkTask GetTask(string taskId) {
            using MySqlCommand cmd = new MySqlCommand("SELECT * FROM hw_tasks WHERE id=@id", _connection);
            cmd.Parameters.AddWithValue("@id", taskId);
            using MySqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read()) rdr.Close(); return new HomeworkTask {
                Id = rdr.GetString("id"),
                Class = rdr.GetString("class"),
                ClassColour = rdr.GetString("classColour"),
                Type = rdr.GetString("ttype"),
                TypeColour = rdr.GetString("typeColour"),
                Task = rdr.GetString("task"),
                DueDate = long.Parse(rdr.GetString("dueDate")),
                Owner = rdr.GetString("owner")
            };
#pragma warning disable CS0162
            rdr.Close();

            return null;
#pragma warning restore CS0162
        }

        public string GetOwnerOfTask(string taskId) {
            using MySqlCommand cmd = new MySqlCommand("SELECT * FROM hw_tasks WHERE id=@id", _connection);
            cmd.Parameters.AddWithValue("@id", taskId);
            using MySqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read()) rdr.Close(); return rdr.GetString("owner");
#pragma warning disable CS0162
            rdr.Close();

            return null;
#pragma warning restore CS0162
        }

        public List<Dictionary<string, string>> GetTasks(string id) {
            
            using MySqlCommand cmd = new MySqlCommand("SELECT * FROM hw_tasks WHERE owner=@user", _connection);
            cmd.Parameters.AddWithValue("@user", id);
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
            }
            rdr.Close();

            return tasks;
        }

        public bool AuthUser(string username, string password, out string id) {
            
            Logger.Debug($"Authenticating user: {username}");
            
            using MySqlCommand cmd = new MySqlCommand("SELECT * FROM hw_users WHERE username=@user", _connection);
            cmd.Parameters.AddWithValue("@user", username);
            using MySqlDataReader rdr = cmd.ExecuteReader();

            string correctPass = null;
            bool exists = false;
            id = null;
            while (rdr.Read()) {
                correctPass = rdr.GetString("password") ?? "";
                id = rdr.GetString("id");
                exists = true;
                break;
            }
            rdr.Close();
            
            if (!exists) {
                Logger.Debug($"User failed Authentication with username '{username}' because that id doesn't exist");
                return false;
            }
            if (Hash(password) == correctPass) {
                Logger.Debug($"User '{username}' succeeded authentication");
                return true;
            }
            Logger.Debug($"User failed Authentication with username '{username}' because the password is wrong");
            return false;

        }

        public bool AuthUser(string username, string password) {
            return AuthUser(username, password, out _);
        }

        public bool CreateUser(User user) {
            using MySqlCommand cmd = new MySqlCommand("SELECT * FROM hw_users WHERE Username=@user", _connection);
            cmd.Parameters.AddWithValue("@user", user.Username);
            using MySqlDataReader rdr = cmd.ExecuteReader();

            bool exists = false;
            while (rdr.Read()) {
                exists = true;
                break;
            }
            rdr.Close();
            
            if (exists) {
                Logger.Debug($"Failed to create user {user.Username} because that name is taken");
                return false;
            }
            
            using MySqlCommand cmd2 = new MySqlCommand(
                "INSERT INTO hw_users (id, username, password, creationDate) VALUES (@id, @user, @pass, @creationDate)",
                _connection);
            cmd2.Parameters.AddWithValue("@user", user.Username);
            cmd2.Parameters.AddWithValue("@pass", Hash(user.Password));
            cmd2.ExecuteNonQuery();
            Logger.Debug($"Created user {user.Username}");
            return true;
        }

        public void RemoveUser(string id) {
            using MySqlCommand cmd2 = new MySqlCommand("DELETE FROM hw_users WHERE id=@user", _connection);
            cmd2.Parameters.AddWithValue("@user", id);
            cmd2.ExecuteNonQuery();
            Logger.Debug($"Removed User: {id}");
        }

        public bool AddTask(string userId, Dictionary<string, string> values, out string id) {
            
            Logger.Debug("Adding task for " + userId);
            id = null;

            bool success = Converter.DictionaryToHomeworkTask(values, out HomeworkTask task, true);
            if (!success) { return false; }  // Invalid

            Dictionary<string, string> outData = new Dictionary<string, string> {
                { "class", task.Class },
                { "classColour", task.ClassColour },
                { "task", task.Task },
                { "type", task.Type },
                { "typeColour", task.TypeColour },
                { "dueDate", task.DueDate.ToString() },
                { "id", task.Id }
            };
            
            // check mysql database for duplicate id
            using MySqlCommand cmd = new MySqlCommand("SELECT * FROM hw_tasks WHERE id=@id", _connection);
            cmd.Parameters.AddWithValue("@id", task.Id);
            using MySqlDataReader rdr = cmd.ExecuteReader();
            bool exists = false;
            while (rdr.Read()) {
                exists = true;
                break;
            }
            rdr.Close();
            if (exists) {
                // replace it
                using MySqlCommand replaceCmd = new MySqlCommand(
                    "UPDATE hw_tasks SET class=@class, classColour=@classColour, task=@task, type=@type, typeColour=@typeColour, dueDate=@dueDate WHERE id=@id",
                    _connection);
                replaceCmd.Parameters.AddWithValue("@class", task.Class);
                replaceCmd.Parameters.AddWithValue("@classColour", task.ClassColour);
                replaceCmd.Parameters.AddWithValue("@task", task.Task);
                replaceCmd.Parameters.AddWithValue("@type", task.Type);
                replaceCmd.Parameters.AddWithValue("@typeColour", task.TypeColour);
                replaceCmd.Parameters.AddWithValue("@dueDate", task.DueDate.ToString());
                replaceCmd.Parameters.AddWithValue("@id", task.Id);
                replaceCmd.ExecuteNonQuery();
                Logger.Debug($"Replaced task {task.Id}");
                return true;
            }
            
            using MySqlCommand cmd2 = new MySqlCommand(
                "INSERT INTO hw_tasks (owner, class, classColour, task, ttype, typeColour, dueDate, id) " +
                "VALUES (@user, @class, @cc, @task, @ttype, @tc, @due, @id)", _connection);
            cmd2.Parameters.AddWithValue("@user", userId);
            cmd2.Parameters.AddWithValue("@class", outData["class"]);
            cmd2.Parameters.AddWithValue("@cc", outData["classColour"]);
            cmd2.Parameters.AddWithValue("@task", outData["task"]);
            cmd2.Parameters.AddWithValue("@ttype", outData["type"]);
            cmd2.Parameters.AddWithValue("@tc", outData["typeColour"]);
            cmd2.Parameters.AddWithValue("@due", outData["dueDate"]);
            cmd2.Parameters.AddWithValue("@id", outData["id"]);
            cmd2.ExecuteNonQuery();
            return true;
        }
        
        public bool AddTask(string username, Dictionary<string, string> values) => AddTask(username, values, out _);

        public bool RemoveTask(string userId /* This is needed for RAMManager because of the way it is setup */, string id) {
            using MySqlCommand cmd2 = new MySqlCommand("DELETE FROM hw_tasks WHERE id=@id", _connection);
            cmd2.Parameters.AddWithValue("@id", id);
            cmd2.ExecuteNonQuery();
            Logger.Debug($"Removed: {id} from {userId}'s tasks");
            return true;
        }
        
        public bool EditTask(string userId, string id, string field, string newValue) {
            if (field == "id" || field == "owner") { throw new ArgumentException($"The field '{field}' cannot be edited"); }
            if (field == "classColour" || field == "typeColour") { FromStr(newValue); }
            if (field == "dueDate") { DateTime.FromBinary(long.Parse(newValue)); }
            
            using MySqlCommand cmd2 = new MySqlCommand("UPDATE hw_tasks SET @field=@value WHERE id=@id", _connection);
            cmd2.Parameters.AddWithValue("@field", field);
            cmd2.Parameters.AddWithValue("@value", newValue);
            cmd2.Parameters.AddWithValue("@id", id);
            cmd2.ExecuteNonQuery();
            Logger.Debug($"Edited {id} from {userId}'s tasks");
            return true;
        }

        public void Init(IConfiguration config) {
            Logger.Info("Connecting to MySQL...");
            _connectString = $"server={config["mysql_ip"]};" +
                             $"userid={config["mysql_user"]};" +
                             $"password={config["mysql_password"]};" +
                             $"database={config["mysql_database"]}";

            try {
                _connection = new MySqlConnection(_connectString);
                _connection.Open();
            }
            catch (Exception e) {
                Logger.Debug(e.ToString());
                throw new Exception("Failed to connect to MySQL");
            }
            Logger.Info("Connected MySQL");
            _connection.StateChange += DatabaseConnectStateChanged;
            Logger.Debug($"MySQL Version: {_connection.ServerVersion}");
            Logger.Info("Creating tables in MySQL...");
            CreateTables();
            Logger.Info("Created MySQL tables");
        }
        
        private static Color FromStr(string str) {
            if (str == "-1.-1.-1") {
                return Color.Empty;
            }
            string[] strs = str.Split(".");
            return Color.FromArgb(255, Convert.ToInt32(strs[0]), Convert.ToInt32(strs[1]), Convert.ToInt32(strs[2]));
        }

        private void CreateTables() {
            SendMySqlStatement(@"CREATE TABLE IF NOT EXISTS hw_users(
                                id VARCHAR(64),
                                username VARCHAR(255), 
                                password VARCHAR(64),
                                creationDate VARCHAR(64))");
            SendMySqlStatement(@"CREATE TABLE IF NOT EXISTS hw_tasks(" +
                               "owner VARCHAR(64), " +
                               "class VARCHAR(255), " +
                               "classColour VARCHAR(11), " +
                               "task VARCHAR(255), " +
                               "ttype VARCHAR(255), " + 
                               "typeColour VARCHAR(11), " + 
                               "dueDate VARCHAR(64), " + 
                               "id VARCHAR(64))");
        }

        private void SendMySqlStatement(string statement) {
            using MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = _connection;
            cmd.CommandText = statement;
            cmd.ExecuteNonQuery();
        }

        private void DatabaseConnectStateChanged(object obj, StateChangeEventArgs args) {
            if (args.CurrentState != ConnectionState.Broken && args.CurrentState != ConnectionState.Closed) return;
            
            // Reconnect
            try {
                _connection = new MySqlConnection(_connectString);
                _connection.Open();
            }
            catch (Exception e) {
                Logger.Error("MySQL reconnect failed: " + e);
                _connection.StateChange -= DatabaseConnectStateChanged;  // Don't loop connect
                throw new Exception("Failed to reconnect to MySQL");
            }
        }

    }
}
