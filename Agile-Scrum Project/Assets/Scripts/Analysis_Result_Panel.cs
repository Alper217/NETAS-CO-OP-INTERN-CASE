using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Analysis_Result_Panel : MonoBehaviour
{
    [Header("UI References")]
    public GameObject analysisPanel;
    public TextMeshProUGUI resultText;
    public Button closeButton;
    public ScrollRect scrollRect; // Uzun metinler için scroll

    void Start()
    {
        // Panel başlangıçta kapalı olsun
        if (analysisPanel != null)
        {
            analysisPanel.SetActive(false);
        }

        // Close button event'ini bağla
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ClosePanel);
        }
    }

    public void ShowResult(string analysisResult)
    {
        // Paneli aç
        if (analysisPanel != null)
        {
            analysisPanel.SetActive(true);
        }

        // Sonucu göster
        if (resultText != null)
        {
            resultText.text = analysisResult;
        }

        // Scroll'u en üste getir
        if (scrollRect != null)
        {
            scrollRect.normalizedPosition = new Vector2(0, 1);
        }

        Debug.Log("📊 Analiz sonucu gösterildi");
    }

    public void ShowLoading()
    {
        // Paneli aç
        if (analysisPanel != null)
        {
            analysisPanel.SetActive(true);
        }
        // Loading mesajı göster
        if (resultText != null)
        {
            resultText.text = "🤖 Projeniz analiz ediliyor...\nLütfen bekleyin...";
        }

        Debug.Log("⏳ Analiz loading gösterildi");
    }

    public void ClosePanel()
    {
        if (analysisPanel != null)
        {
            analysisPanel.SetActive(false);
        }
        Debug.Log("❌ Analiz paneli kapatıldı");
    }

    // Dışarıdan panel açmak için
    public void OpenPanel()
    {
        if (analysisPanel != null)
        {
            analysisPanel.SetActive(true);
        }
    }

    // Panel açık mı kontrolü
    public bool IsPanelOpen()
    {
        return analysisPanel != null && analysisPanel.activeInHierarchy;
    }
}