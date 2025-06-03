using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Tüm proje işlemlerini yöneten optimize edilmiş sınıf
/// Proje CRUD, Task yönetimi ve UI güncellemelerini içerir
/// LoadSelectedProjectToInputs metodu eklendi
/// ✨ Çift tık desteği eklendi - tek tık sadece seçer, çift tık info paneli açar
/// </summary>
public class ProjectManager : MonoBehaviour
{
    [Header("Project UI")]
    public TMP_InputField projectNameInput;
    public TMP_InputField projectDescriptionInput;
    public GameObject projectCardPrefab;
    public RectTransform projectContentParent;

    [Header("Task UI")]
    public TMP_InputField taskTitleInput;
    public TMP_InputField taskDescriptionInput;
    public GameObject taskCardPrefab;
    public Transform todoParent;
    public Transform inProgressParent;
    public Transform doneParent;

    [Header("Info Panel")]
    public TextMeshProUGUI taskNameText;
    public TextMeshProUGUI taskCreatedDateText;
    public TextMeshProUGUI taskExplanationText;

    [Header("Layout Settings")]
    public float itemSpacing = 5f;
    public float itemWidth = 160f;

    // Events
    public System.Action<int> OnProjectSelected;
    public System.Action<int> OnTaskSelected;

    // Private fields
    private int _selectedProjectId = -1;
    private int _selectedTaskId = -1;
    private readonly Dictionary<int, GameObject> _projectCards = new Dictionary<int, GameObject>();
    private readonly Dictionary<int, GameObject> _taskCards = new Dictionary<int, GameObject>();

    public int SelectedProjectId => _selectedProjectId;
    public int SelectedTaskId => _selectedTaskId;

    private void Start()
    {
        LoadAllProjects();
    }

    #region Project Management

    public void CreateProject()
    {
        if (string.IsNullOrWhiteSpace(projectNameInput.text))
        {
            Debug.LogWarning("❌ Proje adı boş olamaz!");
            return;
        }

        var newProject = new ProjectInfoData
        {
            Name = projectNameInput.text.Trim(),
            Description = projectDescriptionInput.text?.Trim() ?? "",
            Created_Date = DateTime.Now.ToString("dd/MM/yyyy")
        };

        try
        {
            DatabaseManager.Instance.Insert(newProject);
            Debug.Log($"✅ Yeni proje oluşturuldu: {newProject.Name}");

            ClearProjectInputs();
            LoadAllProjects();
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Proje oluşturma hatası: {ex.Message}");
        }
    }

    public void UpdateSelectedProject()
    {
        if (_selectedProjectId == -1)
        {
            Debug.LogWarning("❌ Güncellemek için bir proje seçin!");
            return;
        }

        var project = DatabaseManager.Instance.GetById<ProjectInfoData>(_selectedProjectId);
        if (project != null)
        {
            project.Name = projectNameInput.text.Trim();
            project.Description = projectDescriptionInput.text?.Trim() ?? "";
            project.MarkAsModified(); // Değişiklik tarihini güncelle

            DatabaseManager.Instance.UpdateItem(project);
            Debug.Log($"✅ Proje güncellendi: {project.Name}");

            ClearProjectInputs();
            LoadAllProjects();
        }
    }

    public void DeleteSelectedProject()
    {
        if (_selectedProjectId == -1)
        {
            Debug.LogWarning("❌ Silmek için bir proje seçin!");
            return;
        }

        try
        {
            DatabaseManager.Instance.ExecuteTransaction(() =>
            {
                // Önce projenin tüm taskları sil
                var tasks = DatabaseManager.Instance.GetTasksByProjectId(_selectedProjectId);
                foreach (var task in tasks)
                {
                    DatabaseManager.Instance.Delete(task);
                }

                // Sonra projeyi sil
                var project = DatabaseManager.Instance.GetById<ProjectInfoData>(_selectedProjectId);
                if (project != null)
                {
                    DatabaseManager.Instance.Delete(project);
                }
            });

            Debug.Log("✅ Proje ve tüm taskları silindi");
            _selectedProjectId = -1;
            ClearProjectInputs();
            LoadAllProjects();
            ClearTasks();
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Proje silme hatası: {ex.Message}");
        }
    }

