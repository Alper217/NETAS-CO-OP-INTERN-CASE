using SQLite4Unity3d;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Compatible DatabaseManager for existing NETAS-DATAS.db schema
/// Manages ProjectInfoData and ProjectTasks tables
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

            Debug.Log($"Database path: {dbPath}");
            Debug.Log($"File exists: {File.Exists(dbPath)}");

            if (!File.Exists(dbPath))
            {
                Debug.LogError($"Database file not found: {dbPath}");
                Debug.LogError("Please copy NETAS-DATAS.db file to StreamingAssets folder!");
                return;
            }

            lock (_lockObject)
            {
                _connection = new SQLiteConnection(dbPath, SQLiteOpenFlags.ReadWrite);

                Debug.Log("Connected to existing database: " + dbPath);

                ListExistingData();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Database initialization error: {ex.Message}");
        }
    }

    private void ListExistingData()
    {
        try
        {
            Debug.Log("=== EXISTING DATABASE CONTENT ===");

            var projectCount = _connection.ExecuteScalar<int>("SELECT COUNT(*) FROM ProjectInfoData");
            Debug.Log($"ProjectInfoData table contains {projectCount} projects");

            if (projectCount > 0)
            {
                var projects = _connection.Query<ProjectInfoData>("SELECT * FROM ProjectInfoData");
                foreach (var project in projects)
                {
                    Debug.Log($"  Project {project.ID}: '{project.Name}' - {project.Created_Date}");
                }
            }

            var taskCount = _connection.ExecuteScalar<int>("SELECT COUNT(*) FROM ProjectTasks");
            Debug.Log($"ProjectTasks table contains {taskCount} tasks");

            if (taskCount > 0)
            {
                var tasks = _connection.Query<ProjectTasks>("SELECT * FROM ProjectTasks LIMIT 5");
                foreach (var task in tasks)
                {
                    Debug.Log($"  Task {task.id}: '{task.title}' ({task.status}) - Project: {task.projectId}");
                }
                if (taskCount > 5)
                {
                    Debug.Log($"  ... and {taskCount - 5} more tasks");
                }
            }

            Debug.Log("=== DATABASE CONTENT LISTING COMPLETE ===");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Data listing error: {ex.Message}");
        }
    }

    public SQLiteConnection GetConnection()
    {
        if (_connection == null)
            InitializeDatabase();
        return _connection;
    }

    // CRUD Operations - Compatible with existing schema
    public T GetById<T>(int id) where T : new()
    {
        lock (_lockObject)
        {
            try
            {
                string tableName = typeof(T).Name;
                string idColumn = tableName == "ProjectInfoData" ? "ID" : "id";

                var result = _connection.Query<T>($"SELECT * FROM {tableName} WHERE {idColumn} = ?", id);
                return result.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Debug.LogError($"GetById error: {ex.Message}");
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
                Debug.Log($"Retrieved {result.Count} records from {tableName} table");
                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"GetAll error: {ex.Message}");
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
                Debug.Log($"Found {result.Count} tasks for project {projectId}");
                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"GetTasksByProjectId error: {ex.Message}");
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
                Debug.Log($"{typeof(T).Name} inserted successfully: {result}");
                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Insert error: {ex.Message}");
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
                Debug.Log($"{typeof(T).Name} updated successfully: {result}");
                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"UpdateItem error: {ex.Message}");
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
                Debug.Log($"{typeof(T).Name} deleted successfully: {result}");
                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Delete error: {ex.Message}");
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
                Debug.Log("Transaction completed successfully");
            }
            catch (Exception ex)
            {
                _connection.Rollback();
                Debug.LogError($"Transaction error: {ex.Message}");
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