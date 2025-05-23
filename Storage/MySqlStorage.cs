using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using HomeworkTrackerServer.Objects;
using MySql.Data.MySqlClient;

namespace HomeworkTrackerServer.Storage; 

public class MySqlStorage : IStorageMethod {

    private MySqlConnection _connection;  // MySQL Connection Object
    private string _connectString;


    private static string Hash(string str) {
        StringBuilder builder = new();
        foreach (byte t in SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(str))) {
            builder.Append(t.ToString("x2"));
        }

        return builder.ToString();
    }

    public string GetUserPassword(string id) {
        using MySqlCommand cmd = new("SELECT * FROM hw_users WHERE id = @user", _connection);
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
        using MySqlCommand cmd = new(
            "UPDATE hw_users SET password=@value WHERE id=@user", _connection);
        cmd.Parameters.AddWithValue("@user", id);
        cmd.Parameters.AddWithValue("@value", newPassword);
        cmd.ExecuteNonQuery();
    }
        
    public void ChangeUsername(string id, string newUsername) {
        using MySqlCommand cmd = new(
            "UPDATE hw_users SET username=@value WHERE id=@user", _connection);
        cmd.Parameters.AddWithValue("@user", id);
        cmd.Parameters.AddWithValue("@value", newUsername);
        cmd.ExecuteNonQuery();
    }

    public User[] GetAllUsers() {
        using MySqlCommand cmd = new("SELECT * FROM hw_users", _connection);
        using MySqlDataReader rdr = cmd.ExecuteReader();

        List<User> users = new();
        while (rdr.Read()) {
            User usr = new() {
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
        using MySqlCommand cmd = new("SELECT * FROM hw_users WHERE id = @user", _connection);
        cmd.Parameters.AddWithValue("@user", userId);
        using MySqlDataReader rdr = cmd.ExecuteReader();

        User userObj = new();
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
        using MySqlCommand cmd = new("SELECT * FROM hw_users WHERE username=@user", _connection);
        cmd.Parameters.AddWithValue("@user", username);
        using MySqlDataReader rdr = cmd.ExecuteReader();

        string id = null;
        while (rdr.Read()) {
            id = rdr.GetString("id");
            rdr.Close();
            break;
        }
        return id;
            
    }

    public HomeworkTask GetTask(string taskId) {
        using MySqlCommand cmd = new("SELECT * FROM hw_tasks WHERE id=@id", _connection);
        cmd.Parameters.AddWithValue("@id", taskId);
        using MySqlDataReader rdr = cmd.ExecuteReader();

        HomeworkTask task = new();
        while (rdr.Read()) {
            task.Id = rdr.GetString("id");
            task.Class = rdr.GetString("class");
            task.ClassColour = rdr.GetString("classColour");
            task.Type = rdr.GetString("ttype");
            task.TypeColour = rdr.GetString("typeColour");
            task.Task = rdr.GetString("task");
            task.DueDate = long.Parse(rdr.GetString("dueDate"));
            task.Owner = rdr.GetString("owner");
            rdr.Close();
            return task;
        }
#pragma warning disable CS0162
        rdr.Close();
        return null;
#pragma warning restore CS0162
    }

    public string GetOwnerOfTask(string taskId) {
        using MySqlCommand cmd = new("SELECT * FROM hw_tasks WHERE id=@id", _connection);
        cmd.Parameters.AddWithValue("@id", taskId);
        using MySqlDataReader rdr = cmd.ExecuteReader();
        string owner = null;
        while (rdr.Read()) {
            owner = rdr.GetString("owner");
            rdr.Close();
            break;
        }
        return owner;
    }

    public List<Dictionary<string, string>> GetTasks(string id) {
            
        using MySqlCommand cmd = new("SELECT * FROM hw_tasks WHERE owner=@user", _connection);
        cmd.Parameters.AddWithValue("@user", id);
        using MySqlDataReader rdr = cmd.ExecuteReader();

        List<Dictionary<string, string>> tasks = new();
        while (rdr.Read()) {
            Dictionary<string, string> task = new() {
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
            
        Logger.Debug($"\nAuthenticating user: {username}");
            
        using MySqlCommand cmd = new("SELECT * FROM hw_users WHERE username=@user", _connection);
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
            Logger.Debug($"User failed Authentication with username '{username}' because that id doesn't exist\n");
            return false;
        }
        Logger.Debug("User exists, checking password, correct pass: " + correctPass);
        Logger.Debug($"Provided Password: {password}, Hash: {Hash(password)}");
        if (Hash(password) == correctPass) {
            Logger.Debug($"User '{username}' succeeded authentication\n");
            return true;
        }
        Logger.Debug($"User failed Authentication with username '{username}' because the password is wrong\n");
        return false;

    }

    public bool AuthUser(string username, string password) {
        return AuthUser(username, password, out _);
    }

    public bool CreateUser(User user) {
        using MySqlCommand cmd = new("SELECT * FROM hw_users WHERE Username=@user", _connection);
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
            
        using MySqlCommand cmd2 = new(
            "INSERT INTO hw_users (id, username, password, creationDate) VALUES (@id, @user, @pass, @creationDate)",
            _connection);
        cmd2.Parameters.AddWithValue("@id", user.Guid);
        cmd2.Parameters.AddWithValue("@user", user.Username);
        cmd2.Parameters.AddWithValue("@pass", user.Password);
        cmd2.Parameters.AddWithValue("@creationDate", user.CreationDate.ToString());
        cmd2.ExecuteNonQuery();
        Logger.Debug($"Created user {user.Username}");
        return true;
    }

    public void RemoveUser(string id) {
        using MySqlCommand cmd2 = new("DELETE FROM hw_users WHERE id=@user", _connection);
        cmd2.Parameters.AddWithValue("@user", id);
        cmd2.ExecuteNonQuery();
        Logger.Debug($"Removed User: {id}");
    }

    public bool TryAddTask(string userId, Dictionary<string, string> values, out string id) {
            
        Logger.Debug("Adding task for " + userId);
        id = null;

        bool success = Converter.TryConvertDicToTask(values, out HomeworkTask task);
        if (!success) { return false; }  // Invalid

        Dictionary<string, string> outData = new() {
            { "class", task.Class },
            { "classColour", task.ClassColour },
            { "task", task.Task },
            { "type", task.Type },
            { "typeColour", task.TypeColour },
            { "dueDate", task.DueDate.ToString() },
            { "id", task.Id }
        };
            
        // check mysql database for duplicate id
        using MySqlCommand cmd = new("SELECT * FROM hw_tasks WHERE id=@id", _connection);
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
            using MySqlCommand replaceCmd = new(
                "UPDATE hw_tasks SET class=@class, classColour=@classColour, task=@task, ttype=@type, typeColour=@typeColour, dueDate=@dueDate WHERE id=@id",
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
            
        using MySqlCommand cmd2 = new(
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
        
    public bool TryAddTask(string username, Dictionary<string, string> values) => TryAddTask(username, values, out _);

    public bool RemoveTask(string userId /* This is needed for RAMManager because of the way it is setup */, string id) {
        using MySqlCommand cmd2 = new("DELETE FROM hw_tasks WHERE id=@id", _connection);
        cmd2.Parameters.AddWithValue("@id", id);
        cmd2.ExecuteNonQuery();
        Logger.Debug($"Removed: {id} from {userId}'s tasks");
        return true;
    }
        
    public bool EditTask(string userId, string id, string field, string newValue) {
        if (field == "id" || field == "owner") { throw new ArgumentException($"The field '{field}' cannot be edited"); }
        if (field == "classColour" || field == "typeColour") { ColorFromStr(newValue); }
        if (field == "dueDate") { DateTime.FromBinary(long.Parse(newValue)); }
            
        using MySqlCommand cmd2 = new("UPDATE hw_tasks SET @field=@value WHERE id=@id", _connection);
        cmd2.Parameters.AddWithValue("@field", field);
        cmd2.Parameters.AddWithValue("@value", newValue);
        cmd2.Parameters.AddWithValue("@id", id);
        cmd2.ExecuteNonQuery();
        Logger.Debug($"Edited {id} from {userId}'s tasks");
        return true;
    }

    public void Init() {
        Logger.Info("Connecting to MySQL...");
        _connectString = $"server={Program.Config["mysql_ip"]};" +
                         $"userid={Program.Config["mysql_user"]};" +
                         $"password={Program.Config["mysql_password"]};" +
                         $"database={Program.Config["mysql_database"]}";

        try {
            _connection = new MySqlConnection(_connectString);
            _connection.Open();
        }
        catch (Exception e) {
            Logger.Debug(e.ToString());
            throw new StorageFailException("Failed to connect to MySQL");
        }
        Logger.Info("Connected MySQL");
        _connection.StateChange += DatabaseConnectStateChanged;
        Logger.Debug($"MySQL Version: {_connection.ServerVersion}");
        Logger.Info("Creating tables in MySQL...");
        CreateTables();
        Logger.Info("Created MySQL tables");
    }

    public void Deinit() {
        try {
            _connection.Close();
        }
        catch (Exception) {
            Logger.Error("Failed to close MySQL connection");
        }
    }
        
    // The return value isn't used because this function is used as a way to detect invalid colours
    private static Color ColorFromStr(string str) {
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
        using MySqlCommand cmd = new();
        cmd.Connection = _connection;
        cmd.CommandText = statement;
        cmd.ExecuteNonQuery();
    }

    private void DatabaseConnectStateChanged(object obj, StateChangeEventArgs args) {
        if (args.CurrentState != ConnectionState.Broken && 
            args.CurrentState != ConnectionState.Closed) {
            return;
        }
            
        // Reconnect
        try {
            _connection = new MySqlConnection(_connectString);
            _connection.Open();
        }
        catch (Exception e) {
            Logger.Error("MySQL reconnect failed: " + e);
            _connection.StateChange -= DatabaseConnectStateChanged;  // Don't loop connect
            throw new StorageFailException("Failed to reconnect to MySQL");
        }
    }

}