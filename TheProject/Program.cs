var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var port = 3001;

app.UseDefaultFiles();   // makes "/" serve index.html
app.UseStaticFiles();    // serves files from wwwroot, with correct Content-Type automatically

app.Run($"http://localhost:{port}");