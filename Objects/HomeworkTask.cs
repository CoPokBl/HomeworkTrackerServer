namespace HomeworkTrackerServer.Objects {
    
    /// <summary>
    /// A Homework Task represented in object form
    /// </summary>
    public class HomeworkTask {
        public string Owner;
        public string Class;
        public string ClassColour;
        public string Type;
        public string TypeColour;
        public string Task;
        public long DueDate;
        public string Id;

        /// <summary>
        /// Convert task to censored object (object without important fields)
        /// </summary>
        /// <returns>The resulting object</returns>
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
    
    /// <summary>
    /// A censored version of a Homework Task
    /// </summary>
    public class ExternalHomeworkTask {
        public string Class;
        public string ClassColour;
        public string Type;
        public string TypeColour;
        public string Task;
        public long DueDate;
    }
    
}