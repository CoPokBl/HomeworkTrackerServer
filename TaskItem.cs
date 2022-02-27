namespace HomeworkTrackerServer {
    public class TaskItem {
        public string Text;
        public ColouredString Class;
        public ColouredString Type;
        public string Id;
        public long DueDate;

        public TaskItem(string text, ColouredString cls, ColouredString type, string id, long dueDate) {
            Text = text;
            Class = cls;
            Type = type;
            Id = id;
            DueDate = dueDate;
        }
        
    }
}