var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var port = Environment.GetEnvironmentVariable("PORT");

app.UseDefaultFiles();   // makes "/" serve index.html
app.UseStaticFiles();    // serves files from wwwroot, with correct Content-Type automatically

app.Run($"http://0.0.0.0:{port}");