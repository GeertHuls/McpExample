using CarvedRock.Core;
using CarvedRock.Data;
using CarvedRock.Domain;
using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults(); 

builder.Services.AddProblemDetails(opts => // built-in problem details support
    opts.CustomizeProblemDetails = (ctx) =>
    {
        if (!ctx.ProblemDetails.Extensions.ContainsKey("traceId"))
        {
            string? traceId = Activity.Current?.Id ?? ctx.HttpContext.TraceIdentifier;
            ctx.ProblemDetails.Extensions.Add(new KeyValuePair<string, object?>("traceId", traceId));
        }
        var exception = ctx.HttpContext.Features.Get<IExceptionHandlerFeature>()?.Error;
        if (ctx.ProblemDetails.Status == 500)
        {
            ctx.ProblemDetails.Detail = "An error occurred in our API. Use the trace id when contacting us.";
        }
    }
);

var authority = builder.Configuration.GetValue<string>("Auth:Authority");
JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = authority;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = "email",
            //RoleClaimType = "role",
            ValidateAudience = false
        };
    });
builder.Services.AddTransient<IClaimsTransformation, AdminClaimsTransformation>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi(options =>
{
    var authorityUrl = builder.Configuration.GetValue<string>("Auth:Authority");

    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        // Ensure instances exist
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

        var oauthScopes = new Dictionary<string, string>
            {
                { "api", "Resource access: Carved Rock API" },
                { "openid", "OpenID information"},
                { "profile", "User profile information" },
                { "email", "User email address" }
            };

        // Add OAuth2 security scheme (Authorization Code flow only)
        document.Components.SecuritySchemes.Add("oauth2", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Flows = new OpenApiOAuthFlows
            {
                AuthorizationCode = new OpenApiOAuthFlow
                {
                    AuthorizationUrl = new Uri($"{authorityUrl}/connect/authorize"),
                    TokenUrl = new Uri($"{authorityUrl}/connect/token"),
                    Scopes = oauthScopes
                }
            }
        });

        // Apply security requirement globally
        document.Security = [
            new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecuritySchemeReference("oauth2"),
                    oauthScopes.Keys.ToList()
                }
            }
        ];

        // Set the host document for all elements
        // including the security scheme references
        document.SetReferenceHostDocument();

        return Task.CompletedTask;
    });
});

builder.Services.AddScoped<IProductLogic, ProductLogic>();
builder.AddNpgsqlDbContext<LocalContext>("CarvedRockPostgres");
builder.Services.AddScoped<ICarvedRockRepository, CarvedRockRepository>();

builder.Services.AddValidatorsFromAssemblyContaining<NewProductValidator>();

var app = builder.Build();
app.MapDefaultEndpoints();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    SetupDevelopment(app);
}

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<UserScopeMiddleware>();
app.MapControllers().RequireAuthorization();

app.Run();

static void SetupDevelopment(WebApplication app)
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<LocalContext>();
        context.MigrateAndCreateData();
    }

    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
        options.OAuthClientId("interactive.public");
        options.OAuthAppName("CarvedRock API");
        options.OAuthUsePkce();
    });
}
