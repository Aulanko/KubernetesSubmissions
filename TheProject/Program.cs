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
                justify-content: flex-start;
                align-items: center;
                min-height: 100vh;
                margin: 0;
                padding: 24px;
                font-family: Arial, sans-serif;
                background: #f5f5f5;
            }
            .container {
                width: 100%;
                max-width: 900px;
                background: #fff;
                padding: 18px;
                border-radius: 10px;
                box-shadow: 0 6px 18px rgba(0,0,0,0.08);
            }
            h1 {
                margin: 0 0 12px 0;
            }
            .image-wrap {
                text-align: center;
                margin-bottom: 16px;
            }
            img {
                max-width: 100%;
                height: auto;
                border-radius: 8px;
                box-shadow: 0 4px 12px rgba(0,0,0,0.12);
            }
            .todo-input {
                display:flex;
                gap:8px;
                margin: 12px 0;
            }
            input[type="text"] {
                flex:1;
                padding: 10px 12px;
                font-size: 14px;
                border: 1px solid #ddd;
                border-radius: 6px;
            }
            button {
                padding: 10px 14px;
                font-size: 14px;
                border-radius: 6px;
                border: none;
                background: #007bff;
                color: white;
                cursor: pointer;
            }
            button:disabled {
                background: #9fc1ff;
                cursor: default;
            }
            .meta {
                font-size: 13px;
                color: #666;
                margin-top: 8px;
            }
            ul.todo-list {
                margin: 12px 0 0 0;
                padding-left: 18px;
            }
            ul.todo-list li {
                margin: 6px 0;
            }
        </style>
    </head>
    <body>
      <div class="container">
        <h1>Todo App</h1>

        <div class="image-wrap">
          <img src="/image" alt="Random image">
        </div>

        <div class="todo-input">
          <input id="todoInput" type="text" maxlength="140" placeholder="Write a todo (max 140 chars)">
          <button id="sendBtn" disabled>Send</button>
        </div>
        <div class="meta">
          <span id="charCount">140</span> characters remaining
        </div>

        <h2>Todos</h2>
        <ul class="todo-list" id="todoList">
          <li>Buy milk</li>
          <li>Walk the dog</li>
          <li>Write blog post about Kubernetes</li>
        </ul>
      </div>

      <script>
        (function(){
          const input = document.getElementById('todoInput');
          const btn = document.getElementById('sendBtn');
          const count = document.getElementById('charCount');
          const max = 140;

          function update() {
            const remaining = max - input.value.length;
            count.textContent = remaining;
            // Enable send only when there's some text (not required by your task to actually send)
            btn.disabled = input.value.trim().length === 0;
          }

          input.addEventListener('input', update);
          // initialize
          update();

          // Send button currently does nothing functional (per requirement)
          btn.addEventListener('click', function(e){
            e.preventDefault();
            // Visual feedback only:
            btn.textContent = 'Sent!';
            setTimeout(() => btn.textContent = 'Send', 800);
          });
        })();
      </script>
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