

var randomWords = new List<string> {"jeje","koira","kissa","koroko","lehmä"};

var random = new Random();

var currentWord = randomWords[random.Next(randomWords.Count)];
var currentTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");


_ = Task.Run(() =>
{
    while (true)
    {
        currentWord = randomWords[random.Next(randomWords.Count)];
        currentTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        Console.WriteLine($"[{currentTimestamp}] {currentWord}");
        Thread.Sleep(5000);
    }
});


var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", ()=> Results.Json(new
{
    timestamp = currentTimestamp,
    randomString = currentWord

}));

app.Run();