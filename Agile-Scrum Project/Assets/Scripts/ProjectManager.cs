//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;

//public class ProjectManager : MonoBehaviour
//{
//    [Header("UI References")]
//    public GameObject projectItemPrefab;
//    public Transform contentTransform;

//    void Start()
//    {
//        // DatabaseManager'�n haz�r olmas�n� bekle
//        Invoke("LoadProjectsFromDatabase", 0.5f);
//    }

//    void LoadProjectsFromDatabase()
//    {
//        // �nce mevcut UI ��elerini temizle
//        ClearProjectList();

//        // DatabaseManager'dan projeleri al
//        if (DatabaseManager.Instance != null)
//        {
//            List<Project> projects = DatabaseManager.Instance.GetAllProjects();

//            Debug.Log($"Veritaban�ndan {projects.Count} proje y�klendi");

//            // Her proje i�in UI ��esi olu�tur
//            foreach (Project project in projects)
//            {
//                CreateProjectItem(project);
//            }
//        }
//        else
//        {
//            Debug.LogError("DatabaseManager bulunamad�!");
//        }
//    }

//    void CreateProjectItem(Project project)
//    {
//        if (projectItemPrefab == null || contentTransform == null)
//        {
//            Debug.LogError("ProjectItemPrefab veya ContentTransform atanmam��!");
//            return;
//        }

//        // Prefab'� instantiate et
//        GameObject item = Instantiate(projectItemPrefab, contentTransform);

//        // Text bile�enlerini bul ve g�ncelle
//        Transform nameText = item.transform.Find("ProjectNameText");
//        Transform descText = item.transform.Find("ProjectDescText");

//        if (nameText != null)
//        {
//            Text nameComponent = nameText.GetComponent<Text>();
//            if (nameComponent != null)
//            {
//                nameComponent.text = project.name;
//            }
//        }
//        else
//        {
//            Debug.LogWarning("ProjectNameText bulunamad�! Prefab yap�s�n� kontrol et.");
//        }

//        if (descText != null)
//        {
//            Text descComponent = descText.GetComponent<Text>();
//            if (descComponent != null)
//            {
//                descComponent.text = project.description;
//            }
//        }
//        else
//        {
//            Debug.LogWarning("ProjectDescText bulunamad�! Prefab yap�s�n� kontrol et.");
//        }

//        // Proje ID'sini sakla (ileride kullanmak i�in)
//        ProjectItem projectItemScript = item.GetComponent<ProjectItem>();
//        if (projectItemScript == null)
//        {
//            projectItemScript = item.AddComponent<ProjectItem>();
//        }
//        projectItemScript.projectId = project.id;
//        projectItemScript.projectName = project.name;

//        Debug.Log($"Proje UI'ya eklendi: {project.name}");
//    }

//    void ClearProjectList()
//    {
//        if (contentTransform != null)
//        {
//            // T�m �ocuk ��eleri sil
//            for (int i = contentTransform.childCount - 1; i >= 0; i--)
//            {
//                DestroyImmediate(contentTransform.GetChild(i).gameObject);
//            }
//        }
//    }


//}

//// Proje ��esi i�in yard�mc� script
//public class ProjectItem : MonoBehaviour
//{
//    public int projectId;
//    public string projectName;

//    // Proje ��esine t�kland���nda �al��acak
//    public void OnProjectClicked()
//    {
//        Debug.Log($"Proje t�kland�: {projectName} (ID: {projectId})");

//        // Buraya proje detay sayfas�na ge�i� kodu eklenebilir
//        // SceneManager.LoadScene("ProjectDetailScene");
//        // veya panel de�i�imi yap�labilir
//    }
//}
