using System;
using System.Collections.Generic;

namespace HomeworkTrackerServer.Storage {
    public class RAMStorage : IStorageMethod {

        public Dictionary<string, string> Users;
        public Dictionary<string, List<TaskItem>> Tasks;

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
            Tasks[username].Add(new TaskItem(task, classTxt, type));
        }

        public void Init() {
            Users = new Dictionary<string, string>();
            Tasks = new Dictionary<string, List<TaskItem>>();
        }
    }
}