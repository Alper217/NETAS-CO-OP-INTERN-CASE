using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using System.Collections.Generic;

// Claude API data structures
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
/// Optimized project analyzer using Claude API
/// Performs project analysis using Anthropic Claude API
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

    // Cache system
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
            Debug.LogWarning("Claude API key not configured! Only simple analysis will work.");
        }
    }

    public void AnalyzeCurrentProject()
    {
        if (isAnalyzing)
        {
            Debug.LogWarning("Analysis already in progress...");
            return;
        }

        if (projectManager.SelectedProjectId == -1)
        {
            ShowError("Please select a project first!");
            return;
        }

        // Cache check
        string cacheKey = GenerateCacheKey();
        if (_analysisCache.TryGetValue(cacheKey, out var cachedResult))
        {
            if ((DateTime.Now - cachedResult.timestamp).TotalHours < CACHE_VALIDITY_HOURS)
            {
                Debug.Log("Retrieving analysis result from cache...");
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

            // Minimal data check
            if (IsProjectDataMinimal(projectData))
            {
                string simpleAnalysis = GenerateSimpleAnalysis(projectData);
                analysisResultPanel?.ShowResult(simpleAnalysis);
                yield break;
            }

            // API Key check
            if (string.IsNullOrEmpty(claudeApiKey) || claudeApiKey == "YOUR_CLAUDE_API_KEY_HERE")
            {
                string offlineAnalysis = GenerateOfflineAnalysis(projectData);
                analysisResultPanel?.ShowResult(offlineAnalysis);
                yield break;
            }

            // API rate limit check
            float timeSinceLastCall = (float)(DateTime.Now - lastApiCall).TotalSeconds;
            if (timeSinceLastCall < MIN_API_INTERVAL)
            {
                float waitTime = MIN_API_INTERVAL - timeSinceLastCall;
                analysisResultPanel?.ShowResult($"Waiting {waitTime:F1} seconds for API rate limit...");
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

            // Claude API specific headers
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("x-api-key", claudeApiKey);
            www.SetRequestHeader("anthropic-version", CLAUDE_VERSION);

            // Add timeout
            www.timeout = 30;

            Debug.Log("Sending request to Claude API...");
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
            Debug.Log($"Claude API Response: {responseText}");

            var response = JsonUtility.FromJson<ClaudeResponse>(responseText);

            // Debug: Check response structure
            Debug.Log($"Response type: {response.type}");
            Debug.Log($"Response role: {response.role}");
            Debug.Log($"Content count: {response.content?.Length}");
            Debug.Log($"Error: {response.error?.message}");

            // If error exists and message is not empty, show error
            if (response.error != null && !string.IsNullOrEmpty(response.error.message))
            {
                ShowError($"Claude API Error: {response.error.message}");
                return;
            }

            if (response.content?.Length > 0)
            {
                // Check first content
                var firstContent = response.content[0];
                Debug.Log($"First content type: {firstContent.type}");
                Debug.Log($"First content text: {firstContent.text?.Substring(0, Math.Min(100, firstContent.text?.Length ?? 0))}...");

                if (firstContent.type == "text" && !string.IsNullOrEmpty(firstContent.text))
                {
                    string result = firstContent.text;

                    // Save to cache
                    string cacheKey = GenerateCacheKey();
                    _analysisCache[cacheKey] = (result, DateTime.Now);

                    analysisResultPanel?.ShowResult(result);

                    // Open analysis panel and fix scroll
                    var uiController = FindObjectOfType<UIController>();
                    if (uiController != null)
                    {
                        uiController.OpenAnalysisPanel();
                    }

                    Debug.Log("Claude analysis completed successfully!");
                }
                else
                {
                    ShowError($"Invalid content type: {firstContent.type}");
                }
            }
            else
            {
                ShowError("No content found in Claude API response");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Claude JSON parse error: {ex.Message}");
            Debug.LogError($"Response: {responseText}");
            ShowError("Error occurred while processing analysis result.");
        }
    }

    private void HandleAPIError(UnityWebRequest www)
    {
        Debug.LogError($"Claude API Error: {www.error}");
        Debug.LogError($"Response Code: {www.responseCode}");
        Debug.LogError($"Response: {www.downloadHandler.text}");

        switch (www.responseCode)
        {
            case 429:
                ShowError("Claude API rate limit exceeded. Please wait a few minutes.");
                break;
            case 401:
                ShowError("Invalid Claude API Key. Please check your API key.");
                break;
            case 400:
                ShowError("Invalid Claude API request format.");
                break;
            default:
                ShowError($"Claude API error ({www.responseCode}): {www.error}");
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
        analysis.AppendLine("**Simple Project Analysis**\n");

        analysis.AppendLine($"**Task Distribution:** {data.Tasks.Count} total tasks");
        analysis.AppendLine($"   • To Do: {data.TodoTasks.Count}");
        analysis.AppendLine($"   • In Progress: {data.InProgressTasks.Count}");
        analysis.AppendLine($"   • Completed: {data.DoneTasks.Count}\n");

        if (data.Tasks.Count == 0)
        {
            analysis.AppendLine("**Status:** Project hasn't started yet\n");
            analysis.AppendLine("**Recommendations:**");
            analysis.AppendLine("• Start adding tasks to your project");
            analysis.AppendLine("• Break your project into small, manageable tasks");
            analysis.AppendLine("• Identify the most important tasks first");
        }
        else
        {
            float progressPercent = data.Tasks.Count > 0 ? (float)data.DoneTasks.Count / data.Tasks.Count * 100 : 0;
            analysis.AppendLine($"**Progress:** {progressPercent:F0}% completed\n");

            analysis.AppendLine("**Recommendations:**");
            if (data.TodoTasks.Count > 0) analysis.AppendLine("• Prioritize your To Do tasks");
            if (data.InProgressTasks.Count > 1) analysis.AppendLine("• Don't focus on too many tasks at once");
            if (data.DoneTasks.Count > 0) analysis.AppendLine($"• Great! {data.DoneTasks.Count} tasks completed");
        }

        analysis.AppendLine("\n*Add 4+ tasks for detailed Claude analysis.*");
        return analysis.ToString();
    }

    private string GenerateOfflineAnalysis(ProjectAnalysisData data)
    {
        var analysis = new StringBuilder();
        analysis.AppendLine("**Project Analysis (Offline)**\n");

        analysis.AppendLine($"**Project:** {data.Project?.Name ?? "Unknown"}");
        analysis.AppendLine($"**Created:** {data.Project?.Created_Date ?? "Unknown"}\n");

        analysis.AppendLine($"**Task Statistics:**");
        analysis.AppendLine($"• Total: {data.Tasks.Count}");
        analysis.AppendLine($"• ToDo: {data.TodoTasks.Count}");
        analysis.AppendLine($"• InProgress: {data.InProgressTasks.Count}");
        analysis.AppendLine($"• Done: {data.DoneTasks.Count}\n");

        // Progress analysis
        if (data.Tasks.Count > 0)
        {
            float completionRate = (float)data.DoneTasks.Count / data.Tasks.Count;
            analysis.AppendLine("**Status Analysis:**");

            if (completionRate == 0)
                analysis.AppendLine("• Project is in initial stage");
            else if (completionRate < 0.3f)
                analysis.AppendLine("• Project is in early stage, good progress");
            else if (completionRate < 0.7f)
                analysis.AppendLine("• Project is in middle stage, steady progress");
            else if (completionRate < 1.0f)
                analysis.AppendLine("• Project is in final stage, near completion");
            else
                analysis.AppendLine("• Project completed!");
        }

        analysis.AppendLine("\n**Recommendations:**");
        if (data.InProgressTasks.Count > 3)
            analysis.AppendLine("• Too many active tasks, increase focus");
        if (data.TodoTasks.Count > data.InProgressTasks.Count * 3)
            analysis.AppendLine("• Too many pending tasks, prioritize them");
        if (data.Tasks.Count > 0 && data.DoneTasks.Count == 0)
            analysis.AppendLine("• Complete the first task to gain momentum");

        analysis.AppendLine("\n*Claude analysis requires API key.*");
        return analysis.ToString();
    }

    private string CreateOptimizedPrompt(ProjectAnalysisData data)
    {
        var prompt = new StringBuilder();
        prompt.AppendLine($"Project Analysis Request:");
        prompt.AppendLine($"Project Name: {data.Project?.Name}");
        prompt.AppendLine($"Description: {data.Project?.Description}");
        prompt.AppendLine($"Total Tasks: {data.Tasks.Count}\n");

        prompt.AppendLine("=== TO DO TASKS ===");
        if (data.TodoTasks.Count > 0)
        {
            foreach (var task in data.TodoTasks.Take(5))
            {
                string desc = !string.IsNullOrEmpty(task.description) && task.description.Length > 30
                    ? task.description.Substring(0, 30) + "..."
                    : task.description;
                prompt.AppendLine($"• {task.title}");
                if (!string.IsNullOrEmpty(desc)) prompt.AppendLine($"  Description: {desc}");
            }
            if (data.TodoTasks.Count > 5)
                prompt.AppendLine($"• +{data.TodoTasks.Count - 5} more tasks");
        }
        else
        {
            prompt.AppendLine("• No pending tasks");
        }

        prompt.AppendLine("\n=== IN PROGRESS TASKS ===");
        if (data.InProgressTasks.Count > 0)
        {
            foreach (var task in data.InProgressTasks.Take(3))
            {
                prompt.AppendLine($"• {task.title}");
            }
        }
        else
        {
            prompt.AppendLine("• No tasks in progress");
        }

        prompt.AppendLine($"\n=== COMPLETED TASKS ===");
        prompt.AppendLine($"• {data.DoneTasks.Count} tasks completed");

        prompt.AppendLine("\nPlease provide a comprehensive analysis for this project. Address the following topics:");
        prompt.AppendLine("1. Priority tasks and recommendations");
        prompt.AppendLine("2. Project progress status and estimates");
        prompt.AppendLine("3. Risk analysis and potential blockers");
        prompt.AppendLine("4. Improvement suggestions and strategies");
        prompt.AppendLine("5. Overall assessment and conclusion");
        prompt.AppendLine("\nProvide your response in English and format it in markdown. Maximum 800 words.");

        return prompt.ToString();
    }

    private void ShowError(string message)
    {
        Debug.LogWarning(message);
        analysisResultPanel?.ShowResult(message);

        // Open analysis panel and fix scroll
        var uiController = FindObjectOfType<UIController>();
        if (uiController != null)
        {
            uiController.OpenAnalysisPanel();
        }
    }
}

/// <summary>
/// Data structure for project analysis
/// </summary>
public class ProjectAnalysisData
{
    public ProjectInfoData Project { get; set; }
    public List<ProjectTasks> Tasks { get; set; } = new List<ProjectTasks>();
    public List<ProjectTasks> TodoTasks { get; set; } = new List<ProjectTasks>();
    public List<ProjectTasks> InProgressTasks { get; set; } = new List<ProjectTasks>();
    public List<ProjectTasks> DoneTasks { get; set; } = new List<ProjectTasks>();
}