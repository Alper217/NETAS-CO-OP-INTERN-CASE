using SQLite4Unity3d;

public class Project_Info_Data
{
    [PrimaryKey, AutoIncrement]
    public int ID { get; set; }

    [NotNull]
    public string Name { get; set; }

    public string Description { get; set; }

    [NotNull, Column("Created Date")]
    public string Created_Date { get; set; } 
}
