var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
var app = builder.Build();

const string filePath = "/usr/src/app/counter/log.txt";
Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var counter = 0;
if (File.Exists(filePath))
{
    var existingLines = File.ReadAllLines(filePath);
    if (existingLines.Length > 0)
    {
        var lastLine = existingLines[^1];
        var parts = lastLine.Split(':');
        if (parts.Length == 2 && int.TryParse(parts[1].Trim(), out var restored))
        {
            counter = restored;
        }
    }
}

app.MapGet("/pingpong", async (HttpContext ctx) =>
{
    ctx.Response.Headers.CacheControl = "no-store";

    var newCount = Interlocked.Increment(ref counter);
    var answer = "pong " + newCount;

    var line = $"Ping / Pongs: {newCount}";
    await File.AppendAllTextAsync(filePath, line + Environment.NewLine);

    return answer;
})
.WithName("pingpong");

app.Run();