using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace HomeworkTrackerServer.Storage {
    public class RamStorage : IStorageMethod {

        public Dictionary<string, string> Users;          // Username, password
        public Dictionary<string, List<Dictionary<string, string>>> Tasks;  // Username, list of tasks

        public List<Dictionary<string, string>> GetTasks(string username) {
            return !Tasks.ContainsKey(username) ? new List<Dictionary<string, string>>() : Tasks[username];
        }

        public bool AuthUser(string username, string password) {
            Program.Debug($"Authenticating user: {username}");
            
            if (!Users.ContainsKey(username)) {
                Program.Debug($"User failed Authentication with username '{username}' because that name doesn't exist"); 
                return false;
            }
            string correctPass = Users[username];
            if (password == correctPass) {
                Program.Debug($"User '{username}' succeeded authentication");
                return true;
            }
            Program.Debug($"User failed Authentication with username '{username}' because the password is wrong");
            return false;

        }

        public bool CreateUser(string username, string password) {
            if (Users.ContainsKey(username)) {
                Program.Debug($"Failed to create user {username} because that name is taken");
                return false;
            }
            Users.Add(username, password);
            Program.Debug($"Created user {username}");
            return true;
        }

        public void AddTask(string username, Dictionary<string, string> values) {
            
            Program.Debug("Adding task for " + username);
            
            if (!Tasks.ContainsKey(username)) {
                Tasks.Add(username, new List<Dictionary<string, string>>());
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

            var outData = new Dictionary<string, string> {
                { "class", classText },
                { "classColour", classColour },
                { "task", task },
                { "type", typeText },
                { "typeColour", typeColour },
                { "dueDate", dueDate.ToString() },
                { "id", Guid.NewGuid().ToString() }
            };
            
            Tasks[username].Add(outData);
        }

        public bool RemoveTask(string username, string id) {
            bool removed = false;
            foreach (Dictionary<string, string> task in Tasks[username].Where(task => task["id"] == id)) {
                Tasks[username].Remove(task);
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
            foreach (Dictionary<string, string> task in Tasks[username].Where(task => task["id"] == id)) {
                // Validate values for non string fields
                if (field == "classColour" || field == "typeColour") { FromStr(newValue); }
                if (field == "dueDate") { DateTime.FromBinary(long.Parse(newValue)); }
                task[field] = newValue;
                edited = true;
                break;  // If there were multiple then something is wrong so who cares
                // I'd rather it be more efficient that add more error logging
            }
            return edited;
        }

        public void Init() {
            Users = new Dictionary<string, string>();
            Tasks = new Dictionary<string, List<Dictionary<string, string>>>();
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