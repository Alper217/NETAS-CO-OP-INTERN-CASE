using UnityEngine;
using TMPro;
using UnityEngine.UI;

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
        if (analysisPanel != null)
        {
            analysisPanel.SetActive(false);
        }

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
            Debug.LogWarning("Empty analysis result received.");
            return;
        }

        OpenPanel();
        HideLoading();

        if (resultText != null)
        {
            resultText.text = analysisResult;
        }

        ScrollToTop();
        Debug.Log("Analysis result displayed successfully.");
    }

    public void ShowLoading()
    {
        OpenPanel();
        ShowLoadingElements();

        if (resultText != null)
        {
            resultText.text = "";
        }

        Debug.Log("Loading state activated.");
    }

    public void ShowError(string errorMessage)
    {
        OpenPanel();
        HideLoading();

        if (resultText != null)
        {
            resultText.text = $"**Error**\n\n{errorMessage}";
        }

        ScrollToTop();
        Debug.LogWarning($"Error displayed: {errorMessage}");
    }

    public void ClosePanel()
    {
        if (analysisPanel != null)
        {
            analysisPanel.SetActive(false);
        }

        HideLoading();
        Debug.Log("Analysis panel closed.");
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
            loadingText.text = "Analyzing your project...\nPlease wait...";
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
            scrollRect.normalizedPosition = new Vector2(0, 1);

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
            Debug.Log("Scroll position reset to top.");
        }
    }
    private void Update()
    {
        if (IsPanelOpen() && Input.GetKeyDown(KeyCode.Escape))
        {
            ClosePanel();
        }
    }
    public void ShowPanelWithAnimation()
    {
        if (analysisPanel != null)
        {
            analysisPanel.SetActive(true);

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