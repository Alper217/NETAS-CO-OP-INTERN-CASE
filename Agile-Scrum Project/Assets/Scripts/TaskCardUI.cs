using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class TaskCardUI : MonoBehaviour
{
    private TextMeshProUGUI taskTitleText;
    private TextMeshProUGUI taskDescriptionText;
    private Button cardButton;

    public int taskId { get; private set; }
    public int projectId { get; private set; }
    public string taskStatus { get; private set; }

    // Double-click system variables
    private float lastClickTime = 0f;
    private const float DOUBLE_CLICK_TIME = 0.3f;
    private bool waitingForDoubleClick = false;

    private void Start()
    {
        // Get button component
        cardButton = GetComponent<Button>();
        if (cardButton != null)
        {
            cardButton.onClick.RemoveAllListeners();
            cardButton.onClick.AddListener(OnCardClick);
        }
    }

    public void SetData(int id, int projId, string title, string description, string status)
    {
        taskId = id;
        projectId = projId;
        taskStatus = status;

        // Find UI components (lazy loading)
        if (taskTitleText == null || taskDescriptionText == null)
        {
            taskTitleText = transform.Find("TaskTitle")?.GetComponent<TextMeshProUGUI>();
            taskDescriptionText = transform.Find("TaskDescription")?.GetComponent<TextMeshProUGUI>();

            // If TaskTitle not found, use first TextMeshPro component (compatible with existing system)
            if (taskTitleText == null)
            {
                taskTitleText = GetComponentInChildren<TextMeshProUGUI>();
            }
        }

        // Assign data
        if (taskTitleText != null)
            taskTitleText.text = title;

        if (taskDescriptionText != null)
            taskDescriptionText.text = description;
    }

    private void OnCardClick()
    {
        float currentTime = Time.time;
        float timeSinceLastClick = currentTime - lastClickTime;

        if (timeSinceLastClick <= DOUBLE_CLICK_TIME && waitingForDoubleClick)
        {
            // DOUBLE CLICK DETECTED
            OnDoubleClick();
            waitingForDoubleClick = false;
            StopAllCoroutines(); // Stop single click coroutine
        }
        else
        {
            // SINGLE CLICK - Wait a bit, cancel single click if double click comes
            waitingForDoubleClick = true;
            StartCoroutine(SingleClickCoroutine());
        }

        lastClickTime = currentTime;
    }

    private IEnumerator SingleClickCoroutine()
    {
        yield return new WaitForSeconds(DOUBLE_CLICK_TIME);

        if (waitingForDoubleClick)
        {
            // No double click came, perform single click action
            OnSingleClick();
            waitingForDoubleClick = false;
        }
    }

    private void OnSingleClick()
    {
        Debug.Log($"Task single click: {taskId} - {taskTitleText?.text}");

        // Only select the task
        var projectManager = FindObjectOfType<ProjectManager>();
        if (projectManager != null)
        {
            projectManager.SelectTaskOnly(taskId);
        }
    }

    private void OnDoubleClick()
    {
        Debug.Log($"Task double click: {taskId} - {taskTitleText?.text}");

        // Select task AND open info panel
        var projectManager = FindObjectOfType<ProjectManager>();
        if (projectManager != null)
        {
            projectManager.SelectTaskAndOpenInfo(taskId);
        }
    }

    public int GetTaskId()
    {
        return taskId;
    }
}