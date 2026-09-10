using System.Net.Http.Json;

var backendUrl = Environment.GetEnvironmentVariable("BACKEND_URL") ?? "http://todo-backend:1234/todo-backend/todos";
var randomWikipediaUrl = Environment.GetEnvironmentVariable("RANDOM_WIKIPEDIA_URL") ?? "https://en.wikipedia.org/wiki/Special:Random";

using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };

var request = new HttpRequestMessage(HttpMethod.Get, randomWikipediaUrl);
request.Headers.UserAgent.ParseAdd("CronjobWikiTodoBot/1.0 (student project, university course; contact: <your-email>)");

var getRandomResponse = await httpClient.SendAsync(request);
var finalUrl = getRandomResponse.RequestMessage?.RequestUri?.ToString();

if (finalUrl is not null)
{
    var text = $"Read {finalUrl}";
    var postResponse = await httpClient.PostAsJsonAsync(backendUrl, new { text });
    postResponse.EnsureSuccessStatusCode();
    Console.WriteLine($"Created todo: {text}");
}
else
{
    Console.WriteLine("Failed to get a random Wikipedia URL.");
}