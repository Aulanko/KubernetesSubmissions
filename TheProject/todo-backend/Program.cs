using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT") ?? "3002";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");


// Allow the frontend origin (adjust if you use a different host/port)

var corsOrigins = new[]
{
    Environment.GetEnvironmentVariable("CORS_ORIGIN1"),
    Environment.GetEnvironmentVariable("CORS_ORIGIN2")
}.OfType<string>().ToArray();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(corsOrigins).AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();
app.UseCors();



// In-memory store
var todos = new List<TodoItem>();

app.MapGet("/todo-backend/todos", () => Results.Json(todos));



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
        todos.Add(item);
        return Results.Created($"/todos/{item.Id}", item);

    }
    catch {
        return Results.BadRequest(new { error = "catch method catched an error, probably invalid JSON" });
    }
});

app.Run();


// Simple DTOs / models
record TodoItem(Guid Id, string Text, DateTime CreatedAt);
record TodoCreateDto(string Text);