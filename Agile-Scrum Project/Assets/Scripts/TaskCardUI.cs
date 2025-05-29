using UnityEngine;
using TMPro;

public class TaskCardUI : MonoBehaviour
{
    private TextMeshProUGUI taskTitleText;
    private TextMeshProUGUI taskDescriptionText;

    public int taskId { get; private set; }
    public int projectId { get; private set; }
    public string taskStatus { get; private set; }

    public void SetData(int id, int projId, string title, string description, string status)
    {
        taskId = id;
        projectId = projId;
        taskStatus = status;

        // UI bileþenlerini bul (lazy loading)
        if (taskTitleText == null || taskDescriptionText == null)
        {
            taskTitleText = transform.Find("TaskTitle")?.GetComponent<TextMeshProUGUI>();
            taskDescriptionText = transform.Find("TaskDescription")?.GetComponent<TextMeshProUGUI>();

            // Eðer TaskTitle bulunamazsa, ilk TextMeshPro'yu kullan (mevcut sisteminizle uyumlu)
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
    public int GetTaskId()
    {
        return taskId;
    }
}