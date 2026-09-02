using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Allow the frontend origin (adjust if you use a different host/port)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:8082", "http://localhost:8081")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();
app.UseCors("AllowFrontend");



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