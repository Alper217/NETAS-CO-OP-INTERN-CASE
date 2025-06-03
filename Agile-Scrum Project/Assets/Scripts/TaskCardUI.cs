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

    // Çift tık sistemi için
    private float lastClickTime = 0f;
    private const float DOUBLE_CLICK_TIME = 0.3f;
    private bool waitingForDoubleClick = false;

    private void Start()
    {
        // Button component'ini al
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

        // UI bileşenlerini bul (lazy loading)
        if (taskTitleText == null || taskDescriptionText == null)
        {
            taskTitleText = transform.Find("TaskTitle")?.GetComponent<TextMeshProUGUI>();
            taskDescriptionText = transform.Find("TaskDescription")?.GetComponent<TextMeshProUGUI>();

            // Eğer TaskTitle bulunamazsa, ilk TextMeshPro'yu kullan (mevcut sisteminizle uyumlu)
            if (taskTitleText == null)
            {
                taskTitleText = GetComponentInChildren<TextMeshProUGUI>();
            }
        }

        // Verileri ata
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
            // ÇİFT TIK ALGILANDI
            OnDoubleClick();
            waitingForDoubleClick = false;
            StopAllCoroutines(); // Tek tık coroutine'ini durdur
        }
        else
        {
            // TEK TIK - Biraz bekle, çift tık gelirse tek tık iptal et
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
            // Çift tık gelmedi, tek tık işlemini yap
            OnSingleClick();
            waitingForDoubleClick = false;
        }
    }

    private void OnSingleClick()
    {
        Debug.Log($"🖱️ Task tek tık: {taskId} - {taskTitleText?.text}");

        // Sadece task'ı seç
        var projectManager = FindObjectOfType<ProjectManager>();
        if (projectManager != null)
        {
            projectManager.SelectTaskOnly(taskId);
        }
    }

    private void OnDoubleClick()
    {
        Debug.Log($"🖱️🖱️ Task çift tık: {taskId} - {taskTitleText?.text}");

        // Task'ı seç VE info panelini aç
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