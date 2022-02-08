namespace AzureDiagrams.Resources.Retrievers;

public class BasicAzureResourceInfo
{
    public string Id { get; init; } = default!;
    public string Type { get; init; } = default!;
    public string Name { get; init; } = default!;
    public string Location { get; set; } = default!;
}