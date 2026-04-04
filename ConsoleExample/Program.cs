using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddConsole(consoleLogOptions =>
{
    // Configure all logs to go to stderr
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();
await builder.Build().RunAsync();

[McpServerToolType]
public class EchoTool(ILogger<EchoTool> logger)
{
    [McpServerTool, Description("Echoes the message back to the client.")]
    public static string Echo(string message) => $"hello {message}";

    [McpServerTool(Name = "fibonacci"), Description("Get the first N Fibonacci numbers.")]
    public Dictionary<int, int> GetFibonacciNumbers(int count)
    {
        logger.LogInformation("Calculating Fibonacci numbers up to count {Count}", count);

        var dict = new Dictionary<int, int>();
        if (count <= 0) return dict;

        int a = 0, b = 1;
        dict[1] = a;
        if (count == 1) return dict;

        dict[2] = b;
        for (int i = 3; i<= count; i++)
        {
            int next = a + b;
            dict[i] = next;
            a = b;
            b = next;
        }

        return dict;
    }
}

