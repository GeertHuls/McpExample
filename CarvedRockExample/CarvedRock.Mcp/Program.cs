using CarvedRock.Core;
using CarvedRock.Mcp;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ModelContextProtocol.AspNetCore.Authentication;
using ModelContextProtocol.Authentication;
using System.Security.Claims;

// Example of a protected MCP server with OAuth authentication:
// https://github.com/modelcontextprotocol/csharp-sdk/blob/main/samples/ProtectedMcpServer/Program.cs

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

builder.Services.AddCors(options => // cors is required for mcp inspector with oauth.
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var authServer = builder.Configuration.GetValue<string>("AuthServer")!;
var mcpServerUrl = builder.Configuration.GetValue<string>("McpServerUrl")!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultChallengeScheme = McpAuthenticationDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
})
        .AddJwtBearer(options =>
        {
            options.Authority = authServer;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidAudience = mcpServerUrl,
                ValidIssuer = authServer,
                NameClaimType = ClaimTypes.Email // have the mail address included as part of the log entry as the name of the user.
            };
        })
        .AddMcp(options =>
        {
            options.ResourceMetadata = new ProtectedResourceMetadata
            {
                Resource = mcpServerUrl,
                AuthorizationServers = { authServer },
                ScopesSupported = ["api", "openid", "profile", "email", "offline_access"],
            };
        });

builder.Services.AddAuthorization();

builder.Services.AddTransient<IClaimsTransformation, AdminClaimsTransformation>();

builder.Services.AddMcpServer()
    .WithHttpTransport(options =>
    {
        options.Stateless = true; // important for scaling
        options.ConfigureSessionOptions = async (httpCtx, options, cancelToken) =>
        {
            // debugging logged in user:
            var user = httpCtx.User;
        };
    })
    .AddAuthorizationFilters()
    .WithToolsFromAssembly()
    //.WithTools<CarvedRockTools>()
    //.WithTools<AdminTools>()
    ;

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<TokenForwarder>();
builder.Services.AddHttpClient("CarvedRockApi", client =>
        client.BaseAddress = new("https://api"))
    .AddHttpMessageHandler<TokenForwarder>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseCors();
}

app.MapDefaultEndpoints();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<UserScopeMiddleware>();

app.MapMcp()
    .RequireAuthorization();  // this would require auth for **all** connections (even "initialize")
                              // only add if you don't have any anonymous tools to support

app.Run();
