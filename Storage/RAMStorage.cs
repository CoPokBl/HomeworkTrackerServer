using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using HomeworkTrackerServer.Objects;

namespace HomeworkTrackerServer.Storage; 

public class RamStorage : IStorageMethod {
    protected Dictionary<string, User> Users;                                            // Id, password
    protected Dictionary<string, List<Dictionary<string, string>>> Tasks;  // Username, list of tasks
        
    private static string Hash(string str) {
        StringBuilder builder = new StringBuilder();  
        foreach (byte t in SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(str))) {
            builder.Append(t.ToString("x2"));
        }

        return builder.ToString();
    }

    public List<Dictionary<string, string>> GetTasks(string id) => 
        !Tasks.ContainsKey(id) ? new List<Dictionary<string, string>>() : Tasks[id];

    public bool AuthUser(string username, string password, out string id) {
        Logger.Debug($"Authenticating user: {username}");
        id = null;

        foreach (User usr in Users.Values.Where(usr => usr.Username == username)) {
            Logger.Debug(usr.Password);
            Logger.Debug(Hash(password));
            Logger.Debug(Hash("a"));
            if (usr.Password != Hash(password)) {
                Logger.Debug($"User failed Authentication with username '{username}' because the password is wrong");
                return false;
            }

            id = usr.Guid;
            Logger.Debug($"User '{username}' succeeded authentication");
            return true;
        }

        Logger.Debug($"User failed Authentication with username '{username}' because that name doesn't exist");
        return false;
    }

    public bool AuthUser(string username, string password) => AuthUser(username, password, out _);

    public bool CreateUser(User user) {
        if (Users.ContainsKey(user.Guid)) {
            // somehow duplicate ID
            // Change GUID
            user.Guid = Guid.NewGuid().ToString();
        }
        if (Users.Any(kvp => kvp.Value.Username == user.Username)) {
            Logger.Debug($"Failed to create user {user.Username} because that name is taken");
            return false;
        }
        Users.Add(user.Guid, user);
        Logger.Debug($"Created user {user.Username}");
        return true;
    }

    public void RemoveUser(string username) { Users.Remove(username); }

    public bool TryAddTask(string username, Dictionary<string, string> values, out string id) {
            
        Logger.Debug("Adding task for " + username);
        id = null;
            
        if (!Tasks.ContainsKey(username)) { Tasks.Add(username, new List<Dictionary<string, string>>()); }
            
        bool success = Converter.TryConvertDicToTask(values, out HomeworkTask task);
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

        id = task.Id;
        if (Tasks[username].Any(t => t["id"] == task.Id)) {
            // replace
            int index = Tasks[username].FindIndex(t => t["id"] == task.Id);
            Tasks[username][index] = outData;
        }
            
        Tasks[username].Add(outData);
        return true;
    }

    public bool TryAddTask(string username, Dictionary<string, string> values) => TryAddTask(username, values, out _);

    public bool RemoveTask(string username, string id) {
        bool removed = false;
        foreach (Dictionary<string, string> task in Tasks[username].Where(task => task["id"] == id)) {
            Tasks[username].Remove(task);
            Logger.Debug($"Removed one of {username}'s tasks");
            removed = true;
            break;  // If there were multiple then something is wrong so who cares
            // I'd rather it be more efficient that add more error logging
        }
        return removed;
    }
        
    public bool EditTask(string username, string id, string field, string newValue) {
        if (field == "id") { throw new InvalidOperationException("The field 'id' cannot be edited"); }
        bool edited = false;
        foreach (Dictionary<string, string> task in Tasks[username].Where(task => task["id"] == id)) {
            switch (field) {
                // Validate values for non string fields
                case "classColour":
                case "typeColour":
                    Converter.ColorFromString(newValue);
                    break;
                case "dueDate":
                    DateTime.FromBinary(long.Parse(newValue));
                    break;
                
                // If it's not there then it doesn't need to be validated
                default: break;
            }
            task[field] = newValue;  // This will throw if the field is invalid
            edited = true;
            break;  // If there were multiple then something is wrong so who cares
            // I'd rather it be more efficient that add more error logging
        }
        return edited;
    }

    public string GetUserPassword(string username) => Users[username].Password;

    public void ChangePassword(string id, string newPassword) {
        Users[id].Password = newPassword;
    }

    public void ChangeUsername(string userId, string newUsername) {
        Users[userId].Username = newUsername;
    }

    public User[] GetAllUsers() => Users.Values.ToArray();
    public User GetUser(string userId) => Users[userId];
    public string GetUserId(string username) {
        return Users.Values.Where(usr => usr.Username == username).Select(usr => usr.Guid).FirstOrDefault();
        // Not found
    }

    public HomeworkTask GetTask(string taskId) => 
        (from usersTasks in Tasks
            from task in usersTasks.Value.Where(task => task["id"] == taskId)
            select new HomeworkTask {
                Owner = usersTasks.Key,
                Class = task["class"],
                ClassColour = task["classColour"],
                Type = task["type"],
                TypeColour = task["typeColour"],
                Task = task["task"],
                DueDate = long.Parse(task["dueDate"]),
                Id = task["id"]
            }).FirstOrDefault();
        

    public string GetOwnerOfTask(string taskId) {
        return (from usersTasks in Tasks
            from task in usersTasks.Value.Where(task => task["id"] == taskId)
            select usersTasks.Key).FirstOrDefault();
    }

    public virtual void Init() {
        Users = new Dictionary<string, User>();
        Tasks = new Dictionary<string, List<Dictionary<string, string>>>();
    }

    public virtual void Deinit() { }
        
}