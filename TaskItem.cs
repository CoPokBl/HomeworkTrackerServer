namespace HomeworkTrackerServer {
    public class TaskItem {
        public string Text;
        public ColouredString Class;
        public ColouredString Type;

        public TaskItem(string text, ColouredString cls, ColouredString type) {
            Text = text;
            Class = cls;
            Type = type;
        }
        
    }
}