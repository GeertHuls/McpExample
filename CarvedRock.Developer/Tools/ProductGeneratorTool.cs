using System.ComponentModel;
using Bogus;
using Bogus.Extensions;
using ModelContextProtocol.Server;

internal class ProductGeneratorTool
{
    [McpServerTool]
    [Description("Generates a set of CarvedRock products that can be used as test data.")]
    public List<Product> GetTestProducts(
        [Description("Number of products to generate")] int numberToGenerate = 10)
            => ProductFaker.Generate(numberToGenerate);

    internal static readonly Faker<Product> ProductFaker = new Faker<Product>()
        .RuleFor(p => p.Id, f => f.IndexFaker + 1)
        .RuleFor(p => p.Name, f => f.Commerce.ProductName().ClampLength(max: 50))
        .RuleFor(p => p.Description, f => f.Commerce.ProductDescription().ClampLength(max: 150))
        .RuleFor(p => p.Category, f => f.PickRandom(new[] { "boots", "equip", "kayak", "clothing" }))
        .RuleFor(p => p.Price, (f, p) =>
            p.Category switch
            {
                "boots" => f.Random.Decimal(50, 300),
                "equip" => f.Random.Decimal(220, 150),
                "clothing" => f.Random.Decimal(20, 150),
                "kayak" => f.Random.Decimal(100, 500),
                _ => 0
            })
        .RuleFor(p => p.ImgUrl, f => f.Image.PicsumUrl());
}

internal class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal Price { get; set; }
    public string Category { get; set; } = null!;
    public string ImgUrl { get; set; } = null!;
}