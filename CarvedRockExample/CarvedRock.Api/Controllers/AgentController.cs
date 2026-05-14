using Microsoft.Agents.AI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
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
        var agent = new ChatClientAgent(chatClient, instructions: "You are good at telling jokes.", name: "Joker");

        var session = await agent.CreateSessionAsync(cxl);

        await foreach (var update in agent.RunStreamingAsync("Tell me a joke about Alice in Wonderland.",
            session, cancellationToken: cxl))
        {
            yield return update.ToString();
        };
    }
}
