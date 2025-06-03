using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using System.Collections.Generic;

// ✨ Claude API için yeni data structures
[System.Serializable]
public class ClaudeRequest
{
    public string model;
    public int max_tokens;
    public ClaudeMessage[] messages;
}

[System.Serializable]
public class ClaudeMessage
{
    public string role;
    public string content;
}

[System.Serializable]
public class ClaudeResponse
{
    public string id;
    public string type;
    public string role;
    public ClaudeContent[] content;
    public ClaudeError error;
}

[System.Serializable]
public class ClaudeContent
{
    public string type;
    public string text;
}

[System.Serializable]
public class ClaudeError
{
    public string type;
    public string message;
}

/// <summary>
/// Claude API ile optimize edilmiş proje analizörü
/// Anthropic Claude API kullanarak proje analizi yapar
/// </summary>
public class ProjectAnalyzer : MonoBehaviour
{
    [Header("Claude API Settings")]
    [SerializeField] private string claudeApiKey = "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";
    private const string CLAUDE_URL = "https://api.anthropic.com/v1/messages";
    private const string CLAUDE_VERSION = "2023-06-01";

    [Header("References")]
    public ProjectManager projectManager;
    public AnalysisResultPanel analysisResultPanel;

    private DateTime lastApiCall = DateTime.MinValue;
    private const float MIN_API_INTERVAL = 3f;
    private bool isAnalyzing = false;

    // Cache sistemi
    private readonly Dictionary<string, (string result, DateTime timestamp)> _analysisCache =
        new Dictionary<string, (string, DateTime)>();
    private const float CACHE_VALIDITY_HOURS = 1f;

    private void Start()
    {
        ValidateApiKey();
    }

    private void ValidateApiKey()
    {
        if (string.IsNullOrEmpty(claudeApiKey) || claudeApiKey == "YOUR_CLAUDE_API_KEY_HERE")
        {
            Debug.LogWarning("⚠️ Claude API key ayarlanmamış! Sadece basit analiz çalışacak.");
        }
    }

    public void AnalyzeCurrentProject()
    {
        if (isAnalyzing)
        {
            Debug.LogWarning("⏳ Analiz zaten devam ediyor...");
            return;
        }

        if (projectManager.SelectedProjectId == -1)
        {
            ShowError("⚠️ Lütfen önce bir proje seçin!");
            return;
        }

        // Cache kontrolü
        string cacheKey = GenerateCacheKey();
        if (_analysisCache.TryGetValue(cacheKey, out var cachedResult))
        {
            if ((DateTime.Now - cachedResult.timestamp).TotalHours < CACHE_VALIDITY_HOURS)
            {
                Debug.Log("📋 Cache'den analiz sonucu getiriliyor...");
                analysisResultPanel?.ShowResult(cachedResult.result);
                return;
            }
            else
            {
                _analysisCache.Remove(cacheKey);
            }
        }

        StartCoroutine(PerformAnalysisCoroutine());
    }

    private string GenerateCacheKey()
    {
        var tasks = DatabaseManager.Instance.GetTasksByProjectId(projectManager.SelectedProjectId);
        string tasksSignature = string.Join(",", tasks.Select(t => $"{t.id}:{t.status}:{t.title.GetHashCode()}"));
        return $"{projectManager.SelectedProjectId}:{tasksSignature.GetHashCode()}";
    }

    private IEnumerator PerformAnalysisCoroutine()
    {
        isAnalyzing = true;
        analysisResultPanel?.ShowLoading();

        try
        {
            var projectData = CollectProjectData();

            // Minimal veri kontrolü
            if (IsProjectDataMinimal(projectData))
            {
                string simpleAnalysis = GenerateSimpleAnalysis(projectData);
                analysisResultPanel?.ShowResult(simpleAnalysis);
                yield break;
            }

            // API Key kontrolü
            if (string.IsNullOrEmpty(claudeApiKey) || claudeApiKey == "YOUR_CLAUDE_API_KEY_HERE")
            {
                string offlineAnalysis = GenerateOfflineAnalysis(projectData);
                analysisResultPanel?.ShowResult(offlineAnalysis);
                yield break;
            }

            // API rate limit kontrolü
            float timeSinceLastCall = (float)(DateTime.Now - lastApiCall).TotalSeconds;
            if (timeSinceLastCall < MIN_API_INTERVAL)
            {
                float waitTime = MIN_API_INTERVAL - timeSinceLastCall;
                analysisResultPanel?.ShowResult($"⏳ API sınırı için {waitTime:F1} saniye bekleniyor...");
                yield return new WaitForSeconds(waitTime);
            }

            yield return StartCoroutine(CallClaudeAPI(projectData));
        }
        finally
        {
            isAnalyzing = false;
        }
    }

