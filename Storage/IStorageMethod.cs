using System.Collections.Generic;
using HomeworkTrackerServer.Objects;
using Microsoft.Extensions.Configuration;

namespace HomeworkTrackerServer.Storage {
    public interface IStorageMethod {
        // TODO: fix the methods in the storage files to make them use user ids instead of usernames
        
        public List<Dictionary<string, string>> GetTasks(string username);
        public bool AuthUser(string username, string password, out string id);
        public bool AuthUser(string username, string password);
        public bool CreateUser(User user);
        public void RemoveUser(string userId);
        public void AddTask(string userId, Dictionary<string, string> values);
        public bool RemoveTask(string userId, string id);
        public bool EditTask(string userId, string id, string field, string newValue);
        public string GetUserPassword(string userId);
        public void ChangePassword(string userId, string newPassword);
        public void ChangeUsername(string userId, string newUsername);
        public User[] GetAllUsers();
        public User GetUser(string userId);
        public string GetUserId(string username);

        public void Init(IConfiguration config);
    }
}