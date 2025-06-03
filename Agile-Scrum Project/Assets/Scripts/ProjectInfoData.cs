using SQLite4Unity3d;
using System;

/// <summary>
/// ProjectInfoData model compatible with existing database schema
/// No restrictions - retains existing scheme
/// </summary>
public class ProjectInfoData
{
    [PrimaryKey, AutoIncrement]
    public int ID { get; set; }

    public string Name { get; set; }
    public string Description { get; set; }
    public string Created_Date { get; set; }
    public string Modified_Date { get; set; }

    // Parameterless constructor
    public ProjectInfoData()
    {
    }

    // Convenience constructor
    public ProjectInfoData(string name, string description = "")
    {
        Name = name?.Trim() ?? "";
        Description = description?.Trim() ?? "";
        Created_Date = DateTime.Now.ToString("dd/MM/yyyy");
        Modified_Date = Created_Date;
    }

    // Validation
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Name);
    }

    // Update modification date
    public void MarkAsModified()
    {
        Modified_Date = DateTime.Now.ToString("dd/MM/yyyy");
    }

    public override string ToString()
    {
        return $"Project[{ID}]: {Name}";
    }
}