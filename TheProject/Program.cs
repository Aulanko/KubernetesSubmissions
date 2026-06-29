var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseDefaultFiles();   // makes "/" serve index.html
app.UseStaticFiles();    // serves files from wwwroot, with correct Content-Type automatically

app.Run("http://localhost:3001");