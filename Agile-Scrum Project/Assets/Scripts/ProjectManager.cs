using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Optimized class that manages all project operations
/// Includes project CRUD, task management and UI updates
/// Double-click support added - single click only selects, double click opens info panel
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
            Debug.LogWarning("Project name cannot be empty!");
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
            Debug.Log($"New project created: {newProject.Name}");

            ClearProjectInputs();
            LoadAllProjects();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Project creation error: {ex.Message}");
        }
    }

    public void UpdateSelectedProject()
    {
        if (_selectedProjectId == -1)
        {
            Debug.LogWarning("Please select a project to update!");
            return;
        }

        var project = DatabaseManager.Instance.GetById<ProjectInfoData>(_selectedProjectId);
        if (project != null)
        {
            project.Name = projectNameInput.text.Trim();
            project.Description = projectDescriptionInput.text?.Trim() ?? "";
            project.MarkAsModified();

            DatabaseManager.Instance.UpdateItem(project);
            Debug.Log($"Project updated: {project.Name}");

            ClearProjectInputs();
            LoadAllProjects();
        }
    }

    public void DeleteSelectedProject()
    {
        if (_selectedProjectId == -1)
        {
            Debug.LogWarning("Please select a project to delete!");
            return;
        }

        try
        {
            DatabaseManager.Instance.ExecuteTransaction(() =>
            {
                // First delete all project tasks
                var tasks = DatabaseManager.Instance.GetTasksByProjectId(_selectedProjectId);
                foreach (var task in tasks)
                {
                    DatabaseManager.Instance.Delete(task);
                }

                // Then delete the project
                var project = DatabaseManager.Instance.GetById<ProjectInfoData>(_selectedProjectId);
                if (project != null)
                {
                    DatabaseManager.Instance.Delete(project);
                }
            });

            Debug.Log("Project and all tasks deleted successfully");
            _selectedProjectId = -1;
            ClearProjectInputs();
            LoadAllProjects();
            ClearTasks();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Project deletion error: {ex.Message}");
        }
    }

    public void LoadSelectedProjectToInputs()
    {
        if (_selectedProjectId == -1)
        {
            Debug.LogWarning("Please select a project to load!");
            return;
        }

        var project = DatabaseManager.Instance.GetById<ProjectInfoData>(_selectedProjectId);
        if (project != null)
        {
            projectNameInput.text = project.Name;
            projectDescriptionInput.text = project.Description;
            Debug.Log($"Project information loaded to inputs: {project.Name}");
        }
        else
        {
            Debug.LogWarning($"Project not found: {_selectedProjectId}");
        }
    }

    private void LoadAllProjects()
    {
        ClearProjectCards();

        var projects = DatabaseManager.Instance.GetAll<ProjectInfoData>();
        Debug.Log($"Loading {projects.Count} projects...");

        for (int i = 0; i < projects.Count; i++)
        {
            CreateProjectCard(projects[i], i);
        }

        UpdateProjectScrollArea(projects.Count);
        Debug.Log($"{projects.Count} project cards created successfully");
    }

    private void CreateProjectCard(ProjectInfoData project, int index)
    {
        if (projectCardPrefab == null)
        {
            Debug.LogError("Project Card Prefab not assigned!");
            return;
        }

        if (projectContentParent == null)
        {
            Debug.LogError("Project Content Parent not assigned!");
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
            Debug.Log($"Project card created: {project.Name} (ID: {project.ID})");
        }
        else
        {
            Debug.LogError("ProjectCardUI component not found!");
        }

        // Click event
        var button = card.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => SelectProject(project.ID));
        }
        else
        {
            Debug.LogWarning("Button component not found on Project Card!");
        }

        _projectCards[project.ID] = card;
    }

    private void UpdateProjectScrollArea(int projectCount)
    {
        if (projectContentParent != null)
        {
            float totalWidth = projectCount * (itemWidth + itemSpacing);
            projectContentParent.sizeDelta = new Vector2(totalWidth, projectContentParent.sizeDelta.y);
            Debug.Log($"Scroll area width updated: {totalWidth}");
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
        _selectedTaskId = -1; // Clear task selection

        Debug.Log($"Project selected: {projectId}");

        LoadProjectTasks();
        OnProjectSelected?.Invoke(projectId);
    }

    #endregion

    #region Task Management

    public void CreateTask()
    {
        if (_selectedProjectId == -1)
        {
            Debug.LogWarning("Please select a project first to add a task!");
            return;
        }

        if (string.IsNullOrWhiteSpace(taskTitleInput.text))
        {
            Debug.LogWarning("Task title cannot be empty!");
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
            Debug.Log($"New task created: {newTask.title}");

            ClearTaskInputs();
            LoadProjectTasks();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Task creation error: {ex.Message}");
        }
    }

    public void UpdateSelectedTask()
    {
        if (_selectedTaskId == -1)
        {
            Debug.LogWarning("Please select a task to update!");
            return;
        }

        var task = DatabaseManager.Instance.GetById<ProjectTasks>(_selectedTaskId);
        if (task != null)
        {
            task.title = taskTitleInput.text.Trim();
            task.description = taskDescriptionInput.text?.Trim() ?? "";
            task.MarkAsModified();

            DatabaseManager.Instance.UpdateItem(task);
            Debug.Log($"Task updated: {task.title}");

            ClearTaskInputs();
            LoadProjectTasks();
            ClearTaskSelection();
        }
    }

    public void DeleteSelectedTask()
    {
        if (_selectedTaskId == -1)
        {
            Debug.LogWarning("Please select a task to delete!");
            return;
        }

        var task = DatabaseManager.Instance.GetById<ProjectTasks>(_selectedTaskId);
        if (task != null)
        {
            DatabaseManager.Instance.Delete(task);
            Debug.Log($"Task deleted: {task.title}");

            ClearTaskInputs();
            LoadProjectTasks();
            ClearTaskSelection();
        }
    }

    public void ChangeTaskStatus(string newStatus)
    {
        if (_selectedTaskId == -1)
        {
            Debug.LogWarning("Please select a task to change status!");
            return;
        }

        var task = DatabaseManager.Instance.GetById<ProjectTasks>(_selectedTaskId);
        if (task != null)
        {
            string oldStatus = task.status;
            task.status = newStatus;
            task.MarkAsModified();

            DatabaseManager.Instance.UpdateItem(task);

            Debug.Log($"Task status updated: {oldStatus} → {newStatus}");
            LoadProjectTasks();

            SelectTaskOnly(_selectedTaskId);
        }
    }

    #region Task Selection Methods - Double Click Support
    public void SelectTaskOnly(int taskId)
    {
        _selectedTaskId = taskId;
        Debug.Log($"Task selected (only): {taskId}");

        LoadTaskToInputs(taskId);
        LoadTaskInfoPanel(taskId); // Load info panel data but don't open panel
        OnTaskSelected?.Invoke(taskId);

        // No panel opening - only selection
    }

    public void SelectTaskAndOpenInfo(int taskId)
    {
        _selectedTaskId = taskId;
        Debug.Log($"Task selected + Info panel opened: {taskId}");

        LoadTaskToInputs(taskId);
        LoadTaskInfoPanel(taskId);
        OpenTaskInfoPanel();
        OnTaskSelected?.Invoke(taskId);
    }

    private void SelectTask(int taskId)
    {
        _selectedTaskId = taskId;
        Debug.Log($"Task selected: {taskId}");

        LoadTaskToInputs(taskId);
        LoadTaskInfoPanel(taskId);
        OnTaskSelected?.Invoke(taskId);
    }

    public void OpenSelectedTaskInfo()
    {
        if (_selectedTaskId != -1)
        {
            LoadTaskInfoPanel(_selectedTaskId);
            OpenTaskInfoPanel();
            Debug.Log($"Task info panel manually opened: {_selectedTaskId}");
        }
        else
        {
            Debug.LogWarning("Please select a task first to open info panel!");
        }
    }

    #endregion

    private void LoadProjectTasks()
    {
        if (_selectedProjectId == -1) return;

        ClearTasks();

        var tasks = DatabaseManager.Instance.GetTasksByProjectId(_selectedProjectId);
        Debug.Log($"Loading {tasks.Count} tasks...");

        foreach (var task in tasks)
        {
            CreateTaskCard(task);
        }
    }

    private void CreateTaskCard(ProjectTasks task)
    {
        if (taskCardPrefab == null)
        {
            Debug.LogError("Task Card Prefab not assigned!");
            return;
        }

        GameObject taskCard = Instantiate(taskCardPrefab);

        var cardUI = taskCard.GetComponent<TaskCardUI>();
        if (cardUI != null)
        {
            cardUI.SetData(task.id, task.projectId, task.title, task.description, task.status);
            Debug.Log($"Task card created: {task.title} ({task.status})");
        }
        else
        {
            Debug.LogError("TaskCardUI component not found!");
        }

        // NOTE: Button events are now managed by TaskCardUI with double-click system
        // Don't add manual onClick here, TaskCardUI handles it

        // Add to parent
        Transform parent = GetTaskParent(task.status);
        if (parent != null)
        {
            taskCard.transform.SetParent(parent, false);
        }
        else
        {
            Debug.LogError($"Task parent not found: {task.status}");
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

        // Clear parents
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

    public void OpenTaskInfoPanel()
    {
        // Call OpenTaskInfoPanel method from UIController
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
            Debug.LogWarning("Please select a project to take screenshot!");
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

        Debug.Log($"Screenshot saved: {fullPath}");
    }

    #endregion
}