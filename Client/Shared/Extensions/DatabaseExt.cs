using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.Data;
using Client.Shared.Helpers.DB;

namespace Client.Shared.Extensions;

public static class DbExt
{
    private static readonly Dictionary<Type, Func<IDataReader, int, object>> ReadFuncs = new()
    {
        {
            typeof(bool),
            (s, i) => s.GetBoolean(i)
        },
        {
            typeof(bool?),
            (s, i) => !s.IsDBNull(i) ? s.GetBoolean(i) : null
        },
        {
            typeof(byte),
            (s, i) => s.GetByte(i)
        },
        {
            typeof(byte?),
            (s, i) => !s.IsDBNull(i) ? s.GetByte(i) : null
        },
        {
            typeof(short),
            (s, i) => s.GetInt16(i)
        },
        {
            typeof(short?),
            (s, i) => !s.IsDBNull(i) ? s.GetInt16(i) : null
        },
        {
            typeof(int),
            (s, i) => s.GetInt32(i)
        },
        {
            typeof(int?),
            (s, i) => !s.IsDBNull(i) ? s.GetInt32(i) : null
        },
        {
            typeof(long),
            (s, i) => s.GetInt64(i)
        },
        {
            typeof(long?),
            (s, i) => !s.IsDBNull(i) ? s.GetInt64(i) : null
        },
        {
            typeof(string),
            (s, i) => s.GetString(i)
        },
        {
            typeof(decimal),
            (s, i) => s.GetDecimal(i)
        },
        {
            typeof(decimal?),
            (s, i) => !s.IsDBNull(i) ? s.GetDecimal(i) : null
        },
        {
            typeof(float),
            (s, i) => s.GetFloat(i)
        },
        {
            typeof(float?),
            (s, i) => !s.IsDBNull(i) ? s.GetFloat(i) : null
        },
        {
            typeof(double),
            (s, i) => s.GetDouble(i)
        },
        {
            typeof(double?),
            (s, i) => !s.IsDBNull(i) ? s.GetDouble(i) : null
        },
        {
            typeof(DateTime),
            (s, i) => !s.IsDBNull(i) ? s.GetDateTime(i) : null
        },
        {
            typeof(object),
            (s, i) => s.GetValue(i)
        }
    };

    /// <summary>
    /// Executes a query on a database.
    /// </summary>
    /// <param name="olddb">Database to query</param>
    /// <param name="query">Query string with parameters as @0, @1, etc.</param>
    /// <param name="args">Parameters to be put in the query</param>
    /// <returns>The number of rows affected by query</returns>
    public static int Query(this IDbConnection olddb, string query, params object[] args)
    {
        using IDbConnection dbConnection = olddb.CloneEx();
        dbConnection.Open();
        using IDbCommand dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = query;
        for (int i = 0; i < args.Length; i++)
        {
            dbCommand.AddParameter("@" + i, args[i] ?? DBNull.Value);
        }

        return dbCommand.ExecuteNonQuery();
    }

    /// <summary>
    /// Executes a query on a database.
    /// </summary>
    /// <param name="olddb">Database to query</param>
    /// <param name="query">Query string with parameters as @0, @1, etc.</param>
    /// <param name="args">Parameters to be put in the query</param>
    /// <returns>Query result as IDataReader</returns>
    /// <exception cref="Exception"></exception>
    public static QueryResult QueryReader(this IDbConnection olddb, string query, params object[] args)
    {
        IDbConnection dbConnection = olddb.CloneEx();
        try
        {
            dbConnection.Open();
            IDbCommand dbCommand = dbConnection.CreateCommand();
            dbCommand.CommandText = query;
            for (int i = 0; i < args.Length; i++)
            {
                dbCommand.AddParameter("@" + i, args[i]);
            }

            return new QueryResult(dbConnection, dbCommand.ExecuteReader(), dbCommand);
        }
        catch (Exception innerException)
        {
            throw new Exception("Failed to execute query. See inner exception for details.", innerException);
        }
    }

    /// <summary>
    /// Executes a query on a database, returning the first column of the first row of the result set.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="olddb">Database to query</param>
    /// <param name="query">Query string with parameters as @0, @1, etc.</param>
    /// <param name="args">Parameters to be put in the query</param>
    /// <returns></returns>
    public static T QueryScalar<T>(this IDbConnection olddb, string query, params object[] args)
    {
        using IDbConnection dbConnection = olddb.CloneEx();
        dbConnection.Open();
        using IDbCommand dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = query;
        for (int i = 0; i < args.Length; i++)
        {
            dbCommand.AddParameter("@" + i, args[i]);
        }

        object obj = dbCommand.ExecuteScalar();
        if (obj.GetType() != typeof(T) && typeof(IConvertible).IsAssignableFrom(obj.GetType()))
        {
            return (T)Convert.ChangeType(obj, typeof(T));
        }

        return (T)obj;
    }

    public static QueryResult QueryReaderDict(this IDbConnection olddb, string query, Dictionary<string, object> values)
    {
        IDbConnection dbConnection = olddb.CloneEx();
        dbConnection.Open();
        IDbCommand dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = query;
        foreach (KeyValuePair<string, object> value in values)
        {
            dbCommand.AddParameter("@" + value.Key, value.Value);
        }

        return new QueryResult(dbConnection, dbCommand.ExecuteReader(), dbCommand);
    }

    public static IDbDataParameter AddParameter(this IDbCommand command, string name, object data)
    {
        IDbDataParameter dbDataParameter = command.CreateParameter();
        dbDataParameter.ParameterName = name;
        dbDataParameter.Value = data;
        command.Parameters.Add(dbDataParameter);
        return dbDataParameter;
    }

    public static IDbConnection CloneEx(this IDbConnection conn)
    {
        IDbConnection obj = (IDbConnection)Activator.CreateInstance(conn.GetType());
        obj.ConnectionString = conn.ConnectionString;
        return obj;
    }

    public static SqlType GetSqlType(this IDbConnection conn)
    {
        switch (conn.GetType().Name)
        {
            case "SqliteConnection":
            case "SQLiteConnection":
                return SqlType.Sqlite;
            case "MySqlConnection":
                return SqlType.Mysql;
            default:
                return SqlType.Unknown;
        }
    }

    public static T Get<T>(this IDataReader reader, string column)
    {
        return reader.Get<T>(reader.GetOrdinal(column));
    }

    public static T Get<T>(this IDataReader reader, int column)
    {
        if (reader.IsDBNull(column))
        {
            return default;
        }

        if (ReadFuncs.ContainsKey(typeof(T)))
        {
            return (T)ReadFuncs[typeof(T)](reader, column);
        }

        Type fieldType;
        if (typeof(T) != (fieldType = reader.GetFieldType(column)))
        {
            string name = reader.GetName(column);
            throw new InvalidCastException($"Received type '{typeof(T).Name}', however column '{name}' expects type '{fieldType.Name}'");
        }

        if (reader.IsDBNull(column))
        {
            return default;
        }

        return (T)reader.GetValue(column);
    }
}
