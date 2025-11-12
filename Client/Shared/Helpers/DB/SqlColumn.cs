namespace Client.Shared.Helpers.DB;

public class SqlColumn(string name, MySql.Data.MySqlClient.MySqlDbType type, int? length)
{
    public string Name { get; set; } = name;
    public MySql.Data.MySqlClient.MySqlDbType Type { get; set; } = type;
    /// <summary>
    /// Sets/Gets if it's unique
    /// </summary>
    public bool Unique { get; set; }
    /// <summary>
    /// Sets/Gets if it's primary key
    /// </summary>
    public bool Primary { get; set; }
    /// <summary>
    /// Sets/Gets if it autoincrements
    /// </summary>
    public bool AutoIncrement { get; set; }
    /// <summary>
    /// Sets/Gets if it can be or not null
    /// </summary>
    public bool NotNull { get; set; }
    /// <summary>
    /// Sets the default value
    /// </summary>
    public string DefaultValue { get; set; }
    /// <summary>
    /// Use on DateTime only, if true, sets the default value to the current date when creating the row.
    /// </summary>
    public bool DefaultCurrentTimestamp { get; set; }
    /// <summary>
    /// Length of the data type, null = default
    /// </summary>
    public int? Length { get; set; } = length;

    public SqlColumn(string name, MySql.Data.MySqlClient.MySqlDbType type) : this(name, type, null) { }
}
