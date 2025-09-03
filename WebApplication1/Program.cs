using System.Text.Json;
using WebApplication1;

var builder = WebApplication.CreateBuilder(args);


var app = builder.Build();

app.MapGet("/", async (string? name) =>
{
    var typeMappings = new Dictionary<string, string>
    {
        { "refuse", "General Waste" },
        { "recycling", "Recycling" },
        { "garden", "Garden Waste" },
        { "food", "Food Waste" }
    };
    
    var uprn = Environment.GetEnvironmentVariable("UPRN");

    using var httpClient = new HttpClient();
    var response =
        await httpClient.GetAsync($"https://api.westnorthants.digital/openapi/v1/unified-waste-collections/{uprn}");

    if (response.IsSuccessStatusCode)
    {
        var jsonString = await response.Content.ReadAsStringAsync();
        
        var root = JsonSerializer.Deserialize<Root>(jsonString);
        
        if (root?.collectionItems != null)
        {
            var result = "Collection Items:\n";
            foreach (var item in root.collectionItems)
            {
                var mappedType = typeMappings!.GetValueOrDefault(item.type?.ToLowerInvariant(), "Unknown");
                result += $"  {item.date:yyyy-MM-dd} - {mappedType}\n";
            }
            return result;
        }
        else
        {
            return "No collection items found.";
        }
    }
    else
    {
        return $"Error: {response.StatusCode} - {response.ReasonPhrase}";
    }
});

app.Run();