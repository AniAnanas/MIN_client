using System.Text;
using MySql.Data.MySqlClient;
using Client.Shared.Exceptions;
using Client.Shared.Extensions;

namespace Client.Shared.Helpers.DB
{
    /// <summary>
    /// A Generic Query Creator (abstract)
    /// </summary>
    public abstract class GenericQueryCreator
    {
        protected private static Random rand = new();

        /// <summary>
        /// Escapes the table name
        /// </summary>
        /// <param name="table">The name of the table to be escaped</param>
        /// <returns></returns>
        protected abstract string EscapeTableName(string table);

        /// <summary>
        /// Creates a table from a SqlTable object.
        /// </summary>
        /// <param name="table">The SqlTable to create the table from</param>
        /// <returns>The sql query for the table creation.</returns>
        public abstract string CreateTable(SqlTable table);

        /// <summary>
        /// Renames the given table.
        /// </summary>
        /// <param name="from">Old name of the table</param>
        /// <param name="to">New name of the table</param>
        /// <returns>The sql query for renaming the table.</returns>
        public abstract string RenameTable(string from, string to);

        /// <summary>
        /// Alter a table from source to destination
        /// </summary>
        /// <param name="from">Must have name and column names. Column types are not required</param>
        /// <param name="to">Must have column names and column types.</param>
        /// <returns>The SQL Query</returns>
        public string AlterTable(SqlTable from, SqlTable to)
        {
            string text = rand.NextString(20);
            string text2 = EscapeTableName(from.Name);
            string text3 = EscapeTableName("{0}_{1}".SFormat(text, from.Name));
            string text4 = RenameTable(text2, text3);
            string text5 = CreateTable(to);
            string text6 = string.Join(", ", from c in @from.Columns
                                             where to.Columns.Any((c2) => c2.Name == c.Name)
                                             select "`" + c.Name + "`");
            string text7 = "INSERT INTO {0} ({1}) SELECT {1} FROM {2}".SFormat(text2, text6, text3);
            string text8 = "DROP TABLE {0}".SFormat(text3);
            return "{0}; {1}; {2}; {3};".SFormat(text4, text5, text7, text8);
        }

        /// <summary>
        /// Check for errors in the columns.
        /// </summary>
        /// <param name="columns"></param>
        /// <exception cref="SqlColumnException"></exception>
        public static void ValidateSqlColumnType(List<SqlColumn> columns)
        {
            columns.ForEach(delegate (SqlColumn x)
            {
                if (x.DefaultCurrentTimestamp && x.Type != MySqlDbType.DateTime)
                {
                    throw new SqlColumnException("Can't set to true SqlColumn.DefaultCurrentTimestamp when the MySqlDbType is not DateTime");
                }
            });
        }

        /// <summary>
        /// Deletes row(s).
        /// </summary>
        /// <param name="table">The table to delete the row from</param>
        /// <param name="wheres">The SQL query</param>
        /// <returns></returns>
        public string DeleteRow(string table, List<SqlValue> wheres)
        {
            return "DELETE FROM {0} {1}".SFormat(EscapeTableName(table), BuildWhere(wheres));
        }

        /// <summary>
        /// A UPDATE Query
        /// </summary>
        /// <param name="table">The table to update</param>
        /// <param name="values">The values to change</param>
        /// <param name="wheres"></param>
        /// <returns>The SQL query</returns>
        /// <exception cref="ArgumentException"></exception>
        public string UpdateValue(string table, List<SqlValue> values, List<SqlValue> wheres)
        {
            if (values.Count == 0)
            {
                throw new ArgumentException("No values supplied");
            }

            return "UPDATE {0} SET {1} {2}".SFormat(EscapeTableName(table), string.Join(", ", values.Select((v) => v.Name + " = " + v.Value)), BuildWhere(wheres));
        }

        /// <summary>
        /// A SELECT query to get all columns
        /// </summary>
        /// <param name="table">The table to select from</param>
        /// <param name="wheres"></param>
        /// <returns>The SQL query</returns>
        public string ReadColumn(string table, List<SqlValue> wheres)
        {
            return "SELECT * FROM {0} {1}".SFormat(EscapeTableName(table), BuildWhere(wheres));
        }

        /// <summary>
        /// A INSERT query
        /// </summary>
        /// <param name="table">The table to insert to</param>
        /// <param name="values">The SQL Query</param>
        /// <returns></returns>
        public string InsertValues(string table, List<SqlValue> values)
        {
            StringBuilder stringBuilder = new();
            StringBuilder stringBuilder2 = new();
            int num = 0;
            foreach (SqlValue value in values)
            {
                stringBuilder.Append(value.Name);
                stringBuilder2.Append(value.Value.ToString());
                if (num != values.Count - 1)
                {
                    stringBuilder.Append(", ");
                    stringBuilder2.Append(", ");
                }

                num++;
            }

            return "INSERT INTO {0} ({1}) VALUES ({2})".SFormat(EscapeTableName(table), stringBuilder, stringBuilder2);
        }

        /// <summary>
        /// Builds the SQL WHERE clause
        /// </summary>
        /// <param name="wheres"></param>
        /// <returns></returns>
        protected static string BuildWhere(List<SqlValue> wheres)
        {
            if (wheres.Count == 0)
            {
                return string.Empty;
            }

            return "WHERE {0}".SFormat(string.Join(", ", wheres.Select((v) => v.Name + " = " + v.Value)));
        }
    }
}
