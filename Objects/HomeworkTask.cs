namespace HomeworkTrackerServer.Objects {
    
    public class HomeworkTask {
        public string Owner;
        public string Class;
        public string ClassColour;
        public string Type;
        public string TypeColour;
        public string Task;
        public long DueDate;
        public string Id;

        public ExternalHomeworkTask ToExternal() {
            return new ExternalHomeworkTask {
                Class = Class,
                ClassColour = ClassColour,
                Type = Type,
                TypeColour = TypeColour,
                Task = Task,
                DueDate = DueDate
            };
        }
    }
    
    public class ExternalHomeworkTask {
        public string Class;
        public string ClassColour;
        public string Type;
        public string TypeColour;
        public string Task;
        public long DueDate;
    }
    
}