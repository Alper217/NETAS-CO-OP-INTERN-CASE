using UnityEngine;
using UnityEngine.UI;

public class DoubleClickButton : MonoBehaviour
{
    public Button targetButton;
    public float doubleClickThreshold = 0.3f;
    [SerializeField] GameObject taskInfoPage;
    private float lastClickTime = 0f;
    public bool IsClicked;
     UI_ButtonController targetController;

    void Start()
    {
        targetButton.onClick.AddListener(OnButtonClick);
        targetController = GetComponent<UI_ButtonController>();
    }

    private void OnButtonClick()
    {
        float timeSinceLastClick = Time.time - lastClickTime;

        if (timeSinceLastClick <= doubleClickThreshold)
        {
            IsClicked = true;
            Debug.Log("✅ ÇİFT TIKLAMA ALGILANDI");
            OnDoubleClick();
        }

        lastClickTime = Time.time;
    }
    public void OnDoubleClick()
    {
        taskInfoPage.SetActive(true);
    }
}
