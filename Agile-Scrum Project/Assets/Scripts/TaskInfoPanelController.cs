using SQLite4Unity3d;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;

public class TaskInfoPanelController : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI taskNameText;
    public TextMeshProUGUI taskCreatedDateText;
    public TextMeshProUGUI taskExplanationText;

    private SQLiteConnection _connection;

    void Start()
    {
        InitializeDatabase();
    }

    void InitializeDatabase()
    {
        string dbName = "NETAS-DATAS.db";
        string dbPath = Path.Combine(Application.streamingAssetsPath, dbName);
        _connection = new SQLiteConnection(dbPath, SQLiteOpenFlags.ReadWrite);
        Debug.Log("✅ TaskInfoPanel veritabanına bağlandı: " + dbPath);
    }

    public void LoadTaskInfo(int taskId)
    {
        // Eğer connection henüz kurulmadıysa kur
        if (_connection == null)
        {
            InitializeDatabase();
        }

        if (taskId == -1)
        {
            Debug.LogWarning("⚠️ Task bilgileri yüklenemedi: Geçersiz task ID");
            ClearTaskInfo();
            return;
        }

        // Veritabanından task bilgilerini çek
        var task = _connection.Table<Project_Tasks>()
                             .FirstOrDefault(t => t.id == taskId);

        if (task != null)
        {
            // UI elementlerine bilgileri yazdır
            if (taskNameText != null)
                taskNameText.text = task.title;

            if (taskCreatedDateText != null)
                taskCreatedDateText.text = task.createdDate;

            if (taskExplanationText != null)
                taskExplanationText.text = task.description;

            Debug.Log($"✅ Task bilgileri yüklendi. ID: {taskId}, Başlık: {task.title}");
        }
        else
        {
            Debug.LogWarning($"❌ Task bulunamadı. ID: {taskId}");
            ClearTaskInfo();
        }
    }

    private void ClearTaskInfo()
    {
        if (taskNameText != null)
            taskNameText.text = "Task bulunamadı";

        if (taskCreatedDateText != null)
            taskCreatedDateText.text = "";

        if (taskExplanationText != null)
            taskExplanationText.text = "";
    }
}