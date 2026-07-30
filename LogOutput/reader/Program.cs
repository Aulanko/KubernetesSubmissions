
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

const string filePath = "/usr/src/app/files/log.txt";
const string PingPongFilePath = "usr/src/app/counter/log.txt";

app.MapGet("/", async()=> 
{
    if (!File.Exists(filePath) || !File.Exists(PingPongFilePath))
    {
    return Results.Json( new { status = "Waiting for the file.."});
    }
    var lines = await(File.ReadAllLinesAsync(filePath));
    var lastLine = lines.Length >0? lines[^1]:string.Empty;
    var pings = await(File.ReadAllLinesAsync(PingPongFilePath));
    var lastPing = pings.Length >0? pings[^1]:string.Empty;
    return Results.Text($"{lastLine}\n{lastPing}");

});

app.Run();