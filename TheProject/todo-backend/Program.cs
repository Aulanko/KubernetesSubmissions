using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT") ?? "3002";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

var pgHost = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "postgres";
var pgDb = Environment.GetEnvironmentVariable("POSTGRES_DB") ?? "tododb";
var pgUser = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "postgres";
var pgPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "postgres";

var connectionString = $"Host={pgHost}; Username={pgUser}; Password={pgPassword}; Database={pgDb}; Pooling=true;";

using(var adminconn = new Npgsql.NpgsqlConnection(connectionString))
{
    adminconn.Open();
    using var cmd1 = new Npgsql.NpgsqlCommand("CREATE TABLE IF NOT EXISTS todos(id UUID PRIMARY KEY, text TEXT NOT NULL, date TIMESTAMPTZ);", adminconn);
    cmd1.ExecuteNonQuery();

}

// Allow the frontend origin (adjust if you use a different host/port)

var corsOrigins = new[]
{
    Environment.GetEnvironmentVariable("CORS_ORIGIN1" ?? "http://localhost:8081"),
    Environment.GetEnvironmentVariable("CORS_ORIGIN2" ?? "http://localhost:8082")
}.OfType<string>().ToArray();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(corsOrigins).AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();
app.UseCors();




app.MapGet("/todo-backend/todos", async() =>
{
    var items = new List<TodoItem>();
    await using(var conn = new Npgsql.NpgsqlConnection(connectionString))
    {
        await conn.OpenAsync();
        using var cmd = new Npgsql.NpgsqlCommand($"SELECT id, text, date FROM todos;", conn);
        using var reader = await cmd.ExecuteReaderAsync();
        while(await reader.ReadAsync())
        {
            var id = reader.GetGuid(0);
            var text = reader.GetString(1);
            var date = reader.GetDateTime(2);
            items.Add(new TodoItem(id, text, date));
        }
    }
    return Results.Json(items);
});



app.MapPost("/todo-backend/todos", async (HttpContext ctx) => {
    try {
        var dto = await ctx.Request.ReadFromJsonAsync<TodoCreateDto>();
        if (dto == null || string.IsNullOrWhiteSpace(dto.Text)) {
            return Results.BadRequest(new { error = "Text is required" });
        }
        if (dto.Text.Length > 140) {
            return Results.BadRequest(new { error = "Text is required to be less than 140 chars" });
        }

        var item = new TodoItem(Guid.NewGuid(), dto.Text.Trim(), DateTime.UtcNow);
        await using(var conn = new Npgsql.NpgsqlConnection(connectionString))
        {
            await conn.OpenAsync();
            using var cmd = new Npgsql.NpgsqlCommand($"INSERT INTO todos(id, text, date) VALUES(@id, @text, @date);", conn);
            cmd.Parameters.AddWithValue("id", item.Id);
            cmd.Parameters.AddWithValue("text", item.Text);
            cmd.Parameters.AddWithValue("date", item.CreatedAt);
            await cmd.ExecuteNonQueryAsync();
        }
        return Results.Created($"/todo-backend/todos/{item.Id}", item);

    }
    catch {
        return Results.BadRequest(new { error = "catch method catched an error, probably invalid JSON" });
    }
});

app.Run();


// Simple DTOs / models
record TodoItem(Guid Id, string Text, DateTime CreatedAt);
record TodoCreateDto(string Text);