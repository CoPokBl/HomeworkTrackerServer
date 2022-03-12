using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using HomeworkTrackerServer.Objects;
using Microsoft.Extensions.Configuration;

namespace HomeworkTrackerServer.Storage {
    public class RamStorage : IStorageMethod {
        private Dictionary<string, User> _users;                                            // Id, password
        private Dictionary<string, List<Dictionary<string, string>>> _tasks;  // Username, list of tasks
        
        private static string Hash(string str) {
            StringBuilder builder = new StringBuilder();  
            foreach (byte t in SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(str))) {
                builder.Append(t.ToString("x2"));
            }

            return builder.ToString();
        }

        public List<Dictionary<string, string>> GetTasks(string id) => 
            !_tasks.ContainsKey(id) ? new List<Dictionary<string, string>>() : _tasks[id];

        public bool AuthUser(string username, string password, out string id) {
            Program.Debug($"Authenticating user: {username}");
            id = null;

            foreach (User usr in _users.Values) {
                if (usr.Username != username) continue;
                Program.Debug(usr.Password);
                Program.Debug(Hash(password));
                Program.Debug(Hash("a"));
                if (usr.Password != Hash(password)) {
                    Program.Debug($"User failed Authentication with username '{username}' because the password is wrong");
                    return false;
                }

                id = usr.Guid;
                Program.Debug($"User '{username}' succeeded authentication");
                return true;
            }

            Program.Debug($"User failed Authentication with username '{username}' because that name doesn't exist");
            return false;
        }

        public bool AuthUser(string username, string password) => AuthUser(username, password, out _);

        public bool CreateUser(User user) {
            if (_users.ContainsKey(user.Guid)) {
                // somehow duplicate ID
                // Change GUID
                user.Guid = Guid.NewGuid().ToString();
            }
            if (_users.Any(kvp => kvp.Value.Username == user.Username)) {
                Program.Debug($"Failed to create user {user.Username} because that name is taken");
                return false;
            }
            _users.Add(user.Guid, user);
            Program.Debug($"Created user {user.Username}");
            return true;
        }

        public void RemoveUser(string username) { _users.Remove(username); }

        public void AddTask(string username, Dictionary<string, string> values) {
            
            Program.Debug("Adding task for " + username);
            
            if (!_tasks.ContainsKey(username)) {
                _tasks.Add(username, new List<Dictionary<string, string>>());
            }

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

            Dictionary<string, string> outData = new Dictionary<string, string> {
                { "class", classText },
                { "classColour", classColour },
                { "task", task },
                { "type", typeText },
                { "typeColour", typeColour },
                { "dueDate", dueDate.ToString() },
                { "id", Guid.NewGuid().ToString() }
            };
            
            _tasks[username].Add(outData);
        }

        public bool RemoveTask(string username, string id) {
            bool removed = false;
            foreach (Dictionary<string, string> task in _tasks[username].Where(task => task["id"] == id)) {
                _tasks[username].Remove(task);
                Program.Debug($"Removed one of {username}'s tasks");
                removed = true;
                break;  // If there were multiple then something is wrong so who cares
                // I'd rather it be more efficient that add more error logging
            }
            return removed;
        }
        
        public bool EditTask(string username, string id, string field, string newValue) {
            if (field == "id") { throw new Exception("The field 'id' cannot be edited"); }
            bool edited = false;
            foreach (Dictionary<string, string> task in _tasks[username].Where(task => task["id"] == id)) {
                // Validate values for non string fields
                if (field == "classColour" || field == "typeColour") { FromStr(newValue); }
                if (field == "dueDate") { DateTime.FromBinary(long.Parse(newValue)); }
                task[field] = newValue;  // This will throw if the field is invalid
                edited = true;
                break;  // If there were multiple then something is wrong so who cares
                // I'd rather it be more efficient that add more error logging
            }
            return edited;
        }

        public string GetUserPassword(string username) => _users[username].Password;

        public void ChangePassword(string id, string newPassword) {
            _users[id].Password = Hash(newPassword);
        }

        public void ChangeUsername(string userId, string newUsername) {
            _users[userId].Username = newUsername;
        }

        public User[] GetAllUsers() => _users.Values.ToArray();
        public User GetUser(string userId) => _users[userId];
        public string GetUserId(string username) {
            return _users.Values.Where(usr => usr.Username == username).Select(usr => usr.Guid).FirstOrDefault();
            // Not found
        }

        public void Init(IConfiguration config) {
            _users = new Dictionary<string, User>();
            _tasks = new Dictionary<string, List<Dictionary<string, string>>>();
        }
        
        private static Color FromStr(string str) {
            if (str == "-1.-1.-1") {
                return Color.Empty;
            }
            string[] strs = str.Split(".");
            return Color.FromArgb(255, Convert.ToInt32(strs[0]), Convert.ToInt32(strs[1]), Convert.ToInt32(strs[2]));
        }
        
    }
}
