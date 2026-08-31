using System.Net.Http;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };

const string SharedCounterFile = "/usr/src/app/counter/log.txt";
Directory.CreateDirectory(Path.GetDirectoryName(SharedCounterFile)!);

app.MapGet("/", async () =>
{
    string pingpongLine = "Ping / Pongs: 0";
    

    try{
       
        var resp = await http.GetAsync("http://pingpong-service:2345/count");
        if (resp.IsSuccessStatusCode)
        {
            pingpongLine = (await resp.Content.ReadAsStringAsync()).Trim();
        }

    }catch(Exception ex){
    pingpongLine = $"error: {ex.Message}";
    }

    

    var timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
    var randomId = Guid.NewGuid().ToString();

    return Results.Text($"{timestamp}: {randomId}.\n{pingpongLine}");
});

app.Run();