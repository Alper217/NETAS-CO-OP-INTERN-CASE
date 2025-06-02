using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using SQLite4Unity3d;
using System.IO;
using System.Linq;
using System.Collections.Generic;

[System.Serializable]
public class OpenAIRequest
{
    public string model;
    public OpenAIMessage[] messages;
    public float temperature;
    public int max_tokens;
}

[System.Serializable]
public class OpenAIMessage
{
    public string role;
    public string content;
}

[System.Serializable]
public class OpenAIResponse
{
    public OpenAIChoice[] choices;
    public OpenAIError error;
}

[System.Serializable]
public class OpenAIChoice
{
    public OpenAIMessage message;
}

[System.Serializable]
public class OpenAIError
{
    public string message;
    public string type;
    public string code;
}

public class ProjectAnalyzer : MonoBehaviour
{
    [Header("OpenAI Settings")]
    [SerializeField] private string openAIApiKey = "YOUR_API_KEY_HERE";
    private const string OPENAI_URL = "https://api.openai.com/v1/chat/completions";

    [Header("References")]
    public DB_Manager dbManager;
    public Analysis_Result_Panel analysisResultPanel;

    private SQLiteConnection _connection;
    private DateTime lastApiCall = DateTime.MinValue;
    private const float MIN_API_INTERVAL = 3f; // API çağrıları arasında minimum 3 saniye bekle

    void Start()
    {
        InitializeDatabase();
    }

    void InitializeDatabase()
    {
        string dbName = "NETAS-DATAS.db";
        string dbPath = Path.Combine(Application.streamingAssetsPath, dbName);
        _connection = new SQLiteConnection(dbPath, SQLiteOpenFlags.ReadWrite);
        Debug.Log("🔗 ProjectAnalyzer veritabanına bağlandı: " + dbPath);
    }

    public void AnalyzeCurrentProject()
    {
        if (dbManager.selectedProjectId == -1)
        {
            Debug.LogWarning("⚠️ Analiz yapılamadı: Seçili proje yok.");
            if (analysisResultPanel != null)
            {
                analysisResultPanel.ShowResult("⚠️ Lütfen önce bir proje seçin!");
            }
            return;
        }

        if (string.IsNullOrEmpty(openAIApiKey) || openAIApiKey == "YOUR_API_KEY_HERE")
        {
            Debug.LogError("❌ OpenAI API key ayarlanmamış!");
            if (analysisResultPanel != null)
            {
                analysisResultPanel.ShowResult("❌ OpenAI API key ayarlanmamış!");
            }
            return;
        }

        // API çağrıları arasında yeterli süre geçmiş mi kontrol et
        float timeSinceLastCall = (float)(DateTime.Now - lastApiCall).TotalSeconds;
        if (timeSinceLastCall < MIN_API_INTERVAL)
        {
            float waitTime = MIN_API_INTERVAL - timeSinceLastCall;
            Debug.LogWarning($"⏳ API sınırı için {waitTime:F1} saniye bekleniyor...");
            if (analysisResultPanel != null)
            {
                analysisResultPanel.ShowResult($"⏳ Lütfen {waitTime:F1} saniye bekleyin ve tekrar deneyin.");
            }
            return;
        }

        StartCoroutine(PerformAnalysis());
    }

    private IEnumerator PerformAnalysis()
    {
        if (analysisResultPanel != null)
            analysisResultPanel.ShowLoading();

        // API çağrısı zamanını kaydet
        lastApiCall = DateTime.Now;

        string projectData = CollectProjectData();

        // Proje verisini kontrol et - çok az veri varsa basit analiz yap
        if (IsProjectDataMinimal(projectData))
        {
            string simpleAnalysis = GenerateSimpleAnalysis(projectData);
            analysisResultPanel?.ShowResult(simpleAnalysis);
            yield break;
        }

        string analysisPrompt = CreateAnalysisPrompt(projectData);

        OpenAIRequest request = new OpenAIRequest
        {
            model = "gpt-3.5-turbo",
            messages = new OpenAIMessage[]
            {
                new OpenAIMessage
                {
                    role = "user",
                    content = analysisPrompt
                }
            },
            temperature = 0.7f,
            max_tokens = 800 // Token sayısını azalttık
        };

        string jsonRequest = JsonUtility.ToJson(request);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequest);

        int maxRetries = 2; // Retry sayısını azalttık
        int retryDelay = 5; // İlk bekleme süresini artırdık

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            Debug.Log($"🔄 API çağrısı yapılıyor... (Deneme: {attempt}/{maxRetries})");

