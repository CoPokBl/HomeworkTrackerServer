using System.Collections.Generic;

namespace HomeworkTrackerServer.Storage {
    public interface IStorageMethod {
        
        public List<TaskItem> GetTasks(string username);
        public bool AuthUser(string username, string password);
        public bool CreateUser(string username, string password);
        public void AddTask(string username, ColouredString classTxt, string task, ColouredString type);
        public bool RemoveTask(string username, string id);

        public void Init();
    }
}