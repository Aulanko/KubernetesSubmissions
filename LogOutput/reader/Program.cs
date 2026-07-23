
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

const string filePath = "/usr/src/app/files/log.txt";

app.MapGet("/", async()=> 
{
    if (!File.Exists(filePath))
    {
    return Results.Json( new { status = "Waiting for the file.."});
    }
    var lines = await(File.ReadAllLinesAsync(filePath));
    var lastLine = lines.Length >0? lines[^1]:string.Empty;
    return Results.Text(lastLine);

});

app.Run();