using System.Net.Http;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();


const string ImageFile = "/usr/src/app/counter/image.jpg";
const string StaleFlagFile = "/usr/src/app/counter/image-stale-served";


Directory.CreateDirectory(Path.GetDirectoryName(ImageFile)!);

// Semaphore to avoid concurrent downloads
var fetchLock = new SemaphoreSlim(1, 1);
var http = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };

app.MapGet("/", () =>
{
    var html = """
    <!DOCTYPE html>
    <html lang="en">
    <head>
        <meta charset="UTF-8">
        <title>Todo App</title>
        <style>
            body {
                display: flex;
                flex-direction: column;
                justify-content: center;
                align-items: center;
                min-height: 100vh;
                margin: 0;
                font-family: Arial, sans-serif;
                background: #f5f5f5;
            }
            h1 {
                margin-bottom: 20px;
            }
            img {
                max-width: 90%;
                height: auto;
                border-radius: 8px;
                box-shadow: 0 4px 12px rgba(0,0,0,0.15);
            }
        </style>
    </head>
    <body>
        <h1>Todo App</h1>
        <img src="/image" alt="Random image">
    </body>
    </html>
    """;

    return Results.Content(html, "text/html");
});



// Serve the (cached) image and handle refresh policy
app.MapGet("/image", async (HttpContext ctx) =>
{
    // Ensure directory exists
    Directory.CreateDirectory(Path.GetDirectoryName(ImageFile)!);

    // If no image exists yet, fetch one immediately
    if (!File.Exists(ImageFile))
    {
        await FetchAndReplaceImage();
        return Results.File(ImageFile, "image/jpeg");
    }

    // Determine file age
    var lastWrite = File.GetLastWriteTimeUtc(ImageFile);
    var age = DateTime.UtcNow - lastWrite;
    var tenMinutes = TimeSpan.FromMinutes(10);

    // If age < 10min => serve cached
    if (age < tenMinutes)
    {
        return Results.File(ImageFile, "image/jpeg");
    }

    // If age >= 10min and stale-flag not set -> serve old image and create the stale flag
    if (age >= tenMinutes && !File.Exists(StaleFlagFile))
    {
        // create flag (best-effort)
        try { File.WriteAllText(StaleFlagFile, DateTime.UtcNow.ToString("o")); } catch { }
        return Results.File(ImageFile, "image/jpeg");
    }

    // Else: stale flag exists -> fetch fresh image, remove flag, serve new image
    await FetchAndReplaceImage();
    try { if (File.Exists(StaleFlagFile)) File.Delete(StaleFlagFile); } catch { }
    return Results.File(ImageFile, "image/jpeg");
});

// Helper: fetch a random Picsum image and replace existing image atomically
async Task FetchAndReplaceImage()
{
    // Single fetch at a time
    await fetchLock.WaitAsync();
    try
    {
       
        if (File.Exists(ImageFile))
        {
            var lastWrite = File.GetLastWriteTimeUtc(ImageFile);
            if ((DateTime.UtcNow - lastWrite) < TimeSpan.FromSeconds(5)) return;
        }

       
        var requestUri = "https://loremflickr.com/1200/800";
        using var resp = await http.GetAsync(requestUri);
        resp.EnsureSuccessStatusCode();

        // Write to a temp file then move
        var tmp = ImageFile + ".tmp";
        using (var fs = new FileStream(tmp, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await resp.Content.CopyToAsync(fs);
        }

        // Replace atomically
        File.Copy(tmp, ImageFile, true);
        File.Delete(tmp);
        // optionally write metadata (timestamp)
        File.WriteAllText(Path.Combine(Path.GetDirectoryName(ImageFile)!, "image.meta"), DateTime.UtcNow.ToString("o"));
    }
    catch
    {
        // swallow — on failure leave old image in place
    }
    finally { fetchLock.Release(); }
}

app.Run();