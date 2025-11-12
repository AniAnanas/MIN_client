using System.Data;
using Client.Shared.Extensions;

namespace Client.Shared.Helpers.DB;
public class QueryResult : IDisposable
{
    public IDbConnection Connection { get; protected set; }

    public IDataReader Reader { get; protected set; }

    public IDbCommand Command { get; protected set; }

    public QueryResult(IDbConnection conn, IDataReader reader, IDbCommand command)
    {
        Connection = conn;
        Reader = reader;
        Command = command;
    }

    ~QueryResult()
    {
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (Reader != null)
            {
                Reader.Dispose();
                Reader = null;
            }

            if (Command != null)
            {
                Command.Dispose();
                Command = null;
            }

            if (Connection != null)
            {
                Connection.Dispose();
                Connection = null;
            }
        }
    }

    public bool Read()
    {
        if (Reader == null)
        {
            return false;
        }

        return Reader.Read();
    }

    public T Get<T>(string column)
    {
        if (Reader == null)
        {
            return default;
        }

        return Reader.Get<T>(Reader.GetOrdinal(column));
    }
}
