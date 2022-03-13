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

        public bool AddTask(string username, Dictionary<string, string> values, out string id) {
            
            Program.Debug("Adding task for " + username);
            id = null;
            
            if (!_tasks.ContainsKey(username)) { _tasks.Add(username, new List<Dictionary<string, string>>()); }
            
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

            id = task.Id;
            _tasks[username].Add(outData);
            return true;
        }

        public bool AddTask(string username, Dictionary<string, string> values) => AddTask(username, values, out _);

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
                if (field == "classColour" || field == "typeColour") { Converter.ColorFromString(newValue); }
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
        
        
    }
}
