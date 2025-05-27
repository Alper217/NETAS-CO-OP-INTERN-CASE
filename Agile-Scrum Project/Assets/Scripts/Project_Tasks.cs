using SQLite4Unity3d;
using System;

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
    public string status { get; set; } = "ToDo"; // Default deðer

    [NotNull]
    public string createdDate { get; set; }

    public int priority { get; set; } = 2; // Default orta öncelik

    // Constructor - otomatik tarih atar
    public Project_Tasks()
    {
        createdDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }

    // Manuel tarih ile constructor (isteðe baðlý)
    public Project_Tasks(int projectId, string title, string description = "", string status = "ToDo", int priority = 2)
    {
        this.projectId = projectId;
        this.title = title;
        this.description = description;
        this.status = status;
        this.priority = priority;
        this.createdDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
}