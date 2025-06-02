using SQLite4Unity3d;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ScreenshotManager : MonoBehaviour
{
    public DB_Manager dbManager;
    public Button screenshotButton;

    void Start()
    {
        if (screenshotButton != null)
            screenshotButton.onClick.AddListener(TakeScreenshot);
    }

    public void TakeScreenshot()
    {
        if (dbManager.selectedProjectId == -1)
        {
            Debug.LogWarning("❌ Ekran görüntüsü alınamadı: Seçili proje yok.");
            return;
        }

        StartCoroutine(CaptureScreenshot());
    }

    IEnumerator CaptureScreenshot()
    {
        // Bir frame bekle ki UI tam render olsun
        yield return new WaitForEndOfFrame();

        // Proje bilgilerini al
        var connection = GetDatabaseConnection();
        var project = connection.Table<Project_Info_Data>()
                               .FirstOrDefault(p => p.ID == dbManager.selectedProjectId);

        if (project != null)
        {
            string fileName = $"{project.Name}_{System.DateTime.Now:yyyyMMdd_HHmmss}.png";
            string path = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop), "NETAS_Screenshots");

            // Klasör yoksa oluştur
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            string fullPath = Path.Combine(path, fileName);

            // Ekran görüntüsü al
            ScreenCapture.CaptureScreenshot(fullPath);
            Debug.Log($"📸 Ekran görüntüsü kaydedildi: {fullPath}");
        }
    }

    private SQLiteConnection GetDatabaseConnection()
    {
        string dbName = "NETAS-DATAS.db";
        string dbPath = Path.Combine(Application.streamingAssetsPath, dbName);
        return new SQLiteConnection(dbPath, SQLiteOpenFlags.ReadWrite);
    }
}