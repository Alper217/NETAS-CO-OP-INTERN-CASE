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
    [Header("Add Part")]
    [SerializeField] Button newProjectAddButton;
    [SerializeField] Button taskAddButton;
    [Header("Update Part")]
    [SerializeField] Button projectUpdateButton;
    [SerializeField] Button taskUpdateButton;
    [Header("Other Parts")]
    [SerializeField] Button closePageButton;

    DoubleClickButton clickButton;
    private void Start()
    {
        clickButton = GetComponent<DoubleClickButton>();
    }

    public void ClosePage()
    {
        newProjectPage.SetActive(false);
        taskPage.SetActive(false);
        taskInfoPanel.SetActive(false);
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
}
