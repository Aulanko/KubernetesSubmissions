var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

const string PingPongFilePath = "/usr/src/app/counter/log.txt";

app.MapGet("/", async () =>
{
    if (!File.Exists(PingPongFilePath))
    {
        return Results.Json(new { status = "Waiting for the file.." });
    }

    var lines = await File.ReadAllLinesAsync(PingPongFilePath);
    var lastLine = lines.LastOrDefault() ?? "Ping / Pongs: 0";

    var timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
    var randomId = Guid.NewGuid().ToString();

    return Results.Text($"{timestamp}: {randomId}.\n{lastLine}");
});

app.Run();