using System.Collections.Generic;

namespace HomeworkTrackerServer.Storage {
    public interface IStorageMethod {
        
        public List<Dictionary<string, string>> GetTasks(string username);
        public bool AuthUser(string username, string password);
        public bool CreateUser(string username, string password);
        public void AddTask(string username, Dictionary<string, string> values);
        public bool RemoveTask(string username, string id);
        public bool EditTask(string username, string id, string field, string newValue);

        public void Init();
    }
}