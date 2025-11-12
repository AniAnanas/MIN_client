using MySql.Data.MySqlClient;
using Client.Shared.Interfaces;

namespace Client.Shared.Helpers.DB;

/// <summary>
/// Query Creator for Sqlite
/// </summary>
public class SqliteQueryCreator : GenericQueryCreator, IQueryBuilder
{
    private static readonly Dictionary<MySqlDbType, string> TypesAsStrings = new()
    {
        { MySqlDbType.VarChar, "TEXT" },
        { MySqlDbType.String, "TEXT" },
        { MySqlDbType.Text, "TEXT" },
        { MySqlDbType.TinyText, "TEXT" },
        { MySqlDbType.MediumText, "TEXT" },
        { MySqlDbType.LongText, "TEXT" },
        { MySqlDbType.Float, "REAL" },
        { MySqlDbType.Double, "REAL" },
        { MySqlDbType.Int32, "INTEGER" },
        { MySqlDbType.Blob, "BLOB" },
        { MySqlDbType.Int64, "BIGINT" },
        { MySqlDbType.DateTime, "DATETIME" }
    };

    /// <summary>
    /// Creates a table from a SqlTable object.
    /// </summary>
    /// <param name="table">The SqlTable to create the table from</param>
    /// <returns>The sql query for the table creation.</returns>
    public override string CreateTable(SqlTable table)
    {
        ValidateSqlColumnType(table.Columns);
        IEnumerable<string> values = table.Columns.Select(
            (SqlColumn c) => "'{0}' {1} {2} {3} {4} {5}".SFormat(
                c.Name, 
                DbTypeToString(c.Type, c.Length), 
                c.Primary 
                    ? "PRIMARY KEY" 
                    : "", 
                c.AutoIncrement 
                    ? "AUTOINCREMENT" 
                    : "", 
                c.NotNull 
                    ? "NOT NULL" 
                    : "", 
                c.DefaultCurrentTimestamp 
                    ? "DEFAULT CURRENT_TIMESTAMP" 
                    : ""
            )
        );
        IEnumerable<string> enumerable = from c in table.Columns
                                         where c.Unique
                                         select c.Name;
        return "CREATE TABLE {0} ({1} {2})".SFormat(
            EscapeTableName(table.Name), 
            string.Join(", ", values), 
            (enumerable.Any()) 
                ? ", UNIQUE({0})".SFormat(string.Join(", ", enumerable)) 
                : "");
    }

    /// <summary>
    /// Renames the given table.
    /// </summary>
    /// <param name="from">Old name of the table</param>
    /// <param name="to">New name of the table</param>
    /// <returns>The sql query for renaming the table.</returns>
    public override string RenameTable(string from, string to)
    {
        return "ALTER TABLE {0} RENAME TO {1}".SFormat(from, to);
    }

    /// <summary>
    /// Converts the MySqlDbType enum to it's string representation.
    /// </summary>
    /// <param name="type">The MySqlDbType type</param>
    /// <param name="length">The length of the datatype</param>
    /// <returns>The string representation</returns>
    /// <exception cref="NotImplementedException"></exception>
    public string DbTypeToString(MySqlDbType type, int? length)
    {
        if (TypesAsStrings.TryGetValue(type, out var value))
        {
            return value;
        }

        throw new NotImplementedException(Enum.GetName(type));
    }

    /// <summary>
    /// Escapes the table name
    /// </summary>
    /// <param name="table"></param>
    /// <returns>The name of the table to be escaped</returns>
    protected override string EscapeTableName(string table)
    {
        return "'" + table + "'";
    }
}