    // ✨ YENİ METOT: Seçili projeyi inputlara yükler
    public void LoadSelectedProjectToInputs()
    {
        if (_selectedProjectId == -1)
        {
            Debug.LogWarning("❌ Yüklemek için bir proje seçin!");
            return;
        }

        var project = DatabaseManager.Instance.GetById<ProjectInfoData>(_selectedProjectId);
        if (project != null)
        {
            projectNameInput.text = project.Name;
            projectDescriptionInput.text = project.Description;
            Debug.Log($"📝 Proje bilgileri inputlara yüklendi: {project.Name}");
        }
        else
        {
            Debug.LogWarning($"❌ Proje bulunamadı: {_selectedProjectId}");
        }
    }

    private void LoadAllProjects()
    {
        ClearProjectCards();

        var projects = DatabaseManager.Instance.GetAll<ProjectInfoData>();
        Debug.Log($"📂 {projects.Count} proje yükleniyor...");

        for (int i = 0; i < projects.Count; i++)
        {
            CreateProjectCard(projects[i], i);
        }

        UpdateProjectScrollArea(projects.Count);
        Debug.Log($"✅ {projects.Count} proje kartı oluşturuldu");
    }

    private void CreateProjectCard(ProjectInfoData project, int index)
    {
        if (projectCardPrefab == null)
        {
            Debug.LogError("❌ Project Card Prefab atanmamış!");
            return;
        }

        if (projectContentParent == null)
        {
            Debug.LogError("❌ Project Content Parent atanmamış!");
            return;
        }

        GameObject card = Instantiate(projectCardPrefab, projectContentParent);

        // Position set
        RectTransform rt = card.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(index * (itemWidth + itemSpacing), 0);

        // Data set
        var cardUI = card.GetComponent<ProjectCardUI>();
        if (cardUI != null)
        {
            cardUI.SetData(project.ID, project.Name, project.Created_Date, project.Description);
            Debug.Log($"📋 Proje kartı oluşturuldu: {project.Name} (ID: {project.ID})");
        }
        else
        {
            Debug.LogError("❌ ProjectCardUI component bulunamadı!");
        }

        // Click event
        var button = card.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => SelectProject(project.ID));
        }
        else
        {
            Debug.LogWarning("⚠️ Project Card'da Button component bulunamadı!");
        }

        _projectCards[project.ID] = card;
    }

    private void UpdateProjectScrollArea(int projectCount)
    {
        if (projectContentParent != null)
        {
            float totalWidth = projectCount * (itemWidth + itemSpacing);
            projectContentParent.sizeDelta = new Vector2(totalWidth, projectContentParent.sizeDelta.y);
            Debug.Log($"📏 Scroll area genişliği güncellendi: {totalWidth}");
        }
    }

    private void ClearProjectCards()
    {
        foreach (var card in _projectCards.Values)
        {
            if (card != null) Destroy(card);
        }
        _projectCards.Clear();
    }

    private void ClearProjectInputs()
    {
        if (projectNameInput != null) projectNameInput.text = "";
        if (projectDescriptionInput != null) projectDescriptionInput.text = "";
    }

    private void SelectProject(int projectId)
    {
        _selectedProjectId = projectId;
        _selectedTaskId = -1; // Task seçimini temizle

        Debug.Log($"🎯 Proje seçildi: {projectId}");

        LoadProjectTasks();
        OnProjectSelected?.Invoke(projectId);
    }

    #endregion

    #region Task Management

    public void CreateTask()
    {
        if (_selectedProjectId == -1)
        {
            Debug.LogWarning("❌ Task eklemek için önce bir proje seçin!");
            return;
        }

        if (string.IsNullOrWhiteSpace(taskTitleInput.text))
        {
            Debug.LogWarning("❌ Task başlığı boş olamaz!");
            return;
        }

        var newTask = new ProjectTasks
        {
            projectId = _selectedProjectId,
            title = taskTitleInput.text.Trim(),
            description = taskDescriptionInput.text?.Trim() ?? "",
            status = "ToDo",
            createdDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm")
        };

        try
        {
            DatabaseManager.Instance.Insert(newTask);
            Debug.Log($"✅ Yeni task oluşturuldu: {newTask.title}");

            ClearTaskInputs();
            LoadProjectTasks();
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Task oluşturma hatası: {ex.Message}");
        }
    }

    public void UpdateSelectedTask()
    {
        if (_selectedTaskId == -1)
        {
            Debug.LogWarning("❌ Güncellemek için bir task seçin!");
            return;
        }

        var task = DatabaseManager.Instance.GetById<ProjectTasks>(_selectedTaskId);
        if (task != null)
        {
            task.title = taskTitleInput.text.Trim();
            task.description = taskDescriptionInput.text?.Trim() ?? "";
            task.MarkAsModified(); // Değişiklik tarihini güncelle

            DatabaseManager.Instance.UpdateItem(task);
            Debug.Log($"✅ Task güncellendi: {task.title}");

            ClearTaskInputs();
            LoadProjectTasks();
            ClearTaskSelection();
        }
    }

    public void DeleteSelectedTask()
    {
        if (_selectedTaskId == -1)
        {
            Debug.LogWarning("❌ Silmek için bir task seçin!");
            return;
        }

        var task = DatabaseManager.Instance.GetById<ProjectTasks>(_selectedTaskId);
        if (task != null)
        {
            DatabaseManager.Instance.Delete(task);
            Debug.Log($"✅ Task silindi: {task.title}");

            ClearTaskInputs();
            LoadProjectTasks();
            ClearTaskSelection();
        }
    }

    // Status değiştirme metodu - tek metod yeterli
    public void ChangeTaskStatus(string newStatus)
    {
        if (_selectedTaskId == -1)
        {
            Debug.LogWarning("❌ Durum değiştirmek için bir task seçin!");
            return;
        }

        var task = DatabaseManager.Instance.GetById<ProjectTasks>(_selectedTaskId);
        if (task != null)
        {
            string oldStatus = task.status;
            task.status = newStatus;
            task.MarkAsModified(); // Değişiklik tarihini güncelle

            DatabaseManager.Instance.UpdateItem(task);

            Debug.Log($"✅ Task durumu güncellendi: {oldStatus} → {newStatus}");
            LoadProjectTasks();
        }
    }

    #region Task Selection Methods - ✨ Çift Tık Desteği

    // ✨ YENİ METOT: Sadece task seçer, panel açmaz (tek tık için)
    public void SelectTaskOnly(int taskId)
    {
        _selectedTaskId = taskId;
        Debug.Log($"🎯 Task seçildi (sadece): {taskId}");

        LoadTaskToInputs(taskId);
        LoadTaskInfoPanel(taskId); // Info panel verilerini yükle ama paneli açma
        OnTaskSelected?.Invoke(taskId);

        // Panel açma yok - sadece seçim
    }

    // ✨ YENİ METOT: Task seçer VE info panelini açar (çift tık için)
    public void SelectTaskAndOpenInfo(int taskId)
    {
        _selectedTaskId = taskId;
        Debug.Log($"🎯 Task seçildi + Info paneli açıldı: {taskId}");

        LoadTaskToInputs(taskId);
        LoadTaskInfoPanel(taskId);
        OpenTaskInfoPanel();
        OnTaskSelected?.Invoke(taskId);
    }

    // ⚠️ DEĞİŞTİRİLDİ: Eski SelectTask metodu - artık otomatik panel açmıyor
    private void SelectTask(int taskId)
    {
        _selectedTaskId = taskId;
        Debug.Log($"🎯 Task seçildi: {taskId}");

        LoadTaskToInputs(taskId);
        LoadTaskInfoPanel(taskId);
        // OpenTaskInfoPanel(); // ❌ KALDIRILDI - Artık otomatik panel açılmayacak
        OnTaskSelected?.Invoke(taskId);
    }

    // ✨ YENİ METOT: Manuel olarak info paneli açmak için (UI butonundan çağrılabilir)
    public void OpenSelectedTaskInfo()
    {
        if (_selectedTaskId != -1)
        {
            LoadTaskInfoPanel(_selectedTaskId);
            OpenTaskInfoPanel();
            Debug.Log($"📋 Task info paneli manuel açıldı: {_selectedTaskId}");
        }
        else
        {
            Debug.LogWarning("❌ Info paneli açmak için önce bir task seçin!");
        }
    }

    #endregion

    private void LoadProjectTasks()
    {
        if (_selectedProjectId == -1) return;

        ClearTasks();

        var tasks = DatabaseManager.Instance.GetTasksByProjectId(_selectedProjectId);
        Debug.Log($"📋 {tasks.Count} task yükleniyor...");

        foreach (var task in tasks)
        {
            CreateTaskCard(task);
        }
    }

    private void CreateTaskCard(ProjectTasks task)
    {
        if (taskCardPrefab == null)
        {
            Debug.LogError("❌ Task Card Prefab atanmamış!");
            return;
        }

        GameObject taskCard = Instantiate(taskCardPrefab);

        var cardUI = taskCard.GetComponent<TaskCardUI>();
        if (cardUI != null)
        {
            cardUI.SetData(task.id, task.projectId, task.title, task.description, task.status);
            Debug.Log($"📋 Task kartı oluşturuldu: {task.title} ({task.status})");
        }
        else
        {
            Debug.LogError("❌ TaskCardUI component bulunamadı!");
        }

        // ⚠️ DİKKAT: Button event artık TaskCardUI içinde çift tık sistemi ile yönetiliyor
        // Burada manuel onClick eklemeyin, TaskCardUI kendi kendine hallediyor

        // Parent'a ekle
        Transform parent = GetTaskParent(task.status);
        if (parent != null)
        {
            taskCard.transform.SetParent(parent, false);
        }
        else
        {
            Debug.LogError($"❌ Task parent bulunamadı: {task.status}");
        }

        _taskCards[task.id] = taskCard;
    }

    private Transform GetTaskParent(string status)
    {
        return status switch
        {
            "ToDo" => todoParent,
            "InProgress" => inProgressParent,
            "Done" => doneParent,
            _ => todoParent
        };
    }

    public void ClearTasks()
    {
        foreach (var taskCard in _taskCards.Values)
        {
            if (taskCard != null) Destroy(taskCard);
        }
        _taskCards.Clear();

        // Parent'ları temizle
        ClearParent(todoParent);
        ClearParent(inProgressParent);
        ClearParent(doneParent);
    }

    private void ClearParent(Transform parent)
    {
        if (parent != null)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Destroy(parent.GetChild(i).gameObject);
            }
        }
    }

    private void ClearTaskInputs()
    {
        if (taskTitleInput != null) taskTitleInput.text = "";
        if (taskDescriptionInput != null) taskDescriptionInput.text = "";
    }

    private void LoadTaskToInputs(int taskId)
    {
        var task = DatabaseManager.Instance.GetById<ProjectTasks>(taskId);
        if (task != null)
        {
            if (taskTitleInput != null) taskTitleInput.text = task.title;
            if (taskDescriptionInput != null) taskDescriptionInput.text = task.description;
        }
    }

    private void LoadTaskInfoPanel(int taskId)
    {
        var task = DatabaseManager.Instance.GetById<ProjectTasks>(taskId);
        if (task != null)
        {
            if (taskNameText != null) taskNameText.text = task.title;
            if (taskCreatedDateText != null) taskCreatedDateText.text = task.createdDate;
            if (taskExplanationText != null) taskExplanationText.text = task.description;
        }
    }

    // ✨ YENİ METOT: Info panelini açar (UIController'dan çağrılacak)
    public void OpenTaskInfoPanel()
    {
        // UIController'daki OpenTaskInfoPanel metodunu çağır
        var uiController = FindObjectOfType<UIController>();
        if (uiController != null)
        {
            uiController.OpenTaskInfoPanel();
        }
    }

    private void ClearTaskSelection()
    {
        _selectedTaskId = -1;
        ClearTaskInfoPanel();
    }

    private void ClearTaskInfoPanel()
    {
        if (taskNameText != null) taskNameText.text = "";
        if (taskCreatedDateText != null) taskCreatedDateText.text = "";
        if (taskExplanationText != null) taskExplanationText.text = "";
    }

    #endregion

    #region Screenshot

    public void TakeProjectScreenshot()
    {
        if (_selectedProjectId == -1)
        {
            Debug.LogWarning("❌ Ekran görüntüsü için bir proje seçin!");
            return;
        }

        var project = DatabaseManager.Instance.GetById<ProjectInfoData>(_selectedProjectId);
        if (project != null)
        {
            StartCoroutine(CaptureScreenshotCoroutine(project.Name));
        }
    }

    private System.Collections.IEnumerator CaptureScreenshotCoroutine(string projectName)
    {
        yield return new WaitForEndOfFrame();

        string fileName = $"{projectName}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
        string path = System.IO.Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop),
            "NETAS_Screenshots"
        );

        if (!System.IO.Directory.Exists(path))
            System.IO.Directory.CreateDirectory(path);

        string fullPath = System.IO.Path.Combine(path, fileName);
        ScreenCapture.CaptureScreenshot(fullPath);

        Debug.Log($"📸 Ekran görüntüsü kaydedildi: {fullPath}");
    }

    #endregion
}