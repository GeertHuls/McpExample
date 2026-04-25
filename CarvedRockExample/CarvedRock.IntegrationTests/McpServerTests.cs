using CarvedRock.IntegrationTests.Utils;

namespace CarvedRock.IntegrationTests.Tests;

public class McpServerTests(AppFixture appFixture) : IClassFixture<AppFixture>
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

    [Fact]
    public async Task GetToolsIncludesGetProducts()
    {
        // Act
        var tools = await appFixture.McpClient.ListToolsAsync(cancellationToken: appFixture.CancelToken);

        // Assert
        var getProductsTool = tools.FirstOrDefault(t => t.Name == "get_products");
        Assert.NotNull(getProductsTool);
    }
}
