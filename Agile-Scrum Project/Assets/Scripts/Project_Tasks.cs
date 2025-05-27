using SQLite4Unity3d;

public class Project_Tasks
{
    [PrimaryKey, AutoIncrement]
    public int id { get; set; }

    [NotNull]
    public int projectId { get; set; }

    [NotNull]
    public string title { get; set; }

    public string description { get; set; }

    [NotNull]
    public string status { get; set; }

    [NotNull]
    public string createdDate { get; set; }

    public int priority { get; set; } // Varsayýlan deðer 0
}
