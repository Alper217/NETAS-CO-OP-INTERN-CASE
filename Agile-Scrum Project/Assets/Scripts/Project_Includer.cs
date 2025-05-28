using SQLite4Unity3d;
using System;
using System.IO;
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
}
