using UnityEngine;
using TMPro;
using System;

public class ProjectCardUI : MonoBehaviour
{
    private TextMeshProUGUI projectNameText;
    private TextMeshProUGUI projectDateText;
    private TextMeshProUGUI projectExplanationText;

    public int projectId { get; private set; }

    public void SetData(int id, string name, string date, string description)
    {
        projectId = id;

        if (projectNameText == null || projectDateText == null || projectExplanationText == null)
        {
            projectNameText = transform.Find("ProjectName")?.GetComponent<TextMeshProUGUI>();
            projectDateText = transform.Find("ProjectDate")?.GetComponent<TextMeshProUGUI>();
            projectExplanationText = transform.Find("ProjectExplanation")?.GetComponent<TextMeshProUGUI>();
        }

        if (projectNameText != null) projectNameText.text = name;
        if (projectDateText != null) projectDateText.text = date;
        if (projectExplanationText != null) projectExplanationText.text = description;
    }
}
