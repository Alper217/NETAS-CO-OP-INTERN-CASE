using UnityEngine;
using System.IO;
using SQLite4Unity3d;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

public class DB_Manager : MonoBehaviour
{
    public GameObject itemPrefab;
    public Transform contentParent;

    private SQLiteConnection _connection;

    void Start()
    {
#if UNITY_STANDALONE_WIN
        string dbName = "NETAS-DATAS.db";
        string filepath = Path.Combine(Application.streamingAssetsPath, dbName);
        string persistentPath = Path.Combine(Application.persistentDataPath, dbName);

        if (!File.Exists(persistentPath))
        {
            File.Copy(filepath, persistentPath);
        }

        _connection = new SQLiteConnection(persistentPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);
        LoadData();
#endif
    }

    void LoadData()
    {
        var projects = _connection.Table<Project_Info_Data>().ToList();
        Debug.Log("Toplam proje: " + projects.Count);

        foreach (var project in projects)
        {
            var item = Instantiate(itemPrefab, contentParent);

            item.transform.Find("NameText").GetComponent<Text>().text = project.Name;
            item.transform.Find("DescText").GetComponent<Text>().text = project.Description;
            item.transform.Find("DateText").GetComponent<Text>().text = project.Created_Date;
        }
    }
}
