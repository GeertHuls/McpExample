var builder = DistributedApplication.CreateBuilder(args);

var carvedrockdb = builder.AddPostgres("postgres")
                          .AddDatabase("CarvedRockPostgres");

// https://aspire.dev/integrations/ai/ollama/ollama-get-started
// https://aspire.dev/integrations/ai/ollama/ollama-host
var ollama = builder.AddOllama("ollama"/*, port: 11434*/)
    .WithDataVolume()
    .WithGPUSupport()
    //.WithOpenWebUI()
    //.WithImageTag("0.24.0") // does not work
    //.WithImageTag("0.15.0") // does not work
    ;

// Example using Hugging face model:
//var llama = ollama.AddHuggingFaceModel(
//    "llama",
//    "bartowski/Llama-3.2-1B-Instruct-GGUF:IQ4_XS");

//var ministral3 = ollama.AddModel("ollama-ministral3", "ministral-3");

var llama31 = ollama.AddModel("ollama-llama31", "llama3.1");

var idsrv = builder.AddProject<Projects.Duende_IdentityServer_Demo>("idsrv");

var api = builder.AddProject<Projects.CarvedRock_Api>("api")
    .WithReference(carvedrockdb)
    //.WithReference(ministral3)
    .WithReference(llama31)
    .WaitFor(carvedrockdb)
    .WithEnvironment("Auth__Authority", idsrv.GetEndpoint("https"))
    .WithHttpHealthCheck("/health");

var mailpit = builder.AddMailPit("smtp");

builder.AddProject<Projects.CarvedRock_WebApp>("webapp")
    .WithReference(api)
    .WithReference(mailpit)
    .WaitFor(mailpit)
    .WaitFor(api)
    .WithHttpHealthCheck("/health");

var mcp = builder.AddProject<Projects.CarvedRock_Mcp>("mcp")
    .WithReference(api)
    .WithEnvironment("AuthServer", idsrv.GetEndpoint("https")) // use client id interactive.public, without password
    .WithHttpHealthCheck("/health");

builder.AddMcpInspector("mcp-inspector", opt => opt.InspectorVersion = "0.17.5")
    .WithMcpServer(mcp, path: "");

builder.Build().Run();
