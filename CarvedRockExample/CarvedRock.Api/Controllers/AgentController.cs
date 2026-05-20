using Microsoft.Agents.AI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using System.Runtime.CompilerServices;
using System.Text;

namespace CarvedRock.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class AgentController(IChatClient chatClient,
                             IConfiguration config,
                             IHttpContextAccessor httpCtxAccessor) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async IAsyncEnumerable<string> Get(string message, [EnumeratorCancellation] CancellationToken cxl)
    {
        var mcpClient = await McpClientHelper.GetMcpClient(config, httpCtxAccessor, cxl);

        //var clientTransport = new HttpClientTransportOptions
        //{
        //    Endpoint = new Uri("http://localhost:5253"),
        //    TransportMode = HttpTransportMode.StreamableHttp
        //};

        //var mcpClient = await McpClient.CreateAsync(new HttpClientTransport(clientTransport),
        //    cancellationToken: cxl);

        var tools = await mcpClient.ListToolsAsync(cancellationToken: cxl);

        var prompt = await GetPromptAsync(message, mcpClient, cxl);

        var agent = new ChatClientAgent(chatClient,
            instructions: prompt,
            name: "CarvedRock Assistant",
            tools: [.. tools]);

        var session = await agent.CreateSessionAsync(cxl);

        //var message = "I've got a hike coming up on a mostly-paved path. Can you give me some product recommendations?";
        await foreach (var update in agent.RunStreamingAsync(message, session, cancellationToken: cxl))
        {
            yield return update.ToString();
        };
    }

    private async Task<string> GetPromptAsync(string message, McpClient mcpClient,
                                          CancellationToken cxl)
    {
        if (message.StartsWith("/admin", StringComparison.InvariantCultureIgnoreCase) &&
            User.IsInRole("admin"))
        {
            var prompt = await mcpClient.GetPromptAsync("admin_prompt", cancellationToken: cxl);
            var adminPrompt = new StringBuilder();
            foreach (var msg in prompt.Messages)
            {
                adminPrompt.AppendLine((msg.Content as TextContentBlock)!.Text);
            }
            return adminPrompt.ToString();
        }

        return
            """
            You are an assistant that can make recommendations about CarvedRock products.  
            Limit product recommendations to 3 for any request. 
            If you can't help with a request, please say so politely.
            """;
    }
}
