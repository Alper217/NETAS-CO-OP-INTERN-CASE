using SQLite4Unity3d;
using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Task_Includer : MonoBehaviour
{
    public TMP_InputField taskTitleInput;
    public TMP_InputField taskDescriptionInput;

    public DB_Manager dbManager; // 👈 Sahneden atanacak

    private SQLiteConnection _connection;
    private int selectedTaskId = -1;

    void Start()
    {
        string dbName = "NETAS-DATAS.db";
        string dbPath = Path.Combine(Application.streamingAssetsPath, dbName);
        _connection = new SQLiteConnection(dbPath, SQLiteOpenFlags.ReadWrite);
        Debug.Log("✅ Veritabanına bağlandı (Görevler): " + dbPath);
    }

    public void SetSelectedTaskId(int taskId)
    {
        selectedTaskId = taskId;
        Debug.Log($"🎯 Seçilen görev ID: {taskId}");

        var task = _connection.Table<Project_Tasks>().FirstOrDefault(t => t.id == taskId);
        if (task != null)
        {
            taskTitleInput.text = task.title;
            taskDescriptionInput.text = task.description;
        }
    }

    public void InsertTask()
    {
        if (dbManager.selectedProjectId == -1)
        {
            Debug.LogWarning("❌ Görev eklenemedi: Seçili proje yok.");
            return;
        }

        string title = taskTitleInput.text;
        string description = taskDescriptionInput.text;
        string currentDate = DateTime.Now.ToString("dd/MM/yyyy");

        Project_Tasks newTask = new Project_Tasks
        {
            projectId = dbManager.selectedProjectId,
            title = title,
            description = description,
            status = "ToDo",
            createdDate = currentDate
        };

        _connection.Insert(newTask);
        Debug.Log($"✅ Yeni görev eklendi: {title}");

        taskTitleInput.text = "";
        taskDescriptionInput.text = "";

        dbManager.ClearTasks();
        dbManager.LoadTaskUI();
    }

    public void UpdateSelectedTask()
    {
        if (selectedTaskId == -1)
        {
            Debug.LogWarning("❌ Görev güncellenemedi: Seçili görev yok.");
            return;
        }

        var task = _connection.Table<Project_Tasks>().FirstOrDefault(t => t.id == selectedTaskId);

        if (task != null)
        {
            task.title = taskTitleInput.text;
            task.description = taskDescriptionInput.text;

            _connection.Update(task);
            Debug.Log($"📝 Görev güncellendi: ID {selectedTaskId}");

            taskTitleInput.text = "";
            taskDescriptionInput.text = "";
            selectedTaskId = -1; // Seçimi temizle

            dbManager.ClearTasks();
            dbManager.LoadTaskUI();
        }
        else
        {
            Debug.LogWarning("❌ Güncellenmek istenen görev bulunamadı.");
        }
    }

    public void DeleteSelectedTask()
    {
        if (selectedTaskId == -1)
        {
            Debug.LogWarning("❌ Görev silinemedi: Seçili görev yok.");
            return;
        }

        var taskToDelete = _connection.Table<Project_Tasks>().FirstOrDefault(t => t.id == selectedTaskId);

        if (taskToDelete != null)
        {
            int deleted = _connection.Delete(taskToDelete);
            Debug.Log($"🗑️ Görev silindi. ID: {selectedTaskId}, Silinen satır: {deleted}");

            taskTitleInput.text = "";
            taskDescriptionInput.text = "";
            selectedTaskId = -1; // Seçimi temizle

            dbManager.ClearTasks();
            dbManager.LoadTaskUI();
        }
        else
        {
            Debug.LogWarning("❌ Silinmek istenen görev bulunamadı.");
        }
    }

    public int GetSelectedTaskId()
    {
        return selectedTaskId;
    }
}