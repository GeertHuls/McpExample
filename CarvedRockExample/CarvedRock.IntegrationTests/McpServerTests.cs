using CarvedRock.IntegrationTests.Utils;
using ModelContextProtocol.Protocol;
using System.Text.Json;
using static Google.Protobuf.Compiler.CodeGeneratorResponse.Types;

namespace CarvedRock.IntegrationTests.Tests;

public class McpServerTests(AppFixture fixture) : IClassFixture<AppFixture>
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

    [Theory]
    [InlineData("alice", "alice")]
    [InlineData("bob", "bob")]
    public async Task GetToolsIncludesGetProducts(string user, string pwd)
    {
        var mcpClient = await fixture.GetMcpClient(user, pwd, TestContext.Current.CancellationToken);

        var tools = await mcpClient.ListToolsAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        var getProductsTool = tools.FirstOrDefault(t => t.Name == "get_products");
        Assert.NotNull(getProductsTool);
    }

    [Fact]
    public async Task CallGetProductsToolReturnsProducts()
    {
        var mcpClient = await fixture.GetMcpClient("alice", "alice", TestContext.Current.CancellationToken);

        //Act
        var getProductsResponse = await mcpClient.CallToolAsync(
            "get_products", cancellationToken: TestContext.Current.CancellationToken);

        //Assert
        Assert.NotNull(getProductsResponse);
        Assert.NotEqual(true, getProductsResponse.IsError);

        var productJson = getProductsResponse.Content.First(c => c.Type == "text") as TextContentBlock;
        var products = JsonSerializer.Deserialize<List<ProductModel>>(
            productJson?.Text ?? "[]",
            fixture.JsonSerializerOptions);

        Assert.NotNull(products);
        Assert.Contains(products!, p => p.Name == "Alpine Trekker");
    }
}

public record ProductModel
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public double Price { get; set; }
    public string Category { get; set; } = null!;
}