    private IEnumerator CallClaudeAPI(ProjectAnalysisData projectData)
    {
        lastApiCall = DateTime.Now;

        string analysisPrompt = CreateOptimizedPrompt(projectData);

        var request = new ClaudeRequest
        {
            model = "claude-3-5-sonnet-20241022",
            max_tokens = 1024,
            messages = new ClaudeMessage[]
            {
                new ClaudeMessage { role = "user", content = analysisPrompt }
            }
        };

        string jsonRequest = JsonUtility.ToJson(request);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequest);

        using (var www = new UnityWebRequest(CLAUDE_URL, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();

            // ✨ Claude API için özel header'lar
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("x-api-key", claudeApiKey);
            www.SetRequestHeader("anthropic-version", CLAUDE_VERSION);

            // Timeout ekle
            www.timeout = 30;

            Debug.Log("🤖 Claude API'ye istek gönderiliyor...");
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                HandleSuccessfulResponse(www.downloadHandler.text);
            }
            else
            {
                HandleAPIError(www);
            }
        }
    }

    private void HandleSuccessfulResponse(string responseText)
    {
        try
        {
            Debug.Log($"🔍 Claude API Response: {responseText}");

            var response = JsonUtility.FromJson<ClaudeResponse>(responseText);

            // ✅ Debug: Response yapısını kontrol et
            Debug.Log($"🔍 Response type: {response.type}");
            Debug.Log($"🔍 Response role: {response.role}");
            Debug.Log($"🔍 Content count: {response.content?.Length}");
            Debug.Log($"🔍 Error: {response.error?.message}");

            // Eğer error varsa ve message boş değilse hata göster
            if (response.error != null && !string.IsNullOrEmpty(response.error.message))
            {
                ShowError($"❌ Claude API Hatası: {response.error.message}");
                return;
            }

            if (response.content?.Length > 0)
            {
                // İlk content'i kontrol et
                var firstContent = response.content[0];
                Debug.Log($"🔍 First content type: {firstContent.type}");
                Debug.Log($"🔍 First content text: {firstContent.text?.Substring(0, Math.Min(100, firstContent.text?.Length ?? 0))}...");

                if (firstContent.type == "text" && !string.IsNullOrEmpty(firstContent.text))
                {
                    string result = firstContent.text;

                    // Cache'e kaydet
                    string cacheKey = GenerateCacheKey();
                    _analysisCache[cacheKey] = (result, DateTime.Now);

                    analysisResultPanel?.ShowResult(result);

                    // ✨ Analysis panelini aç ve scroll'u düzelt
                    var uiController = FindObjectOfType<UIController>();
                    if (uiController != null)
                    {
                        uiController.OpenAnalysisPanel();
                    }

                    Debug.Log("✅ Claude analizi tamamlandı!");
                }
                else
                {
                    ShowError($"❌ Content type hatalı: {firstContent.type}");
                }
            }
            else
            {
                ShowError("❌ Claude API yanıtında content bulunamadı");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Claude JSON parse hatası: {ex.Message}");
            Debug.LogError($"❌ Response: {responseText}");
            ShowError("⚠️ Analiz sonucu işlenirken hata oluştu.");
        }
    }

    private void HandleAPIError(UnityWebRequest www)
    {
        Debug.LogError($"❌ Claude API Error: {www.error}");
        Debug.LogError($"❌ Response Code: {www.responseCode}");
        Debug.LogError($"❌ Response: {www.downloadHandler.text}");

        switch (www.responseCode)
        {
            case 429:
                ShowError("❌ Claude API sınırı aşıldı. Lütfen birkaç dakika bekleyin.");
                break;
            case 401:
                ShowError("❌ Claude API Key geçersiz. Lütfen API key'inizi kontrol edin.");
                break;
            case 400:
                ShowError("❌ Claude API istek formatı hatalı.");
                break;
            default:
                ShowError($"⚠️ Claude API hatası ({www.responseCode}): {www.error}");
                break;
        }
    }

    private ProjectAnalysisData CollectProjectData()
    {
        var project = DatabaseManager.Instance.GetById<ProjectInfoData>(projectManager.SelectedProjectId);
        var tasks = DatabaseManager.Instance.GetTasksByProjectId(projectManager.SelectedProjectId);

        return new ProjectAnalysisData
        {
            Project = project,
            Tasks = tasks,
            TodoTasks = tasks.Where(t => t.status == "ToDo").ToList(),
            InProgressTasks = tasks.Where(t => t.status == "InProgress").ToList(),
            DoneTasks = tasks.Where(t => t.status == "Done").ToList()
        };
    }

    private bool IsProjectDataMinimal(ProjectAnalysisData data)
    {
        return data.Tasks.Count <= 3 ||
               data.Tasks.Count == 0 ||
               (data.Tasks.Count <= 2 && data.Tasks.All(t =>
                   string.IsNullOrEmpty(t.title) || t.title.Split(' ').Length <= 2));
    }

    private string GenerateSimpleAnalysis(ProjectAnalysisData data)
    {
        var analysis = new StringBuilder();
        analysis.AppendLine("📊 **Basit Proje Analizi**\n");

        analysis.AppendLine($"🔢 **Görev Dağılımı:** {data.Tasks.Count} toplam görev");
        analysis.AppendLine($"   • ⏳ Yapılacak: {data.TodoTasks.Count}");
        analysis.AppendLine($"   • 🔄 Devam Eden: {data.InProgressTasks.Count}");
        analysis.AppendLine($"   • ✅ Tamamlanan: {data.DoneTasks.Count}\n");

        if (data.Tasks.Count == 0)
        {
            analysis.AppendLine("🔍 **Durum:** Proje henüz başlamamış\n");
            analysis.AppendLine("💡 **Öneriler:**");
            analysis.AppendLine("• Projeniz için görevler eklemeye başlayın");
            analysis.AppendLine("• Projenizi küçük, yönetilebilir görevlere bölün");
            analysis.AppendLine("• İlk olarak en önemli görevleri belirleyin");
        }
        else
        {
            float progressPercent = data.Tasks.Count > 0 ? (float)data.DoneTasks.Count / data.Tasks.Count * 100 : 0;
            analysis.AppendLine($"📈 **İlerleme:** %{progressPercent:F0} tamamlandı\n");

            analysis.AppendLine("💡 **Öneriler:**");
            if (data.TodoTasks.Count > 0) analysis.AppendLine("• Yapılacak görevlere öncelik verin");
            if (data.InProgressTasks.Count > 1) analysis.AppendLine("• Aynı anda çok fazla göreve odaklanmayın");
            if (data.DoneTasks.Count > 0) analysis.AppendLine($"• Harika! {data.DoneTasks.Count} görev tamamlandı");
        }

        analysis.AppendLine("\nℹ️ *Daha detaylı Claude analizi için 4+ görev ekleyin.*");
        return analysis.ToString();
    }

    private string GenerateOfflineAnalysis(ProjectAnalysisData data)
    {
        var analysis = new StringBuilder();
        analysis.AppendLine("🤖 **Proje Analizi (Offline)**\n");

        analysis.AppendLine($"📋 **Proje:** {data.Project?.Name ?? "Bilinmeyen"}");
        analysis.AppendLine($"📅 **Oluşturulma:** {data.Project?.Created_Date ?? "Bilinmeyen"}\n");

        analysis.AppendLine($"📊 **Görev İstatistikleri:**");
        analysis.AppendLine($"• Toplam: {data.Tasks.Count}");
        analysis.AppendLine($"• ToDo: {data.TodoTasks.Count}");
        analysis.AppendLine($"• InProgress: {data.InProgressTasks.Count}");
        analysis.AppendLine($"• Done: {data.DoneTasks.Count}\n");

        // İlerleme analizi
        if (data.Tasks.Count > 0)
        {
            float completionRate = (float)data.DoneTasks.Count / data.Tasks.Count;
            analysis.AppendLine("🎯 **Durum Analizi:**");

            if (completionRate == 0)
                analysis.AppendLine("• Proje başlangıç aşamasında");
            else if (completionRate < 0.3f)
                analysis.AppendLine("• Proje erken aşamada, iyi ilerleme");
            else if (completionRate < 0.7f)
                analysis.AppendLine("• Proje orta aşamada, düzenli ilerleme");
            else if (completionRate < 1.0f)
                analysis.AppendLine("• Proje son aşamada, tamamlanmaya yakın");
            else
                analysis.AppendLine("• 🎉 Proje tamamlandı!");
        }

        analysis.AppendLine("\n💡 **Öneriler:**");
        if (data.InProgressTasks.Count > 3)
            analysis.AppendLine("• Aynı anda çok fazla görev aktif, odaklanmayı artırın");
        if (data.TodoTasks.Count > data.InProgressTasks.Count * 3)
            analysis.AppendLine("• Çok fazla bekleyen görev var, önceliklendirin");
        if (data.Tasks.Count > 0 && data.DoneTasks.Count == 0)
            analysis.AppendLine("• İlk görevi tamamlayarak momentum kazanın");

        analysis.AppendLine("\nℹ️ *Claude analizi için API key gerekli.*");
        return analysis.ToString();
    }

    private string CreateOptimizedPrompt(ProjectAnalysisData data)
    {
        var prompt = new StringBuilder();
        prompt.AppendLine($"Proje Analizi İsteği:");
        prompt.AppendLine($"Proje Adı: {data.Project?.Name}");
        prompt.AppendLine($"Açıklama: {data.Project?.Description}");
        prompt.AppendLine($"Toplam Görev: {data.Tasks.Count}\n");

        prompt.AppendLine("=== YAPILACAK GÖREVLER ===");
        if (data.TodoTasks.Count > 0)
        {
            foreach (var task in data.TodoTasks.Take(5))
            {
                string desc = !string.IsNullOrEmpty(task.description) && task.description.Length > 30
                    ? task.description.Substring(0, 30) + "..."
                    : task.description;
                prompt.AppendLine($"• {task.title}");
                if (!string.IsNullOrEmpty(desc)) prompt.AppendLine($"  Açıklama: {desc}");
            }
            if (data.TodoTasks.Count > 5)
                prompt.AppendLine($"• +{data.TodoTasks.Count - 5} görev daha");
        }
        else
        {
            prompt.AppendLine("• Yapılacak görev yok");
        }

        prompt.AppendLine("\n=== DEVAM EDEN GÖREVLER ===");
        if (data.InProgressTasks.Count > 0)
        {
            foreach (var task in data.InProgressTasks.Take(3))
            {
                prompt.AppendLine($"• {task.title}");
            }
        }
        else
        {
            prompt.AppendLine("• Devam eden görev yok");
        }

        prompt.AppendLine($"\n=== TAMAMLANAN GÖREVLER ===");
        prompt.AppendLine($"• {data.DoneTasks.Count} görev tamamlandı");

        prompt.AppendLine("\nLütfen bu proje için kapsamlı bir analiz yap. Aşağıdaki konuları ele al:");
        prompt.AppendLine("1. 🎯 Öncelikli görevler ve öneriler");
        prompt.AppendLine("2. 📈 Proje ilerleme durumu ve tahminler");
        prompt.AppendLine("3. ⚠️ Risk analizi ve potansiyel engelleyiciler");
        prompt.AppendLine("4. 💡 İyileştirme önerileri ve stratejiler");
        prompt.AppendLine("5. 📊 Genel değerlendirme ve sonuç");
        prompt.AppendLine("\nCevabını Türkçe ver ve markdown formatında düzenle. Maksimum 800 kelime.");

        return prompt.ToString();
    }

    private void ShowError(string message)
    {
        Debug.LogWarning(message);
        analysisResultPanel?.ShowResult(message);

        // ✨ Analysis panelini aç ve scroll'u düzelt
        var uiController = FindObjectOfType<UIController>();
        if (uiController != null)
        {
            uiController.OpenAnalysisPanel();
        }
    }
}

/// <summary>
/// Proje analizi için veri yapısı
/// </summary>
public class ProjectAnalysisData
{
    public ProjectInfoData Project { get; set; }
    public List<ProjectTasks> Tasks { get; set; } = new List<ProjectTasks>();
    public List<ProjectTasks> TodoTasks { get; set; } = new List<ProjectTasks>();
    public List<ProjectTasks> InProgressTasks { get; set; } = new List<ProjectTasks>();
    public List<ProjectTasks> DoneTasks { get; set; } = new List<ProjectTasks>();
}