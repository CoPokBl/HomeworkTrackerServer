using System;
using System.Collections.Generic;
using System.Linq;

namespace HomeworkTrackerServer.Storage {
    public class RamStorage : IStorageMethod {

        public Dictionary<string, string> Users;          // Username, password
        public Dictionary<string, List<TaskItem>> Tasks;  // Username, list of tasks

        public List<TaskItem> GetTasks(string username) {
            return !Tasks.ContainsKey(username) ? new List<TaskItem>() : Tasks[username];
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

        public void AddTask(string username, ColouredString classTxt, string task, ColouredString type) {
            if (!Tasks.ContainsKey(username)) {
                Tasks.Add(username, new List<TaskItem>());
            }
            
            Tasks[username].Add(new TaskItem(task, classTxt, type, Guid.NewGuid().ToString()));
        }

        public bool RemoveTask(string username, string id) {
            bool removed = false;
            foreach (TaskItem task in Tasks[username].Where(task => task.Id == id)) {
                Tasks[username].Remove(task);
                Program.Debug($"Removed one of {username}'s tasks");
                removed = true;
                break;  // If there were multiple then something is wrong so who cares
                // I'd rather it be more efficient that add more error logging
            }
            return removed;
        }

        public void Init() {
            Users = new Dictionary<string, string>();
            Tasks = new Dictionary<string, List<TaskItem>>();
        }
    }
}