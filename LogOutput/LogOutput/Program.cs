var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

const string SharedCounterFile = "/usr/src/app/counter/log.txt";
Directory.CreateDirectory(Path.GetDirectoryName(SharedCounterFile)!);

app.MapGet("/", async () =>
{
    var pingCount = 0;

    if (File.Exists(SharedCounterFile))
    {
        var lines = await File.ReadAllLinesAsync(SharedCounterFile);
        var lastLine = lines.LastOrDefault();

        if (!string.IsNullOrWhiteSpace(lastLine) && lastLine.StartsWith("Ping / Pongs:"))
        {
            var value = lastLine.Replace("Ping / Pongs:", "").Trim();
            int.TryParse(value, out pingCount);
        }
    }

    var timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
    var randomId = Guid.NewGuid().ToString();

    return Results.Text($"{timestamp}: {randomId}.\nPing / Pongs: {pingCount}");
});

app.Run();