namespace WebApplication1;

public class WasteCollectionResponse
{
    public List<CollectionItem>? CollectionItems { get; set; }
}

public class CollectionItem
{
    public DateTime Date { get; set; }
    public string? Type { get; set; }
}

public class WasteCollectionResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public List<WasteCollectionItem>? Items { get; set; }
    public string? Uprn { get; set; }
}

public class WasteCollectionItem
{
    public DateTime Date { get; set; }
    public string? Type { get; set; }
    public string MappedType { get; set; } = "Waste Collection";
}