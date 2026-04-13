var builder = DistributedApplication.CreateBuilder(args);

var server = builder.AddProject<Projects.WebAppExample_Server>("server")
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints();

var webfrontend = builder.AddViteApp("webfrontend", "../frontend")
    .WithReference(server)
    .WaitFor(server);

server.PublishWithContainerFiles(webfrontend, "wwwroot");

var mcp = builder.AddProject<Projects.WebAppExample_McpServer>("mcp-server")
    .WithHttpHealthCheck("/health")
    .WithReference(server);

builder.AddMcpInspector("mcp-inspector", opt => opt.InspectorVersion = "0.17.5")
    .WithMcpServer(mcp, path: "");

builder.Build().Run();
