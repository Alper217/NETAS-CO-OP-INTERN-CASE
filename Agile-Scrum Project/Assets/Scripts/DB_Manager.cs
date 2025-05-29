using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using SQLite4Unity3d;
using TMPro;
using System.Linq;

public class DB_Manager : MonoBehaviour
{
    public GameObject itemPrefab;            // Proje kart prefabı
    public RectTransform contentParent;      // Scroll alanı (projeler için)
    public GameObject taskItemPrefab;        // Görev (task) kartı prefabı (sadece ad yazar)
    public Transform todoParent;             // "ToDo" görevleri için parent
    public Transform inProgressParent;       // "InProgress" görevleri için parent
    public Transform doneParent;             // "Done" görevleri için parent

    public float itemSpacing = 5f;
    public float itemWidth = 160f;

    private SQLiteConnection _connection;
    public int selectedProjectId = -1;

    // Task_Includer referansı eklendi
    public Task_Includer taskIncluder;

    void Start()
    {
        InitializeDatabase();
        LoadDataToUI();
    }

    void InitializeDatabase()
    {
        string dbName = "NETAS-DATAS.db";
        string dbPath = Path.Combine(Application.streamingAssetsPath, dbName);
        _connection = new SQLiteConnection(dbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);
        Debug.Log("✅ Veritabanına bağlandı: " + dbPath);
    }

    public void LoadDataToUI()
    {
        var projects = _connection.Table<Project_Info_Data>().ToList();
        Debug.Log("Toplam proje sayısı: " + projects.Count);

        for (int i = 0; i < projects.Count; i++)
        {
            var project = projects[i];

            // Prefab oluştur
            GameObject item = Instantiate(itemPrefab, contentParent);
            RectTransform rt = item.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(i * (itemWidth + itemSpacing), 0);

            // ProjectCardUI scripti üzerinden verileri aktar
            ProjectCardUI cardUI = item.GetComponent<ProjectCardUI>();
            if (cardUI != null)
            {
                cardUI.SetData(project.ID, project.Name, project.Created_Date, project.Description);
            }

            // Tıklama olayını ata
            Button btn = item.GetComponent<Button>();
            if (btn != null)
            {
                int capturedID = project.ID; // Closure problemi yaşamamak için
                btn.onClick.AddListener(() => OnProjectCardClicked(capturedID));
            }
        }

        // Scroll içeriğini genişlet
        float totalWidth = projects.Count * (itemWidth + itemSpacing);
        contentParent.sizeDelta = new Vector2(totalWidth, contentParent.sizeDelta.y);
    }

    public void LoadTaskUI()
    {
        if (selectedProjectId == -1)
        {
            Debug.LogWarning("⚠️ Görevler yüklenemedi: Seçili proje yok.");
            return;
        }

        ClearTasks(); // Önce eski görevleri temizle

        var tasks = _connection.Table<Project_Tasks>()
                               .Where(t => t.projectId == selectedProjectId)
                               .ToList();

        foreach (var task in tasks)
        {
            GameObject taskItem = Instantiate(taskItemPrefab);

            // TaskCardUI component'ini kontrol et ve set et
            TaskCardUI taskCardUI = taskItem.GetComponent<TaskCardUI>();
            if (taskCardUI != null)
            {
                taskCardUI.SetData(task.id, task.projectId, task.title, task.description, task.status);
            }
            else
            {
                // Eğer TaskCardUI yoksa sadece text'i set et (eski sistem)
                taskItem.GetComponentInChildren<TextMeshProUGUI>().text = task.title;
            }

            // Görev kartına tıklama olayı ekle
            Button taskBtn = taskItem.GetComponent<Button>();
            if (taskBtn != null)
            {
                int capturedTaskId = task.id;
                taskBtn.onClick.AddListener(() => OnTaskCardClicked(capturedTaskId));
            }

            switch (task.status)
            {
                case "ToDo":
                    taskItem.transform.SetParent(todoParent, false);
                    break;
                case "InProgress":
                    taskItem.transform.SetParent(inProgressParent, false);
                    break;
                case "Done":
                    taskItem.transform.SetParent(doneParent, false);
                    break;
                default:
                    Debug.LogWarning("❗ Bilinmeyen görev durumu: " + task.status);
                    break;
            }
        }
    }

    public void ClearUI()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
    }

    void OnProjectCardClicked(int projectId)
    {
        Debug.Log("🟢 Tıklanan proje ID: " + projectId);
        selectedProjectId = projectId;
        LoadTaskUI(); // LoadTaskUI metodunu kullan
    }

    void OnTaskCardClicked(int taskId)
    {
        Debug.Log("🎯 Tıklanan görev ID: " + taskId);

        // Task_Includer'a seçili görev ID'sini gönder
        if (taskIncluder != null)
        {
            taskIncluder.SetSelectedTaskId(taskId);
        }
        else
        {
            Debug.LogWarning("⚠️ Task_Includer referansı atanmamış!");
        }
    }

    public void ClearTasks()
    {
        foreach (Transform child in todoParent)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in inProgressParent)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in doneParent)
        {
            Destroy(child.gameObject);
        }
    }
}