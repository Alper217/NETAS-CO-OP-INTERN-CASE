using UnityEngine;
using TMPro;

public class ProjectCardUI : MonoBehaviour
{
    private TextMeshProUGUI projectNameText;
    private TextMeshProUGUI projectDateText;
    private TextMeshProUGUI projectExplanationText;

    public void SetData(string name, string date, string description)
    {
        // Ýlk kez çaðrýldýðýnda TextMeshPro nesnelerini bul
        if (projectNameText == null || projectDateText == null || projectExplanationText == null)
        {
            projectNameText = transform.Find("ProjectName")?.GetComponent<TextMeshProUGUI>();
            projectDateText = transform.Find("ProjectDate")?.GetComponent<TextMeshProUGUI>();
            projectExplanationText = transform.Find("ProjectExplanation")?.GetComponent<TextMeshProUGUI>();
        }

        // Verileri atama
        if (projectNameText != null) projectNameText.text = name;
        if (projectDateText != null) projectDateText.text = date;
        if (projectExplanationText != null) projectExplanationText.text = description;
    }
}
