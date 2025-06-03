using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Optimize edilmiş analiz sonuç paneli
/// Performans iyileştirmeleri ve temiz kod yapısı
/// </summary>
public class AnalysisResultPanel : MonoBehaviour
{
    [Header("UI References")]
    public GameObject analysisPanel;
    public TextMeshProUGUI resultText;
    public Button closeButton;
    public ScrollRect scrollRect;

    [Header("Loading Animation")]
    public GameObject loadingSpinner;
    public TextMeshProUGUI loadingText;

    private void Start()
    {
        InitializePanel();
        SetupButtonEvents();
    }

    private void InitializePanel()
    {
        // Panel başlangıçta kapalı
        if (analysisPanel != null)
        {
            analysisPanel.SetActive(false);
        }

        // Loading elementi gizle
        if (loadingSpinner != null)
        {
            loadingSpinner.SetActive(false);
        }
    }

    private void SetupButtonEvents()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(ClosePanel);
        }
    }

    public void ShowResult(string analysisResult)
    {
        if (string.IsNullOrEmpty(analysisResult))
        {
            Debug.LogWarning("⚠️ Boş analiz sonucu!");
            return;
        }

        OpenPanel();
        HideLoading();

        if (resultText != null)
        {
            resultText.text = analysisResult;
        }

        ScrollToTop();
        Debug.Log("📊 Analiz sonucu gösterildi");
    }

    public void ShowLoading()
    {
        OpenPanel();
        ShowLoadingElements();

        if (resultText != null)
        {
            resultText.text = "";
        }

        Debug.Log("⏳ Loading gösteriliyor");
    }

    public void ShowError(string errorMessage)
    {
        OpenPanel();
        HideLoading();

        if (resultText != null)
        {
            resultText.text = $"❌ **Hata**\n\n{errorMessage}";
        }

        ScrollToTop();
        Debug.LogWarning($"❌ Hata gösterildi: {errorMessage}");
    }

    public void ClosePanel()
    {
        if (analysisPanel != null)
        {
            analysisPanel.SetActive(false);
        }

        HideLoading();
        Debug.Log("❌ Analiz paneli kapatıldı");
    }

    public void OpenPanel()
    {
        if (analysisPanel != null)
        {
            analysisPanel.SetActive(true);
        }
    }

    public bool IsPanelOpen()
    {
        return analysisPanel != null && analysisPanel.activeInHierarchy;
    }

    private void ShowLoadingElements()
    {
        if (loadingSpinner != null)
        {
            loadingSpinner.SetActive(true);
        }

        if (loadingText != null)
        {
            loadingText.text = "🤖 Projeniz analiz ediliyor...\nLütfen bekleyin...";
        }
    }

    private void HideLoading()
    {
        if (loadingSpinner != null)
        {
            loadingSpinner.SetActive(false);
        }

        if (loadingText != null)
        {
            loadingText.text = "";
        }
    }

    private void ScrollToTop()
    {
        if (scrollRect != null)
        {
            // Hemen scroll'u en üste getir
            scrollRect.normalizedPosition = new Vector2(0, 1);

            // Bir frame sonra da tekrar kontrol et
            StartCoroutine(ScrollToTopCoroutine());
        }
    }

    private System.Collections.IEnumerator ScrollToTopCoroutine()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame(); // 2 frame bekle

        if (scrollRect != null)
        {
            scrollRect.normalizedPosition = new Vector2(0, 1);
            Debug.Log("📜 Scroll en üste getirildi");
        }
    }
    // Keyboard shortcut support
    private void Update()
    {
        if (IsPanelOpen() && Input.GetKeyDown(KeyCode.Escape))
        {
            ClosePanel();
        }
    }

    // Animation support (opsiyonel)
    public void ShowPanelWithAnimation()
    {
        if (analysisPanel != null)
        {
            analysisPanel.SetActive(true);

            // Basit fade-in animasyonu
            var canvasGroup = analysisPanel.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                StartCoroutine(FadeInCoroutine(canvasGroup));
            }
        }
    }

    private System.Collections.IEnumerator FadeInCoroutine(CanvasGroup canvasGroup)
    {
        canvasGroup.alpha = 0f;
        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }
}