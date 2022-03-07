using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace HomeworkTrackerServer.Storage {
    public interface IStorageMethod {
        
        public List<Dictionary<string, string>> GetTasks(string username);
        public bool AuthUser(string username, string password);
        public bool CreateUser(string username, string password);
        public void RemoveUser(string username);
        public void AddTask(string username, Dictionary<string, string> values);
        public bool RemoveTask(string username, string id);
        public bool EditTask(string username, string id, string field, string newValue);
        public string GetUserPassword(string username);
        public void ChangePassword(string username, string newPassword);

        public void Init(IConfiguration config);
    }
}