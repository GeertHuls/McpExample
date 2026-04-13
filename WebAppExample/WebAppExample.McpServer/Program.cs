using System.Net.Http.Headers;
using WebAppExample.McpServer.Tools;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddMcpServer()
    .WithHttpTransport(o => o.Stateless = false)
    .WithTools<WeatherTools>();

// Configure HttpClientFactory for weather.gov API
builder.Services.AddHttpClient("WeatherApi", client =>
{
    client.BaseAddress = new Uri("https://api.weather.gov");
    client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("weather-tool", "1.0"));
});

builder.Services.AddHttpClient("SimpleWeather", client =>
    client.BaseAddress = new("https://server"));

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapMcp();

app.Run();
