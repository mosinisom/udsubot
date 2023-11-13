using Npgsql;

public class DbService
{
    IConfiguration _cfg = null;
    ILogger<DbService> _log = null;
    public DbService(IConfiguration cfg, ILogger<DbService> log)
    {
        _cfg = cfg;
        _log = log;
    }

    NpgsqlConnection GetConnection()
    {
        var connString = _cfg.GetConnectionString("ConnStr");
        var conn = new NpgsqlConnection(connString);
        return conn;
    }

    public async Task<int> GetCountOfUsers()
    {
        var conn = GetConnection();
        await conn.OpenAsync();
        var cmd = new NpgsqlCommand("select count(*) from users", conn);
        var result = await cmd.ExecuteScalarAsync();
        await conn.CloseAsync();
        return Convert.ToInt32(result);
    }

    // get user by chat_id
    

}