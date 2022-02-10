namespace HomeworkTrackerServer {
    public class TaskItem {
        public string Text;
        public ColouredString Class;
        public ColouredString Type;
        public string Id;

        public TaskItem(string text, ColouredString cls, ColouredString type, string id) {
            Text = text;
            Class = cls;
            Type = type;
            Id = id;
        }
        
    }
}