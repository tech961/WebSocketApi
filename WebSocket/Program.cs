using Microsoft.OpenApi.Models;
using WebSocket.Hubs;
using WebSocket.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<StaticVideoOptions>(builder.Configuration.GetSection(StaticVideoOptions.SectionName));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "WebSocket API",
        Version = "v1",
        Description = "APIs for broadcasting videos to SignalR clients."
    });
});

builder.Services.AddSignalR();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "WebSocket API v1");
    options.RoutePrefix = string.Empty;
});

app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();
app.MapHub<VideoHub>("/hubs/video");

app.Run();
