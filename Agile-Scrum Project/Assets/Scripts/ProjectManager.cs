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
//        // DatabaseManager'ýn hazýr olmasýný bekle
//        Invoke("LoadProjectsFromDatabase", 0.5f);
//    }

//    void LoadProjectsFromDatabase()
//    {
//        // Önce mevcut UI öðelerini temizle
//        ClearProjectList();

//        // DatabaseManager'dan projeleri al
//        if (DatabaseManager.Instance != null)
//        {
//            List<Project> projects = DatabaseManager.Instance.GetAllProjects();

//            Debug.Log($"Veritabanýndan {projects.Count} proje yüklendi");

//            // Her proje için UI öðesi oluþtur
//            foreach (Project project in projects)
//            {
//                CreateProjectItem(project);
//            }
//        }
//        else
//        {
//            Debug.LogError("DatabaseManager bulunamadý!");
//        }
//    }

//    void CreateProjectItem(Project project)
//    {
//        if (projectItemPrefab == null || contentTransform == null)
//        {
//            Debug.LogError("ProjectItemPrefab veya ContentTransform atanmamýþ!");
//            return;
//        }

//        // Prefab'ý instantiate et
//        GameObject item = Instantiate(projectItemPrefab, contentTransform);

//        // Text bileþenlerini bul ve güncelle
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
//            Debug.LogWarning("ProjectNameText bulunamadý! Prefab yapýsýný kontrol et.");
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
//            Debug.LogWarning("ProjectDescText bulunamadý! Prefab yapýsýný kontrol et.");
//        }

//        // Proje ID'sini sakla (ileride kullanmak için)
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
//            // Tüm çocuk öðeleri sil
//            for (int i = contentTransform.childCount - 1; i >= 0; i--)
//            {
//                DestroyImmediate(contentTransform.GetChild(i).gameObject);
//            }
//        }
//    }


//}

//// Proje öðesi için yardýmcý script
//public class ProjectItem : MonoBehaviour
//{
//    public int projectId;
//    public string projectName;

//    // Proje öðesine týklandýðýnda çalýþacak
//    public void OnProjectClicked()
//    {
//        Debug.Log($"Proje týklandý: {projectName} (ID: {projectId})");

//        // Buraya proje detay sayfasýna geçiþ kodu eklenebilir
//        // SceneManager.LoadScene("ProjectDetailScene");
//        // veya panel deðiþimi yapýlabilir
//    }
//}
