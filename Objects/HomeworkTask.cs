namespace HomeworkTrackerServer.Objects; 

/// <summary>
/// A Homework Task represented in object form
/// </summary>
public class HomeworkTask {
    public string Owner { get; set; }
    public string Class { get; set; }
    public string ClassColour { get; set; }
    public string Type { get; set; }
    public string TypeColour { get; set; }
    public string Task { get; set; }
    public long DueDate { get; set; }
    public string Id { get; set; }

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
    public string Class { get; init; }
    public string ClassColour { get; init; }
    public string Type { get; init; }
    public string TypeColour { get; init; }
    public string Task { get; init; }
    public long DueDate { get; init; }
}