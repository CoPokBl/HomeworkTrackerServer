using System.Collections.Generic;
using HomeworkTrackerServer.Objects;

namespace HomeworkTrackerServer.Storage {
    public interface IStorageMethod {
        public List<Dictionary<string, string>> GetTasks(string id);
        public bool AuthUser(string username, string password, out string id);
        public bool AuthUser(string username, string password);
        public bool CreateUser(User user);
        public void RemoveUser(string userId);
        public bool AddTask(string userId, Dictionary<string, string> values);
        public bool AddTask(string userId, Dictionary<string, string> values, out string id);
        public bool RemoveTask(string userId, string id);
        public bool EditTask(string userId, string id, string field, string newValue);
        public string GetUserPassword(string userId);
        public void ChangePassword(string userId, string newPassword);
        public void ChangeUsername(string userId, string newUsername);
        public User[] GetAllUsers();
        public User GetUser(string userId);
        public string GetUserId(string username);
        public HomeworkTask GetTask(string taskId);
        public string GetOwnerOfTask(string taskId);

        public void Init();
        public void Deinit();
    }
}