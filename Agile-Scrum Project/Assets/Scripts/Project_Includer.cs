using SQLite4Unity3d;
using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Project_Includer : MonoBehaviour
{
    public TMP_InputField projectNameInput;
    public TMP_InputField projectDescriptionInput;

    public DB_Manager dbManager; // 👈 Bu sahnede atanacak

    private SQLiteConnection _connection;

    void Start()
    {
        string dbName = "NETAS-DATAS.db";
        string dbPath = Path.Combine(Application.streamingAssetsPath, dbName);
        _connection = new SQLiteConnection(dbPath, SQLiteOpenFlags.ReadWrite);
        Debug.Log("✅ Veritabanına bağlandı: " + dbPath);
    }

    public void InsertProject()
    {
        string projectName = projectNameInput.text;
        string projectDescription = projectDescriptionInput.text;
        string currentDate = DateTime.Now.ToString("dd/MM/yyyy");

        Project_Info_Data newProject = new Project_Info_Data
        {
            Name = projectName,
            Description = projectDescription,
            Created_Date = currentDate
        };

        _connection.Insert(newProject);
        Debug.Log("✅ Yeni proje veritabanına eklendi: " + projectName);
        projectNameInput.text = "";
        projectDescriptionInput.text = "";
        dbManager.ClearUI();      // 👈 Doğru çağrı
        dbManager.LoadDataToUI(); // 👈 Doğru çağrı
    }
    public void UpdateSelectedProject()
    {
        if (dbManager == null || dbManager.selectedProjectId == -1)
        {
            Debug.LogWarning("❌ Güncelleme işlemi yapılamaz: Seçili proje yok.");
            return;
        }

        string updatedName = projectNameInput.text;
        string updatedDescription = projectDescriptionInput.text;

        // Veritabanından projeyi al
        var project = _connection.Table<Project_Info_Data>()
                                 .FirstOrDefault(p => p.ID == dbManager.selectedProjectId);

        if (project != null)
        {
            project.Name = updatedName;
            project.Description = updatedDescription;

            _connection.Update(project);
            Debug.Log($"🔄 Proje güncellendi. ID: {project.ID}, Yeni İsim: {updatedName}");

            // UI'yi güncelle
            projectNameInput.text = "";
            projectDescriptionInput.text = "";
            dbManager.ClearUI();
            dbManager.LoadDataToUI();
        }
        else
        {
            Debug.LogWarning("⚠️ Güncellenmek istenen proje veritabanında bulunamadı.");
        }
    }
    public void DeleteSelectedProject()
    {
        if (dbManager.selectedProjectId == -1)
        {
            Debug.LogWarning("❌ Hiçbir proje seçilmedi, silme işlemi yapılamaz.");
            return;
        }

        // Önce o ID’ye sahip projeyi bul
        var projectToDelete = _connection.Table<Project_Info_Data>()
                                         .FirstOrDefault(p => p.ID == dbManager.selectedProjectId);

        if (projectToDelete != null)
        {
            int deletedRows = _connection.Delete(projectToDelete);
            Debug.Log("🗑️ Proje silindi. Silinen satır sayısı: " + deletedRows);
        }
        else
        {
            Debug.LogWarning("❌ Silinmek istenen proje veritabanında bulunamadı.");
        }

        // UI'yı güncelle
        dbManager.selectedProjectId = -1;
        dbManager.ClearUI();
        dbManager.LoadDataToUI();
        dbManager.ClearTasks();
    }
}
