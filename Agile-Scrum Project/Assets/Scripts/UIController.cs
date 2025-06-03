using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Fixed UI control class - Button event ordering corrected
/// Double-click support added - single click only selects, double click opens info panel
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
    public Button createProjectFormButton;  // "Create Project" button inside panel
    public Button updateProjectFormButton;  // "Update Project" button inside panel

    [Header("Form Buttons (Task Panel)")]
    public Button createTaskFormButton;     // "Create Task" button inside panel
    public Button updateTaskFormButton;     // "Update Task" button inside panel

    [Header("Info Panel Buttons")]
    public Button openTaskInfoButton;       // Manual info panel open button

    [Header("References")]
    public ProjectManager projectManager;
    public ProjectAnalyzer projectAnalyzer;

    private void Start()
    {
        SetupButtonEvents();
        CloseAllPanels();

        Debug.Log("UIController initialized");
    }

    private void SetupButtonEvents()
    {
        // Main menu buttons - only open panels
        addProjectButton?.onClick.AddListener(() => {
            Debug.Log("Opening project creation panel");
            OpenProjectPanel(PanelMode.Add);
        });

        updateProjectButton?.onClick.AddListener(() => {
            Debug.Log("Opening project update panel");
            if (projectManager.SelectedProjectId != -1)
            {
                OpenProjectPanel(PanelMode.Update);
                projectManager.LoadSelectedProjectToInputs();
            }
            else
            {
                Debug.LogWarning("Please select a project first to update!");
            }
        });

        deleteProjectButton?.onClick.AddListener(() => {
            if (ConfirmDelete("Are you sure you want to delete this project and all its tasks?"))
            {
                projectManager.DeleteSelectedProject();
            }
        });

        // Form buttons inside panels - perform actual operations
        createProjectFormButton?.onClick.AddListener(() => {
            Debug.Log("Creating project");
            projectManager.CreateProject();
            CloseAllPanels();
        });

        updateProjectFormButton?.onClick.AddListener(() => {
            Debug.Log("Updating project");
            projectManager.UpdateSelectedProject();
            CloseAllPanels();
        });

        // Task operations
        addTaskButton?.onClick.AddListener(() => {
            Debug.Log("Opening task creation panel");
            if (projectManager.SelectedProjectId != -1)
            {
                OpenTaskPanel(PanelMode.Add);
            }
            else
            {
                Debug.LogWarning("Please select a project first to add a task!");
            }
        });

        updateTaskButton?.onClick.AddListener(() => {
            Debug.Log("Opening task update panel");
            if (projectManager.SelectedTaskId != -1)
            {
                OpenTaskPanel(PanelMode.Update);
            }
            else
            {
                Debug.LogWarning("Please select a task first to update!");
            }
        });

        deleteTaskButton?.onClick.AddListener(() => {
            if (ConfirmDelete("Are you sure you want to delete this task?"))
            {
                projectManager.DeleteSelectedTask();
            }
        });

        // Task form buttons inside panels
        createTaskFormButton?.onClick.AddListener(() => {
            Debug.Log("Creating task");
            projectManager.CreateTask();
            CloseAllPanels();
        });

        updateTaskFormButton?.onClick.AddListener(() => {
            Debug.Log("Updating task");
            projectManager.UpdateSelectedTask();
            CloseAllPanels();
        });

        // Manual info panel open button
        openTaskInfoButton?.onClick.AddListener(() => {
            if (projectManager.SelectedTaskId != -1)
            {
                projectManager.OpenSelectedTaskInfo();
            }
            else
            {
                Debug.LogWarning("Please select a task first to open info panel!");
            }
        });

        // Task status changes
        todoToInProgressButton?.onClick.AddListener(() => {
            projectManager.ChangeTaskStatus("InProgress");
            Debug.Log("Task status: ToDo → InProgress");
        });

        inProgressToDoneButton?.onClick.AddListener(() => {
            projectManager.ChangeTaskStatus("Done");
            Debug.Log("Task status: InProgress → Done");
        });

        doneToInProgressButton?.onClick.AddListener(() => {
            projectManager.ChangeTaskStatus("InProgress");
            Debug.Log("Task status: Done → InProgress (reverted)");
        });

        inProgressToTodoButton?.onClick.AddListener(() => {
            projectManager.ChangeTaskStatus("ToDo");
            Debug.Log("Task status: InProgress → ToDo (reverted)");
        });

        // Other operations
        analyzeProjectButton?.onClick.AddListener(() => {
            if (projectManager.SelectedProjectId != -1)
            {
                projectAnalyzer?.AnalyzeCurrentProject();
            }
            else
            {
                Debug.LogWarning("Please select a project first to analyze!");
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
        Debug.Log($"Project selected: {projectId}");
        UpdateButtonStates();
    }

    // FIXED: No longer automatically opens info panel
    private void OnTaskSelected(int taskId)
    {
        Debug.Log($"Task selected: {taskId}");
        UpdateButtonStates();
        // OpenTaskInfoPanel(); // REMOVED - Now only opens with double click
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

        // Info panel button state
        if (openTaskInfoButton != null) openTaskInfoButton.interactable = hasSelectedTask;

        // Task status buttons - active/inactive based on selected task status
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

    // Returns the status of selected task
    private string GetSelectedTaskStatus()
    {
        if (projectManager == null || projectManager.SelectedTaskId == -1)
            return "";

        // Get selected task from DatabaseManager and return its status
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

        // Set form button visibility
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

        // Set form button visibility
        if (createTaskFormButton != null) createTaskFormButton.gameObject.SetActive(mode == PanelMode.Add);
        if (updateTaskFormButton != null) updateTaskFormButton.gameObject.SetActive(mode == PanelMode.Update);
    }

    public void OpenTaskInfoPanel()
    {
        if (taskInfoPanel != null)
        {
            taskInfoPanel.SetActive(true);
            Debug.Log("Task info panel opened");
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

        Debug.Log("All panels closed");
    }

    private bool ConfirmDelete(string message)
    {
#if UNITY_EDITOR
        return UnityEditor.EditorUtility.DisplayDialog("Delete Confirmation", message, "Yes", "No");
#else
        Debug.LogWarning("Delete operation: " + message);
        return true; // Always true in runtime, you can add UI confirmation
#endif
    }

    // External method calls - These methods can be called directly from UI buttons
    public void OnAddProjectButtonClick() => OpenProjectPanel(PanelMode.Add);
    public void OnUpdateProjectButtonClick() => OpenProjectPanel(PanelMode.Update);
    public void OnAddTaskButtonClick() => OpenTaskPanel(PanelMode.Add);
    public void OnUpdateTaskButtonClick() => OpenTaskPanel(PanelMode.Update);

    private void OnDestroy()
    {
        // Clean up event subscriptions
        if (projectManager != null)
        {
            projectManager.OnProjectSelected -= OnProjectSelected;
            projectManager.OnTaskSelected -= OnTaskSelected;
        }
    }
}