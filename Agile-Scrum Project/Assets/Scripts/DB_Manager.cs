using System.Collections.Generic;
using System.IO;
using UnityEngine;
using SQLite4Unity3d;
using TMPro;
using System.Linq;

public class DB_Manager : MonoBehaviour
{
    public GameObject itemPrefab;         // Her proje için kullanılacak prefab
    public RectTransform contentParent;   // Prefab'ların içinde bulunduğu yatay alan
    public float itemSpacing = 5f;        // Kartlar arası boşluk
    public float itemWidth = 160f;        // Kart genişliği

    private SQLiteConnection _connection;

    void Start()
    {
        InitializeDatabase();
        LoadDataToUI();
    }

    void InitializeDatabase()
    {
        string dbName = "NETAS-DATAS.db";
        string dbPath = Path.Combine(Application.streamingAssetsPath, dbName);
        _connection = new SQLiteConnection(dbPath, SQLiteOpenFlags.ReadOnly);
        Debug.Log("✅ Veritabanına bağlandı: " + dbPath);
    }

    void LoadDataToUI()
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

            // ProjectCardUI scriptini bul ve verileri gönder
            ProjectCardUI cardUI = item.GetComponent<ProjectCardUI>();
            if (cardUI != null)
            {
                cardUI.SetData(project.Name, project.Created_Date, project.Description);
            }
            else
            {
                Debug.LogWarning("❗ Prefab'ta ProjectCardUI scripti eksik!");
            }
        }

        // Content genişliğini ayarla (scroll için)
        float totalWidth = projects.Count * (itemWidth + itemSpacing);
        contentParent.sizeDelta = new Vector2(totalWidth, contentParent.sizeDelta.y);
    }
}
