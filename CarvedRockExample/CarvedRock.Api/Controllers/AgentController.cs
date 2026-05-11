using Microsoft.Agents.AI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OllamaSharp;
using System.Runtime.CompilerServices;

namespace CarvedRock.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class AgentController : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async IAsyncEnumerable<string> Get([EnumeratorCancellation] CancellationToken cxl)
    {
        var chatClient = new OllamaApiClient(new Uri("http://localhost:11434"), "ministral-3");
        var agent = new ChatClientAgent(chatClient, instructions: "You are good at telling jokes.", name: "Joker");

        var session = await agent.CreateSessionAsync(cxl);

        await foreach (var update in agent.RunStreamingAsync("Tell me a joke about Alice in Wonderland.",
            session, cancellationToken: cxl))
        {
            yield return update.ToString();
        };
    }
}
