using System.Text.Json;
using WebApplication1;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient("WasteApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("WasteApi:BaseUrl") 
        ?? "https://api.westnorthants.digital/openapi/v1/");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddSingleton<WasteCollectionService>();

var app = builder.Build();

app.MapGet("/test", async (WasteCollectionService wasteService) =>
{
    var result = await wasteService.GetCollectionsAsync();
    
    if (!result.Success)
        return Results.Problem(result.ErrorMessage);
    
    if (result.Items == null || !result.Items.Any())
        return Results.NotFound("No collection items found.");
    
    var output = "Collection Items:\n";
    foreach (var item in result.Items)
    {
        output += $"  {item.Date:yyyy-MM-dd} - {item.MappedType}\n";
    }
    return Results.Text(output);
});

app.MapGet("/calendar.ics", async (WasteCollectionService wasteService, HttpContext context) =>
{
    var result = await wasteService.GetCollectionsAsync();
    
    if (!result.Success)
    {
        context.Response.StatusCode = 500;
        return Results.Text($"Error fetching data: {result.ErrorMessage}");
    }

    if (result.Items == null || !result.Items.Any())
    {
        context.Response.StatusCode = 404;
        return Results.Text("No collection items found.");
    }
    
    var icsContent = GenerateIcsCalendar(result.Items);
    
    context.Response.ContentType = "text/calendar; charset=utf-8";
    context.Response.Headers.Append("Content-Disposition", "inline; filename=waste-collection.ics");
    context.Response.Headers.Append("Cache-Control", "public, max-age=604800");
    context.Response.Headers.Append("X-WR-RELCALID", result.Uprn ?? "waste-collection");
    
    return Results.Text(icsContent, "text/calendar; charset=utf-8");
});

app.MapGet("/health", () => Results.Ok("Healthy"));

app.Run();
//I AI generated this function and it works
static string GenerateIcsCalendar(IEnumerable<WasteCollectionItem> items)
{
    var ics = new System.Text.StringBuilder();
    ics.AppendLine("BEGIN:VCALENDAR");
    ics.AppendLine("VERSION:2.0");
    ics.AppendLine("PRODID:-//Waste Collection Calendar//EN");
    ics.AppendLine("CALSCALE:GREGORIAN");
    ics.AppendLine("METHOD:PUBLISH");
    ics.AppendLine("X-WR-CALNAME:Waste Collections");
    ics.AppendLine("X-WR-TIMEZONE:Europe/London");
    ics.AppendLine("X-WR-CALDESC:Waste collection schedule - Auto-refreshes weekly");
    ics.AppendLine("X-PUBLISHED-TTL:PT1W");
    ics.AppendLine("REFRESH-INTERVAL;VALUE=DURATION:P1W");

    foreach (var item in items)
    {
        var dateStr = item.Date.ToString("yyyyMMdd");
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddTHHmmssZ");
        var uid = $"{item.Date:yyyyMMdd}-{item.Type?.ToLowerInvariant() ?? "waste"}@wastecollection.local";

        ics.AppendLine("BEGIN:VEVENT");
        ics.AppendLine($"UID:{uid}");
        ics.AppendLine($"DTSTAMP:{timestamp}");
        ics.AppendLine($"DTSTART;VALUE=DATE:{dateStr}");
        ics.AppendLine($"DTEND;VALUE=DATE:{dateStr}");
        ics.AppendLine($"SUMMARY:{item.MappedType}");
        ics.AppendLine($"DESCRIPTION:Remember to put out your {item.MappedType.ToLower()}");
        ics.AppendLine("STATUS:CONFIRMED");
        ics.AppendLine("SEQUENCE:0");
        ics.AppendLine("BEGIN:VALARM");
        ics.AppendLine("TRIGGER:-PT18H");
        ics.AppendLine("ACTION:DISPLAY");
        ics.AppendLine($"DESCRIPTION:Reminder: {item.MappedType} tomorrow");
        ics.AppendLine("END:VALARM");
        ics.AppendLine("END:VEVENT");
    }

    ics.AppendLine("END:VCALENDAR");
    return ics.ToString();
}

public class WasteCollectionService(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<WasteCollectionService> logger)
{
    private static readonly Dictionary<string, string> TypeMappings = new()
    {
        { "refuse", "General Waste Collection" },
        { "recycling", "Recycling Collection" },
        { "garden", "Garden Waste Collection" },
        { "food", "Food Waste Collection" }
    };
    
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<WasteCollectionResult> GetCollectionsAsync()
    {
        var uprn = configuration.GetValue<string>("UPRN") ?? Environment.GetEnvironmentVariable("UPRN");
        
        if (string.IsNullOrEmpty(uprn))
        {
            logger.LogError("UPRN not configured");
            return new WasteCollectionResult { Success = false, ErrorMessage = "UPRN not configured" };
        }

        try
        {
            var client = httpClientFactory.CreateClient("WasteApi");
            var response = await client.GetAsync($"unified-waste-collections/{uprn}");

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("API returned {StatusCode}: {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
                return new WasteCollectionResult 
                { 
                    Success = false, 
                    ErrorMessage = $"{response.StatusCode} - {response.ReasonPhrase}" 
                };
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            var root = JsonSerializer.Deserialize<WasteCollectionResponse>(jsonString, JsonOptions);

            if (root?.CollectionItems == null || !root.CollectionItems.Any())
            {
                return new WasteCollectionResult { Success = true, Items = [], Uprn = uprn };
            }

            var items = root.CollectionItems.Select(item => new WasteCollectionItem
            {
                Date = item.Date,
                Type = item.Type,
                MappedType = TypeMappings.GetValueOrDefault(item.Type?.ToLowerInvariant() ?? "", "Waste Collection")
            }).ToList();

            return new WasteCollectionResult { Success = true, Items = items, Uprn = uprn };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching waste collection data");
            return new WasteCollectionResult { Success = false, ErrorMessage = ex.Message };
        }
    }
}





