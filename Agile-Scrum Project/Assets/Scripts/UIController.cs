using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Düzeltilmiş UI kontrol sınıfı - Button event sıralamalar düzeltildi
/// ✨ Çift tık desteği eklendi - tek tık sadece seçer, çift tık info paneli açar
/// </summary>
public class UIController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject projectEditPanel;
    public GameObject taskEditPanel;
    public GameObject taskInfoPanel;
    public GameObject analysisPanel;

    [Header("Project Buttons")]
    public Button addProjectButton;
    public Button updateProjectButton;
    public Button deleteProjectButton;

    [Header("Task Buttons")]
    public Button addTaskButton;
    public Button updateTaskButton;
    public Button deleteTaskButton;

    [Header("Task Status Buttons")]
    public Button todoToInProgressButton;
    public Button inProgressToDoneButton;
    public Button doneToInProgressButton;
    public Button inProgressToTodoButton;

    [Header("Other Buttons")]
    public Button analyzeProjectButton;
    public Button screenshotButton;
    public Button closeAllPanelsButton;

    [Header("Form Buttons (Project Panel)")]
    public Button createProjectFormButton;  // Panel içindeki "Proje Oluştur" butonu
    public Button updateProjectFormButton;  // Panel içindeki "Proje Güncelle" butonu

    [Header("Form Buttons (Task Panel)")]
    public Button createTaskFormButton;     // Panel içindeki "Görev Oluştur" butonu
    public Button updateTaskFormButton;     // Panel içindeki "Görev Güncelle" butonu

    [Header("Info Panel Buttons")]
    public Button openTaskInfoButton;       // ✨ YENİ: Manuel info paneli açma butonu

    [Header("References")]
    public ProjectManager projectManager;
    public ProjectAnalyzer projectAnalyzer;

    private void Start()
    {
        SetupButtonEvents();
        CloseAllPanels();

        // İlk açılışta projeleri yükle
        Debug.Log("🚀 UIController başlatıldı");
    }

    private void SetupButtonEvents()
    {
        // Ana menu butonları - sadece panel açar
        addProjectButton?.onClick.AddListener(() => {
            Debug.Log("📝 Proje ekleme paneli açılıyor");
            OpenProjectPanel(PanelMode.Add);
        });

        updateProjectButton?.onClick.AddListener(() => {
            Debug.Log("✏️ Proje güncelleme paneli açılıyor");
            if (projectManager.SelectedProjectId != -1)
            {
                OpenProjectPanel(PanelMode.Update);
                projectManager.LoadSelectedProjectToInputs(); // Bu metodu ekleyeceğiz
            }
            else
            {
                Debug.LogWarning("❌ Güncellemek için önce bir proje seçin!");
            }
        });

        deleteProjectButton?.onClick.AddListener(() => {
            if (ConfirmDelete("Bu projeyi ve tüm görevlerini silmek istediğinize emin misiniz?"))
            {
                projectManager.DeleteSelectedProject();
            }
        });

        // Panel içindeki form butonları - asıl işlemi yapar
        createProjectFormButton?.onClick.AddListener(() => {
            Debug.Log("💾 Proje oluşturuluyor");
            projectManager.CreateProject();
            CloseAllPanels();
        });

        updateProjectFormButton?.onClick.AddListener(() => {
            Debug.Log("💾 Proje güncelleniyor");
            projectManager.UpdateSelectedProject();
            CloseAllPanels();
        });

        // Task operations
        addTaskButton?.onClick.AddListener(() => {
            Debug.Log("📝 Görev ekleme paneli açılıyor");
            if (projectManager.SelectedProjectId != -1)
            {
                OpenTaskPanel(PanelMode.Add);
            }
            else
            {
                Debug.LogWarning("❌ Görev eklemek için önce bir proje seçin!");
            }
        });

        updateTaskButton?.onClick.AddListener(() => {
            Debug.Log("✏️ Görev güncelleme paneli açılıyor");
            if (projectManager.SelectedTaskId != -1)
            {
                OpenTaskPanel(PanelMode.Update);
            }
            else
            {
                Debug.LogWarning("❌ Güncellemek için önce bir görev seçin!");
            }
        });

        deleteTaskButton?.onClick.AddListener(() => {
            if (ConfirmDelete("Bu görevi silmek istediğinize emin misiniz?"))
            {
                projectManager.DeleteSelectedTask();
            }
        });

        // Panel içindeki task form butonları
        createTaskFormButton?.onClick.AddListener(() => {
            Debug.Log("💾 Görev oluşturuluyor");
            projectManager.CreateTask();
            CloseAllPanels();
        });

        updateTaskFormButton?.onClick.AddListener(() => {
            Debug.Log("💾 Görev güncelleniyor");
            projectManager.UpdateSelectedTask();
            CloseAllPanels();
        });

        // ✨ YENİ: Manuel info paneli açma butonu
        openTaskInfoButton?.onClick.AddListener(() => {
            if (projectManager.SelectedTaskId != -1)
            {
                projectManager.OpenSelectedTaskInfo();
            }
            else
            {
                Debug.LogWarning("❌ Info paneli açmak için önce bir görev seçin!");
            }
        });

        // Task status changes
        todoToInProgressButton?.onClick.AddListener(() => {
            projectManager.ChangeTaskStatus("InProgress");
            Debug.Log("✅ Task durumu: ToDo → InProgress");
        });

        inProgressToDoneButton?.onClick.AddListener(() => {
            projectManager.ChangeTaskStatus("Done");
            Debug.Log("✅ Task durumu: InProgress → Done");
        });

        doneToInProgressButton?.onClick.AddListener(() => {
            projectManager.ChangeTaskStatus("InProgress");
            Debug.Log("✅ Task durumu: Done → InProgress (geri alındı)");
        });

        inProgressToTodoButton?.onClick.AddListener(() => {
            projectManager.ChangeTaskStatus("ToDo");
            Debug.Log("✅ Task durumu: InProgress → ToDo (geri alındı)");
        });

        // Other operations
        analyzeProjectButton?.onClick.AddListener(() => {
            if (projectManager.SelectedProjectId != -1)
            {
                projectAnalyzer?.AnalyzeCurrentProject();
            }
            else
            {
                Debug.LogWarning("❌ Analiz için önce bir proje seçin!");
            }
        });

        screenshotButton?.onClick.AddListener(() => {
            projectManager?.TakeProjectScreenshot();
        });

        closeAllPanelsButton?.onClick.AddListener(CloseAllPanels);

        // Project/Task selection events
        if (projectManager != null)
        {
            projectManager.OnProjectSelected += OnProjectSelected;
            projectManager.OnTaskSelected += OnTaskSelected;
        }
    }

    private void OnProjectSelected(int projectId)
    {
        Debug.Log($"🎯 Proje seçildi: {projectId}");
        UpdateButtonStates();
    }

    // ⚠️ DÜZELTİLDİ: Artık otomatik info paneli açmıyor
    private void OnTaskSelected(int taskId)
    {
        Debug.Log($"🎯 Görev seçildi: {taskId}");
        UpdateButtonStates();
        // OpenTaskInfoPanel(); // ❌ KALDIRILDI - Artık sadece çift tık ile açılacak
    }

    private void UpdateButtonStates()
    {
        bool hasSelectedProject = projectManager != null && projectManager.SelectedProjectId != -1;
        bool hasSelectedTask = projectManager != null && projectManager.SelectedTaskId != -1;

        // Project buttons
        if (updateProjectButton != null) updateProjectButton.interactable = hasSelectedProject;
        if (deleteProjectButton != null) deleteProjectButton.interactable = hasSelectedProject;

        // Task buttons
        if (addTaskButton != null) addTaskButton.interactable = hasSelectedProject;
        if (updateTaskButton != null) updateTaskButton.interactable = hasSelectedTask;
        if (deleteTaskButton != null) deleteTaskButton.interactable = hasSelectedTask;

        // ✨ YENİ: Info panel butonu durumu
        if (openTaskInfoButton != null) openTaskInfoButton.interactable = hasSelectedTask;

        // ✨ YENİ: Task status butonları - seçili task'ın durumuna göre aktif/pasif
        string selectedTaskStatus = GetSelectedTaskStatus();

        if (todoToInProgressButton != null)
            todoToInProgressButton.interactable = selectedTaskStatus == "ToDo";

        if (inProgressToDoneButton != null)
            inProgressToDoneButton.interactable = selectedTaskStatus == "InProgress";

        if (doneToInProgressButton != null)
            doneToInProgressButton.interactable = selectedTaskStatus == "Done";

        if (inProgressToTodoButton != null)
            inProgressToTodoButton.interactable = selectedTaskStatus == "InProgress";

        // Other buttons
        if (analyzeProjectButton != null) analyzeProjectButton.interactable = hasSelectedProject;
        if (screenshotButton != null) screenshotButton.interactable = hasSelectedProject;
    }

    // ✨ YENİ METOT: Seçili task'ın durumunu döndürür
    private string GetSelectedTaskStatus()
    {
        if (projectManager == null || projectManager.SelectedTaskId == -1)
            return "";

        // DatabaseManager'dan seçili task'ı al ve durumunu döndür
        var task = DatabaseManager.Instance.GetById<ProjectTasks>(projectManager.SelectedTaskId);
        return task?.status ?? "";
    }

    public enum PanelMode
    {
        Add,
        Update
    }

    public void OpenProjectPanel(PanelMode mode)
    {
        CloseAllPanels();

        if (projectEditPanel != null)
        {
            projectEditPanel.SetActive(true);
        }

        // Form butonlarının görünürlüğünü ayarla
        if (createProjectFormButton != null) createProjectFormButton.gameObject.SetActive(mode == PanelMode.Add);
        if (updateProjectFormButton != null) updateProjectFormButton.gameObject.SetActive(mode == PanelMode.Update);
    }

    public void OpenTaskPanel(PanelMode mode)
    {
        CloseAllPanels();

        if (taskEditPanel != null)
        {
            taskEditPanel.SetActive(true);
        }

        // Form butonlarının görünürlüğünü ayarla
        if (createTaskFormButton != null) createTaskFormButton.gameObject.SetActive(mode == PanelMode.Add);
        if (updateTaskFormButton != null) updateTaskFormButton.gameObject.SetActive(mode == PanelMode.Update);
    }

    public void OpenTaskInfoPanel()
    {
        if (taskInfoPanel != null)
        {
            taskInfoPanel.SetActive(true);
            Debug.Log("📋 Task info paneli açıldı");
        }
    }

    public void OpenAnalysisPanel()
    {
        if (analysisPanel != null)
        {
            analysisPanel.SetActive(true);
        }
    }

    public void CloseAllPanels()
    {
        if (projectEditPanel != null) projectEditPanel.SetActive(false);
        if (taskEditPanel != null) taskEditPanel.SetActive(false);
        if (taskInfoPanel != null) taskInfoPanel.SetActive(false);
        if (analysisPanel != null) analysisPanel.SetActive(false);

        Debug.Log("🔒 Tüm paneller kapatıldı");
    }

    private bool ConfirmDelete(string message)
    {
#if UNITY_EDITOR
        return UnityEditor.EditorUtility.DisplayDialog("Silme Onayı", message, "Evet", "Hayır");
#else
        Debug.LogWarning("Silme işlemi: " + message);
        return true; // Runtime'da always true, UI confirmation ekleyebilirsiniz
#endif
    }

    // External method calls - Bu metotlar UI butonlarından direk çağrılabilir
    public void OnAddProjectButtonClick() => OpenProjectPanel(PanelMode.Add);
    public void OnUpdateProjectButtonClick() => OpenProjectPanel(PanelMode.Update);
    public void OnAddTaskButtonClick() => OpenTaskPanel(PanelMode.Add);
    public void OnUpdateTaskButtonClick() => OpenTaskPanel(PanelMode.Update);

    private void OnDestroy()
    {
        // Event subscription'ları temizle
        if (projectManager != null)
        {
            projectManager.OnProjectSelected -= OnProjectSelected;
            projectManager.OnTaskSelected -= OnTaskSelected;
        }
    }
}