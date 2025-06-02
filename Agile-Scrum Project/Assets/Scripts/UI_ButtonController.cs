using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_ButtonController : MonoBehaviour
{
    [Header("Pages")]
    [SerializeField] GameObject newProjectPage;
    [SerializeField] GameObject taskPage;
    [SerializeField] public GameObject taskInfoPanel;
    [SerializeField] public GameObject analysisPanel; // 👈 YENİ: Analiz paneli

    [Header("Add Part")]
    [SerializeField] Button newProjectAddButton;
    [SerializeField] Button taskAddButton;

    [Header("Update Part")]
    [SerializeField] Button projectUpdateButton;
    [SerializeField] Button taskUpdateButton;

    [Header("Analysis Part")] // 👈 YENİ BÖLÜM
    [SerializeField] Button analyzeProjectButton;
    [SerializeField] ProjectAnalyzer projectAnalyzer;

    [Header("Other Parts")]
    [SerializeField] Button closePageButton;

    DoubleClickButton clickButton;

    private void Start()
    {
        clickButton = GetComponent<DoubleClickButton>();

        // 👈 YENİ: Analiz butonuna event ekle
        if (analyzeProjectButton != null)
        {
            analyzeProjectButton.onClick.AddListener(AnalyzeProject);
        }
    }

    public void ClosePage()
    {
        newProjectPage.SetActive(false);
        taskPage.SetActive(false);
        taskInfoPanel.SetActive(false);

        // 👈 YENİ: Analiz panelini de kapat
        if (analysisPanel != null)
        {
            analysisPanel.SetActive(false);
        }
    }

    public void OpenaAddPage()
    {
        newProjectPage.SetActive(true);
        newProjectAddButton.gameObject.SetActive(true);
        projectUpdateButton.gameObject.SetActive(false);
    }

    public void OpenUpdatePage()
    {
        newProjectPage.SetActive(true);
        newProjectAddButton.gameObject.SetActive(false);
        projectUpdateButton.gameObject.SetActive(true);
    }

    public void OpenTaskAddPage()
    {
        taskPage.SetActive(true);
        taskAddButton.gameObject.SetActive(true);
        taskUpdateButton.gameObject.SetActive(false);
    }

    public void OpenTaskUpdatePage()
    {
        taskPage.SetActive(true);
        taskAddButton.gameObject.SetActive(false);
        taskUpdateButton.gameObject.SetActive(true);
    }

    public void OpenTaskInfoPanel()
    {
        taskInfoPanel.SetActive(true);
    }

    // 👈 YENİ METOD: Analiz Et
    public void AnalyzeProject()
    {
        if (projectAnalyzer != null)
        {
            Debug.Log("🔍 Proje analizi başlatıldı");
            projectAnalyzer.AnalyzeCurrentProject();
        }
        else
        {
            Debug.LogWarning("⚠️ ProjectAnalyzer referansı atanmamış!");
        }
    }

    // 👈 YENİ METOD: Analiz panelini aç
    public void OpenAnalysisPanel()
    {
        if (analysisPanel != null)
        {
            analysisPanel.SetActive(true);
        }
    }

    // 👈 YENİ METOD: Analiz panelini kapat
    public void CloseAnalysisPanel()
    {
        if (analysisPanel != null)
        {
            analysisPanel.SetActive(false);
        }
    }
}