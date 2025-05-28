using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_ButtonController : MonoBehaviour
{
    [SerializeField] Button newProjectAddButton;
    [SerializeField] Button closePageButton;
    [SerializeField] GameObject newProjectAddPage;

    public void ClosePage()
    {
        newProjectAddPage.SetActive(false);
    }
    public void OpenPage()
    {
        newProjectAddPage.SetActive(true);
    }
}
