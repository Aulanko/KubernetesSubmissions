var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var counter = 0;

app.MapGet("/pingpong", (HttpContext ctx) =>
{
    ctx.Response.Headers.CacheControl = "no-store";

    string answer = "pong "+counter;
    counter++;
    return answer;
})
.WithName("pingpong");

app.Run();

