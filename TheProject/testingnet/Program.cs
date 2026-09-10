using System.Diagnostics;
using System.Net.Http.Json;

Console.WriteLine("--- Proxy-related env vars ---");
foreach (var name in new[] { "HTTP_PROXY", "HTTPS_PROXY", "NO_PROXY", "http_proxy", "https_proxy", "no_proxy" })
{
    var value = Environment.GetEnvironmentVariable(name);
    Console.WriteLine($"{name} = {value ?? "(not set)"}");
}

var url = "https://en.wikipedia.org/wiki/Special:Random";

await RunTest("Default HttpClient (proxy-aware)", new HttpClient { Timeout = TimeSpan.FromSeconds(15) }, url);
await RunTest("HttpClient with proxy explicitly disabled",
    new HttpClient(new SocketsHttpHandler { UseProxy = false }) { Timeout = TimeSpan.FromSeconds(15) }, url);

static async Task RunTest(string label, HttpClient client, string url)
{
    Console.WriteLine($"\n--- {label} ---");
    var sw = Stopwatch.StartNew();
    try
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.UserAgent.ParseAdd("CronjobWikiTodoBot/1.0 (student project, university course; contact: <your-email>)");
        var response = await client.SendAsync(request);
        sw.Stop();
        Console.WriteLine($"OK in {sw.ElapsedMilliseconds}ms — status {(int)response.StatusCode}, final URL: {response.RequestMessage?.RequestUri}");
    }
    catch (Exception ex)
    {
        sw.Stop();
        Console.WriteLine($"FAILED after {sw.ElapsedMilliseconds}ms: {ex.GetType().FullName}: {ex.Message}");
        for (var inner = ex.InnerException; inner is not null; inner = inner.InnerException)
            Console.WriteLine($"  caused by: {inner.GetType().FullName}: {inner.Message}");
    }
    finally
    {
        client.Dispose();
    }
}