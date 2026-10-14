using MySql.Data.MySqlClient;

public static class DataReaderExtensions
{
    public static bool HasColumn(this MySqlDataReader reader, string columnName)
    {
        try { reader.GetOrdinal(columnName); return true; }
        catch { return false; }
    }
}
