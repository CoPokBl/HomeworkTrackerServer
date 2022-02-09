namespace HomeworkTrackerServer {
    public class TaskItem {
        public ColouredString Text;
        public string Class;
        public ColouredString Type;

        public TaskItem(ColouredString text, string cls, ColouredString type) {
            Text = text;
            Class = cls;
            Type = type;
        }
        
    }
}