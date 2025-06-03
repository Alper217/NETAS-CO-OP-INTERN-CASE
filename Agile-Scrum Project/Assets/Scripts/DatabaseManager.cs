using SQLite4Unity3d;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Mevcut NETAS-DATAS.db şemasıyla tamamen uyumlu DatabaseManager
/// ProjectInfoData ve ProjectTasks tablolarını kullanır
/// </summary>
public class DatabaseManager : MonoBehaviour
{
    private static DatabaseManager _instance;
    public static DatabaseManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<DatabaseManager>();
            return _instance;
        }
    }

    private SQLiteConnection _connection;
    private readonly object _lockObject = new object();

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        try
        {
            string dbName = "NETAS-DATAS.db";
            string dbPath = Path.Combine(Application.streamingAssetsPath, dbName);

            Debug.Log($"🗂️ Veritabanı yolu: {dbPath}");
            Debug.Log($"📂 Dosya var mı? {File.Exists(dbPath)}");

            if (!File.Exists(dbPath))
            {
                Debug.LogError($"❌ Veritabanı dosyası bulunamadı: {dbPath}");
                Debug.LogError("❌ NETAS-DATAS.db dosyasını StreamingAssets klasörüne kopyalayın!");
                return;
            }

            lock (_lockObject)
            {
                // Mevcut veritabanına sadece bağlan - tablo oluşturma
                _connection = new SQLiteConnection(dbPath, SQLiteOpenFlags.ReadWrite);

                Debug.Log("✅ Mevcut veritabanına bağlandı: " + dbPath);

                // Mevcut verileri listele
                ListExistingData();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Veritabanı başlatma hatası: {ex.Message}");
        }
    }

    private void ListExistingData()
    {
        try
        {
            Debug.Log("🔍 === MEVCUT VERİTABANI İÇERİĞİ ===");

            // ProjectInfoData tablosundan veri çek
            var projectCount = _connection.ExecuteScalar<int>("SELECT COUNT(*) FROM ProjectInfoData");
            Debug.Log($"📂 ProjectInfoData tablosunda {projectCount} proje var");

            if (projectCount > 0)
            {
                var projects = _connection.Query<ProjectInfoData>("SELECT * FROM ProjectInfoData");
                foreach (var project in projects)
                {
                    Debug.Log($"  📋 Proje {project.ID}: '{project.Name}' - {project.Created_Date}");
                }
            }

            // ProjectTasks tablosundan veri çek
            var taskCount = _connection.ExecuteScalar<int>("SELECT COUNT(*) FROM ProjectTasks");
            Debug.Log($"📝 ProjectTasks tablosunda {taskCount} task var");

            if (taskCount > 0)
            {
                var tasks = _connection.Query<ProjectTasks>("SELECT * FROM ProjectTasks LIMIT 5");
                foreach (var task in tasks)
                {
                    Debug.Log($"  🔸 Task {task.id}: '{task.title}' ({task.status}) - Proje: {task.projectId}");
                }
                if (taskCount > 5)
                {
                    Debug.Log($"  ... ve {taskCount - 5} task daha");
                }
            }

            Debug.Log("🔍 === VERİTABANI İÇERİK LİSTESİ BİTTİ ===");
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Veri listeleme hatası: {ex.Message}");
        }
    }

    public SQLiteConnection GetConnection()
    {
        if (_connection == null)
            InitializeDatabase();
        return _connection;
    }

    // CRUD Operations - Mevcut şema ile uyumlu
    public T GetById<T>(int id) where T : new()
    {
        lock (_lockObject)
        {
            try
            {
                // Direkt SQL sorgusu ile daha güvenli
                string tableName = typeof(T).Name;
                string idColumn = tableName == "ProjectInfoData" ? "ID" : "id";

                var result = _connection.Query<T>($"SELECT * FROM {tableName} WHERE {idColumn} = ?", id);
                return result.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ GetById hatası: {ex.Message}");
                return default(T);
            }
        }
    }

    public List<T> GetAll<T>() where T : new()
    {
        lock (_lockObject)
        {
            try
            {
                string tableName = typeof(T).Name;
                var result = _connection.Query<T>($"SELECT * FROM {tableName}");
                Debug.Log($"🔍 {tableName} tablosundan {result.Count} kayıt getirildi");
                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ GetAll hatası: {ex.Message}");
                return new List<T>();
            }
        }
    }

    public List<ProjectTasks> GetTasksByProjectId(int projectId)
    {
        lock (_lockObject)
        {
            try
            {
                var result = _connection.Query<ProjectTasks>(
                    "SELECT * FROM ProjectTasks WHERE projectId = ?", projectId);
                Debug.Log($"🔍 Proje {projectId} için {result.Count} task bulundu");
                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ GetTasksByProjectId hatası: {ex.Message}");
                return new List<ProjectTasks>();
            }
        }
    }

    public int Insert<T>(T item)
    {
        lock (_lockObject)
        {
            try
            {
                int result = _connection.Insert(item);
                Debug.Log($"✅ {typeof(T).Name} eklendi: {result}");
                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ Insert hatası: {ex.Message}");
                return 0;
            }
        }
    }

    public int UpdateItem<T>(T item)
    {
        lock (_lockObject)
        {
            try
            {
                int result = _connection.Update(item);
                Debug.Log($"✅ {typeof(T).Name} güncellendi: {result}");
                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ UpdateItem hatası: {ex.Message}");
                return 0;
            }
        }
    }

    public int Delete<T>(T item)
    {
        lock (_lockObject)
        {
            try
            {
                int result = _connection.Delete(item);
                Debug.Log($"✅ {typeof(T).Name} silindi: {result}");
                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ Delete hatası: {ex.Message}");
                return 0;
            }
        }
    }

    public void ExecuteTransaction(System.Action action)
    {
        lock (_lockObject)
        {
            try
            {
                _connection.BeginTransaction();
                action?.Invoke();
                _connection.Commit();
                Debug.Log("✅ Transaction tamamlandı");
            }
            catch (Exception ex)
            {
                _connection.Rollback();
                Debug.LogError($"❌ Transaction hatası: {ex.Message}");
                throw;
            }
        }
    }

    private void OnDestroy()
    {
        lock (_lockObject)
        {
            _connection?.Close();
            _connection?.Dispose();
        }
    }
}