using SQLite4Unity3d;
using System;
using UnityEngine;

/// <summary>
/// Mevcut veritabanı şemasıyla uyumlu ProjectTasks modeli
/// Kısıtlama yok - mevcut şema korunuyor
/// ✅ Veritabanı şeması ile tamamen uyumlu hale getirildi
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

    // ❌ KALDIRILDI: Veritabanında modifiedDate kolonu yok!
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
        // modifiedDate yok - sadece createdDate kullanılıyor
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

    // ❌ KALDIRILDI: Veritabanında modifiedDate kolonu olmadığı için devre dışı
    public void MarkAsModified()
    {
        // modifiedDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        // Veritabanında modifiedDate kolonu yok, sadece createdDate var
        Debug.Log("⚠️ ModifiedDate kolonu veritabanında yok - güncelleme atlandı");
    }

    public override string ToString()
    {
        return $"Task[{id}]: {title} ({status})";
    }
}