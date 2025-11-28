using System.IO;
using System.Data;
using Microsoft.Data.Sqlite;
using MySql.Data.MySqlClient;
using Client.Shared.Helpers.DB;
using Client.Shared.Extensions;


namespace Client.Data;

public class Database
{
    private static Database? _instance;
    private static object _connection;
    private static Config _config;

    public static Database Instance
    {
        get
        {
            if (_instance == null)
                throw new InvalidOperationException("Database not initialized. Call Database.Initialize() first.");
            return _instance;
        }
    }

    public static void Initialize(bool reloading = false)
    {
        if (_instance != null && !reloading) return; // Already initialized
        if (reloading) Close();

        _instance = new Database();
        _config = Config.Read();

        try
        {
            string dbPath = Path.Combine(AppContext.BaseDirectory, _config.DatabasePath);
            _connection = new SqliteConnection($"Data Source={dbPath}");
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to connect to SQLite database: {ex}");
            throw;
        }

        CacheManager.StopAutoSave();
        if (CacheManager.AutoSaveSetup(_config.DBSaveInterval))
            Log.Info("AutoSave enabled.");
        else
            Log.Info("AutoSave hasn't been enabled.");
    }

    public static object Connection => _connection;
    public static Config Config => _config;

    public void CreateAccountsTable()
    {
        // Используем SqlTableCreator для создания таблицы Accounts
        var creator = new SqlTableCreator((SqliteConnection)_connection, new SqliteQueryCreator());
        var table = new SqlTable("Users",
            new SqlColumn("Id", MySqlDbType.Int32) { Primary = true },
            new SqlColumn("Username", MySqlDbType.Text) { Unique = true, NotNull = true },
            new SqlColumn("FirstName", MySqlDbType.Text) { DefaultValue = "No Name" }
        );
        creator.EnsureTableStructure(table);
    }

    public void CreateTable(SqlTable table)
    {
        var creator = new SqlTableCreator((SqliteConnection)_connection, new SqliteQueryCreator());
        creator.EnsureTableStructure(table);
    }

    public IDbCommand CreateCommand()
    {
        ((SqliteConnection)_connection).Open();
        return ((SqliteConnection)_connection).CreateCommand();
    }

    public void AddUser(string AccountID)
    {
        if (!CacheManager.Cache.GetCache<long>("Balance").TryGetValue(AccountID, out _))
        {
            CacheManager.Cache.GetCache<long>("Balance").Update(AccountID, 0);
        }
    }

    public void BalanceInitialize()
    {
        var balance = CacheManager.Cache.GetCache<long>("Balance");
        balance.MysqlQuery = "SELECT AccountID AS 'Key', Balance AS 'Value' FROM Accounts";
        balance.SqliteQuery = balance.MysqlQuery;
        balance.SaveMysqlQuery = "INSERT INTO Accounts (AccountID, Balance) VALUES (@key, @value) ON DUPLICATE KEY UPDATE Balance = @value";
        balance.SaveSqliteQuery = @"INSERT INTO Accounts (AccountID, Balance) VALUES (@key, @value) ON CONFLICT(AccountID) DO UPDATE SET Balance = @value";
        balance.Init();
    }

    public long GetBalance(int AccountID) => GetBalance(AccountID.ToString());
    public long GetBalance(string AccountID)
    {
        return CacheManager.Cache.GetCache<long>("Balance").GetValue(AccountID);
    }
    public void AddBalance(int AccountID, long amount) => AddBalance(AccountID.ToString(), amount);
    public void AddBalance(string AccountID, long amount)
    {
        long balance = GetBalance(AccountID);
        CacheManager.Cache.GetCache<long>("Balance").Update(AccountID, balance + amount);
    }
    public void RemoveBalance(int AccountID, long amount) => RemoveBalance(AccountID.ToString(), amount);
    public void RemoveBalance(string AccountID, long amount)
    {
        long balance = GetBalance(AccountID);
        CacheManager.Cache.GetCache<long>("Balance").Update(AccountID, balance - amount);
    }
    public dynamic CustomVoid(string query, object? param = null, bool output = false)
    {
        using var cmd = CreateCommand();
        cmd.CommandText = query;
        if (param != null)
        {
            var properties = param.GetType().GetProperties();
            foreach (var property in properties)
            {
                cmd.AddParameter($"@{property.Name}", property.GetValue(param));
            }
        }

        if (output)
        {
            var resultList = new List<Dictionary<string, object>>();

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var row = new Dictionary<string, object>();

                for (int i = 0; i < reader.FieldCount; i++)
                {
#pragma warning disable CS8601 // Possible null reference assignment.
                    row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
#pragma warning restore CS8601 // Possible null reference assignment.
                }

                resultList.Add(row);
            }

            return resultList;
        }
        else
        {
            cmd.ExecuteNonQuery();
            return new List<Dictionary<string, object>>();
        }
    }

    public static void Close()
    {
        if (_connection == null) return;
        ((SqliteConnection)_connection).Close();
    }
}
