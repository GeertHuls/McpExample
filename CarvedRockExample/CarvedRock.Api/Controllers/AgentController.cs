using Microsoft.Agents.AI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using System.Runtime.CompilerServices;

namespace CarvedRock.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class AgentController(IChatClient chatClient) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async IAsyncEnumerable<string> Get([EnumeratorCancellation] CancellationToken cxl)
    {
        var clientTransport = new HttpClientTransportOptions
        {
            Endpoint = new Uri("http://localhost:5253"),
            TransportMode = HttpTransportMode.StreamableHttp
        };

        var mcpClient = await McpClient.CreateAsync(new HttpClientTransport(clientTransport),
            cancellationToken: cxl);

        var tools = await mcpClient.ListToolsAsync(cancellationToken: cxl);

        var agent = new ChatClientAgent(chatClient,
            instructions:
            """
            You are an assistant that can make recommendations about CarvedRock products.
            Limit product recommendations to 3 for any request.
            If you can't help with a request, please say so politely.
            """,
            name: "CarvedRock Assistant",
            tools: [.. tools]);

        var session = await agent.CreateSessionAsync(cxl);

        var message = "I've got a hike coming up on a mostly-paved path. Can you give me some product recommendations?";
        await foreach (var update in agent.RunStreamingAsync(message, session, cancellationToken: cxl))
        {
            yield return update.ToString();
        };
    }
}