            using (UnityWebRequest www = new UnityWebRequest(OPENAI_URL, "POST"))
            {
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");
                www.SetRequestHeader("Authorization", "Bearer " + openAIApiKey);

                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        OpenAIResponse response = JsonUtility.FromJson<OpenAIResponse>(www.downloadHandler.text);

                        if (response.error != null)
                        {
                            Debug.LogError($"❌ OpenAI API Error: {response.error.message}");
                            analysisResultPanel?.ShowResult($"❌ API Hatası: {response.error.message}");
                            yield break;
                        }

                        if (response.choices != null && response.choices.Length > 0)
                        {
                            string analysisResult = response.choices[0].message.content;
                            Debug.Log("✅ Analiz tamamlandı!");
                            analysisResultPanel?.ShowResult(analysisResult);
                            yield break;
                        }
                        else
                        {
                            Debug.LogError("❌ API yanıtında veri bulunamadı");
                            analysisResultPanel?.ShowResult("❌ Analiz sonucu alınamadı.");
                            yield break;
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("❌ JSON parse hatası: " + e.Message);
                        Debug.LogError("Response: " + www.downloadHandler.text);
                        analysisResultPanel?.ShowResult("⚠️ Analiz sonucu işlenirken hata oluştu.");
                        yield break;
                    }
                }
                else if (www.responseCode == 429)
                {
                    Debug.LogWarning($"⚠️ API sınırı aşıldı (429), {retryDelay} saniye sonra tekrar denenecek... (Deneme: {attempt}/{maxRetries})");

                    if (attempt < maxRetries)
                    {
                        yield return new WaitForSeconds(retryDelay);
                        retryDelay *= 2; // exponential backoff
                    }
                    else
                    {
                        analysisResultPanel?.ShowResult("❌ API sınırı aşıldı. Lütfen birkaç dakika bekleyip tekrar deneyin.");
                        yield break;
                    }
                }
                else if (www.responseCode == 401)
                {
                    Debug.LogError("❌ API Key geçersiz veya yetkisiz erişim");
                    analysisResultPanel?.ShowResult("❌ API Key hatası. Lütfen API key'inizi kontrol edin.");
                    yield break;
                }
                else
                {
                    Debug.LogError($"❌ OpenAI API hatası ({www.responseCode}): " + www.error);
                    Debug.LogError("Response: " + www.downloadHandler.text);
                    analysisResultPanel?.ShowResult($"⚠️ Analiz sırasında hata oluştu: {www.error}");
                    yield break;
                }
            }
        }
    }

    private bool IsProjectDataMinimal(string projectData)
    {
        if (_connection == null)
        {
            InitializeDatabase();
        }

        // Görev sayılarını direkt veritabanından kontrol et
        var allTasks = _connection.Table<Project_Tasks>()
                                 .Where(t => t.projectId == dbManager.selectedProjectId)
                                 .ToList();

        var todoTasks = allTasks.Where(t => t.status == "ToDo").Count();
        var inProgressTasks = allTasks.Where(t => t.status == "InProgress").Count();
        var doneTasks = allTasks.Where(t => t.status == "Done").Count();

        int totalTasks = allTasks.Count;

        Debug.Log($"📊 Görev Dağılımı - Todo: {todoTasks}, InProgress: {inProgressTasks}, Done: {doneTasks}, Toplam: {totalTasks}");

        // Eğer toplam görev 3 ve altındaysa VE çoğunlukla boşsa minimal say
        if (totalTasks <= 3)
        {
            return true;
        }

        // Eğer hiç görev yoksa minimal
        if (totalTasks == 0)
        {
            return true;
        }

        // Eğer sadece 1-2 tane tek kelimelik görev varsa minimal say
        if (totalTasks <= 2)
        {
            var taskTitles = allTasks.Select(t => t.title?.Trim() ?? "").ToList();
            bool hasOnlySimpleTasks = taskTitles.All(title =>
                string.IsNullOrEmpty(title) || title.Split(' ').Length <= 2);

            if (hasOnlySimpleTasks)
            {
                return true;
            }
        }

        // Diğer durumlarda (4+ görev) tam analiz yap
        return false;
    }

    private string GenerateSimpleAnalysis(string projectData)
    {
        if (_connection == null)
        {
            InitializeDatabase();
        }

        // Görev istatistiklerini al
        var allTasks = _connection.Table<Project_Tasks>()
                                 .Where(t => t.projectId == dbManager.selectedProjectId)
                                 .ToList();

        var todoCount = allTasks.Count(t => t.status == "ToDo");
        var inProgressCount = allTasks.Count(t => t.status == "InProgress");
        var doneCount = allTasks.Count(t => t.status == "Done");
        var totalCount = allTasks.Count;

        StringBuilder analysis = new StringBuilder();
        analysis.AppendLine("📊 **Basit Proje Analizi**");
        analysis.AppendLine();
        analysis.AppendLine($"🔢 **Görev Dağılımı:** {totalCount} toplam görev");
        analysis.AppendLine($"   • ⏳ Yapılacak: {todoCount}");
        analysis.AppendLine($"   • 🔄 Devam Eden: {inProgressCount}");
        analysis.AppendLine($"   • ✅ Tamamlanan: {doneCount}");
        analysis.AppendLine();

        if (totalCount == 0)
        {
            analysis.AppendLine("🔍 **Durum:** Proje henüz başlamamış");
            analysis.AppendLine();
            analysis.AppendLine("💡 **Öneriler:**");
            analysis.AppendLine("• Projeniz için görevler eklemeye başlayın");
            analysis.AppendLine("• Projenizi küçük, yönetilebilir görevlere bölün");
            analysis.AppendLine("• İlk olarak en önemli görevleri belirleyin");
        }
        else if (totalCount <= 3)
        {
            analysis.AppendLine("🔍 **Durum:** Küçük ölçekli proje");
            analysis.AppendLine();
            analysis.AppendLine("💡 **Öneriler:**");
            if (todoCount > 0)
            {
                analysis.AppendLine("• Yapılacak görevlere öncelik verin");
            }
            if (inProgressCount > 1)
            {
                analysis.AppendLine("• Aynı anda çok fazla göreve odaklanmayın");
            }
            if (doneCount > 0)
            {
                analysis.AppendLine($"• Harika! {doneCount} görev tamamlandı");
            }
            analysis.AppendLine("• Proje büyüdükçe daha fazla görev ekleyebilirsiniz");
        }

        // İlerleme yüzdesi hesapla
        if (totalCount > 0)
        {
            float progressPercent = (float)doneCount / totalCount * 100;
            analysis.AppendLine();
            analysis.AppendLine($"📈 **İlerleme:** %{progressPercent:F0} tamamlandı");

            if (progressPercent == 0)
            {
                analysis.AppendLine("• Projeye başlamak için ilk görevi seçin!");
            }
            else if (progressPercent < 30)
            {
                analysis.AppendLine("• Proje yeni başlamış, devam edin!");
            }
            else if (progressPercent < 70)
            {
                analysis.AppendLine("• İyi ilerleme kaydediyorsunuz!");
            }
            else if (progressPercent < 100)
            {
                analysis.AppendLine("• Son sprint! Bitiş çizgisine yaklaştınız!");
            }
            else
            {
                analysis.AppendLine("• 🎉 Tebrikler! Proje tamamlandı!");
            }
        }

        analysis.AppendLine();
        analysis.AppendLine("ℹ️ *Daha detaylı AI analizi için 4+ görev ekleyin.*");

        return analysis.ToString();
    }

    private string CollectProjectData()
    {
        if (_connection == null)
        {
            InitializeDatabase();
        }

        // Proje bilgilerini al
        var project = _connection.Table<Project_Info_Data>()
                                .FirstOrDefault(p => p.ID == dbManager.selectedProjectId);

        if (project == null)
        {
            return "Proje bulunamadı.";
        }

        // Görevleri al ve kategorilere ayır
        var allTasks = _connection.Table<Project_Tasks>()
                                 .Where(t => t.projectId == dbManager.selectedProjectId)
                                 .ToList();

        var todoTasks = allTasks.Where(t => t.status == "ToDo").ToList();
        var inProgressTasks = allTasks.Where(t => t.status == "InProgress").ToList();
        var doneTasks = allTasks.Where(t => t.status == "Done").ToList();

        // Veri string'ini oluştur
        StringBuilder dataBuilder = new StringBuilder();
        dataBuilder.AppendLine($"Proje Adı: {project.Name}");
        dataBuilder.AppendLine($"Proje Açıklaması: {project.Description}");
        dataBuilder.AppendLine($"Toplam Görev: {allTasks.Count}");
        dataBuilder.AppendLine();

        dataBuilder.AppendLine("=== YAPILACAKLAR (ToDo) ===");
        if (todoTasks.Count > 0)
        {
            int limit = 8; // Limiti düşürdük
            foreach (var task in todoTasks.Take(limit))
            {
                string description = string.IsNullOrEmpty(task.description) ? "" :
                    $" - {task.description.Substring(0, Math.Min(task.description.Length, 50))}...";
                dataBuilder.AppendLine($"• {task.title}{description}");
            }
            if (todoTasks.Count > limit)
            {
                dataBuilder.AppendLine($"• ...ve {todoTasks.Count - limit} görev daha");
            }
        }
        else
        {
            dataBuilder.AppendLine("• Henüz yapılacak görev yok");
        }

        dataBuilder.AppendLine();
        dataBuilder.AppendLine("=== DEVAM EDENLER (InProgress) ===");
        if (inProgressTasks.Count > 0)
        {
            foreach (var task in inProgressTasks.Take(5)) // Limit ekledik
            {
                dataBuilder.AppendLine($"• {task.title}");
            }
            if (inProgressTasks.Count > 5)
            {
                dataBuilder.AppendLine($"• ...ve {inProgressTasks.Count - 5} görev daha");
            }
        }
        else
        {
            dataBuilder.AppendLine("• Şu anda devam eden görev yok");
        }

        dataBuilder.AppendLine();
        dataBuilder.AppendLine("=== TAMAMLANANLAR (Done) ===");
        dataBuilder.AppendLine($"• Toplam {doneTasks.Count} görev tamamlandı");

        return dataBuilder.ToString();
    }

    private string CreateAnalysisPrompt(string projectData)
    {
        return $@"Proje analizi yap:

{projectData}

Kısa ve öz öneriler ver:
1. Öncelikli görevler
2. Sıralama önerisi  
3. Genel durum
4. İyileştirme önerileri

Maksimum 500 kelime, Türkçe cevap ver.";
    }
}