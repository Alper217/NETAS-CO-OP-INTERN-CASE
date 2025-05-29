using System;
using UnityEngine;
using UnityEngine.UI;

public class DoubleClickButton : MonoBehaviour
{
    public Button targetButton;
    public float doubleClickThreshold = 0.3f;
    private float lastClickTime = 0f;
    public bool IsClicked;
    public UI_ButtonController targetController; // public yaptık


    void Start()
    {
        targetButton.onClick.AddListener(OnButtonClick);
        targetController = FindObjectOfType<UI_ButtonController>();

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
        Debug.Log("Çalıştı");
        targetController.OpenTaskInfoPanel();
    }
}
