using SQLite4Unity3d;
using System;
using UnityEngine;

/// <summary>
/// ProjectTasks model compatible with existing database schema
/// No restrictions - existing schema is preserved
/// Fully compatible with database schema
/// </summary>
public class ProjectTasks
{
    [PrimaryKey, AutoIncrement]
    public int id { get; set; }

    public int projectId { get; set; }
    public string title { get; set; }
    public string description { get; set; }
    public string status { get; set; } = "ToDo";
    public string createdDate { get; set; }

    // REMOVED: modifiedDate column does not exist in database!
    // public string modifiedDate { get; set; }

    // Parameterless constructor
    public ProjectTasks()
    {
    }

    // Convenience constructor
    public ProjectTasks(int projectId, string title, string description = "", string status = "ToDo")
    {
        this.projectId = projectId;
        this.title = title?.Trim() ?? "";
        this.description = description?.Trim() ?? "";
        this.status = status;
        this.createdDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        // modifiedDate not used - only createdDate is available
    }

    // Validation method
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(title) &&
               projectId > 0 &&
               IsValidStatus(status);
    }

    public static bool IsValidStatus(string status)
    {
        return status == "ToDo" || status == "InProgress" || status == "Done";
    }

    // DISABLED: modifiedDate column does not exist in database
    public void MarkAsModified()
    {
        // modifiedDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        // modifiedDate column does not exist in database, only createdDate is available
        Debug.Log("ModifiedDate column does not exist in database - update skipped");
    }

    public override string ToString()
    {
        return $"Task[{id}]: {title} ({status})";
    }
}