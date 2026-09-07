using Npgsql;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
var app = builder.Build();

const string filePath = "/usr/src/app/counter/log.txt";
Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var pgHost = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "postgres";
var pgDb = Environment.GetEnvironmentVariable("POSTGRES_DB") ?? "pingpongdb";
var pgUser = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "postgres";
var pgPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "postgres";
var connectionString = $"Host={pgHost};Username={pgUser};Password={pgPassword};Database={pgDb};Pooling=true;";

using (var adminconn = new Npgsql.NpgsqlConnection(connectionString))
{
    adminconn.Open();
    using var cmd1 = new Npgsql.NpgsqlCommand($"CREATE TABLE IF NOT EXISTS pingpong_counter(key TEXT PRIMARY KEY, value INT);", adminconn);
    cmd1.ExecuteNonQuery();
    using var cmd2 = new Npgsql.NpgsqlCommand($"INSERT INTO pingpong_counter(key,value) VALUES('counter',0) ON CONFLICT(key) DO NOTHING;", adminconn);
    cmd2.ExecuteNonQuery();
}



app.MapGet("/pingpong", async (HttpContext ctx) =>
{
    ctx.Response.Headers.CacheControl = "no-store";

    long newCount;
    await using (var conn = new Npgsql.NpgsqlConnection(connectionString))
    {
        await conn.OpenAsync();
        using var tx = await conn.BeginTransactionAsync();
        using var cmd = new Npgsql.NpgsqlCommand($"UPDATE pingpong_counter SET value = value + 1 WHERE key = 'counter' RETURNING value;", conn, tx);
        var res = await cmd.ExecuteScalarAsync();
        newCount = res is null ? 0 : Convert.ToInt64(res);
        await tx.CommitAsync();

    }

   
    return Results.Text($"Ping / Pongs:{newCount}");
})
.WithName("pingpong");

app.MapGet("/count", async()=>{
    long current;
    await using(var conn = new Npgsql.NpgsqlConnection(connectionString))
    {
        await conn.OpenAsync();
        using var cmd = new Npgsql.NpgsqlCommand($"SELECT value FROM pingpong_counter WHERE key = 'counter';", conn);
        var res = await cmd.ExecuteScalarAsync();
        current = res is null ? 0 : Convert.ToInt64(res);
    }
   
    
    return Results.Text($"Ping / Pongs:{current}");
});

app.Run();