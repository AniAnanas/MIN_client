using System.Data;
using MySql.Data.MySqlClient;
using Client.Shared.Interfaces;
using Client.Shared.Extensions;

namespace Client.Shared.Helpers.DB;

public class SqlTableCreator(IDbConnection db, IQueryBuilder provider)
{
    private IDbConnection database = db;

    private IQueryBuilder creator = provider;

    public bool EnsureTableStructure(SqlTable table)
    {
        List<string> columns = GetColumns(table);
        if (columns.Count > 0)
        {
            if (!table.Columns.All((SqlColumn c) => columns.Contains(c.Name)) || !columns.All((c) => table.Columns.Any((SqlColumn c2) => c2.Name == c)))
            {
                SqlTable from = new(table.Name, columns.Select((s) => new SqlColumn(s, MySqlDbType.String)).ToList());
                database.Query(creator.AlterTable(from, table));
            }

            return false;
        }

        database.Query(creator.CreateTable(table));
        return true;
    }

    public List<string> GetColumns(SqlTable table)
    {
        List<string> list = [];
        switch (database.GetSqlType())
        {
            case SqlType.Sqlite:
                {
                    using (QueryResult queryResult2 = database.QueryReader("PRAGMA table_info({0})".SFormat(table.Name)))
                    {
                        while (queryResult2.Read())
                        {
                            list.Add(queryResult2.Get<string>("name"));
                        }
                    }

                    break;
                }
            case SqlType.Mysql:
                {
                    using (QueryResult queryResult = database.QueryReader("SELECT COLUMN_NAME FROM information_schema.COLUMNS WHERE TABLE_NAME=@0 AND TABLE_SCHEMA=@1", table.Name, database.Database))
                    {
                        while (queryResult.Read())
                        {
                            list.Add(queryResult.Get<string>("COLUMN_NAME"));
                        }
                    }

                    break;
                }
            default:
                throw new NotSupportedException();
        }

        return list;
    }

    public void DeleteRow(string table, List<SqlValue> wheres)
    {
        database.Query(creator.DeleteRow(table, wheres));
    }
}
