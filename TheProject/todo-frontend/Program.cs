using System.Net.Http;

var builder = WebApplication.CreateBuilder(args);

var randomImageUrl = Environment.GetEnvironmentVariable("RANDOM_IMAGE_URL") ?? "https://loremflickr.com/1200/800";
var randomImageFilePath = Environment.GetEnvironmentVariable("RANDOM_IMAGE_FILE_PATH") ?? "/usr/src/app/counter/image.jpg";
var staleFlagFilePath = Environment.GetEnvironmentVariable("STALE_FLAG_FILE_PATH") ?? "/usr/src/app/counter/image-stale-served";

var app = builder.Build();

app.UseDefaultFiles(); // maps "/" -> wwwroot/index.html
app.UseStaticFiles();  



Directory.CreateDirectory(Path.GetDirectoryName(randomImageFilePath)!);

// Semaphore to avoid concurrent downloads
var fetchLock = new SemaphoreSlim(1, 1);
var http = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };




// Serve the (cached) image and handle refresh policy
app.MapGet("/image", async (HttpContext ctx) =>
{
    // Ensure directory exists
    Directory.CreateDirectory(Path.GetDirectoryName(randomImageFilePath)!);

    // If no image exists yet, fetch one immediately
    if (!File.Exists(randomImageFilePath))
    {
        await FetchAndReplaceImage();
        return Results.File(randomImageFilePath, "image/jpeg");
    }

    // Determine file age
    var lastWrite = File.GetLastWriteTimeUtc(randomImageFilePath);
    var age = DateTime.UtcNow - lastWrite;
    var tenMinutes = TimeSpan.FromMinutes(10);

    // If age < 10min => serve cached
    if (age < tenMinutes)
    {
        return Results.File(randomImageFilePath, "image/jpeg");
    }

    // If age >= 10min and stale-flag not set -> serve old image and create the stale flag
    if (age >= tenMinutes && !File.Exists(staleFlagFilePath))
    {
        // create flag (best-effort)
        try { File.WriteAllText(staleFlagFilePath, DateTime.UtcNow.ToString("o")); } catch { }
        return Results.File(randomImageFilePath, "image/jpeg");
    }

    // Else: stale flag exists -> fetch fresh image, remove flag, serve new image
    await FetchAndReplaceImage();
    try { if (File.Exists(staleFlagFilePath)) File.Delete(staleFlagFilePath); } catch { }
    return Results.File(randomImageFilePath, "image/jpeg");
});

// Helper: fetch a random Picsum image and replace existing image atomically
async Task FetchAndReplaceImage()
{
    // Single fetch at a time
    await fetchLock.WaitAsync();
    try
    {
       
        if (File.Exists(randomImageFilePath))
        {
            var lastWrite = File.GetLastWriteTimeUtc(randomImageFilePath);
            if ((DateTime.UtcNow - lastWrite) < TimeSpan.FromSeconds(5)) return;
        }

       
        var requestUri = randomImageUrl;
        using var resp = await http.GetAsync(requestUri);
        resp.EnsureSuccessStatusCode();

        // Write to a temp file then move
        var tmp = randomImageFilePath + ".tmp";
        using (var fs = new FileStream(tmp, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await resp.Content.CopyToAsync(fs);
        }

        // Replace atomically
        File.Copy(tmp, randomImageFilePath, true);
        File.Delete(tmp);
        // optionally write metadata (timestamp)
        File.WriteAllText(Path.Combine(Path.GetDirectoryName(randomImageFilePath)!, "image.meta"), DateTime.UtcNow.ToString("o"));
    }
    catch
    {
        // swallow — on failure leave old image in place
    }
    finally { fetchLock.Release(); }
}

app.Run();