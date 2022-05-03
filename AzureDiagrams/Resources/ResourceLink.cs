
namespace AzureDiagrams.Resources;

public class ResourceLink
{
    public ResourceLink(AzureResource @from, AzureResource to, string? details, Plane plane)
    {
        From = from;
        To = to;
        Details = details;
        Plane = plane;
    }

    public AzureResource From { get; }
    public AzureResource To { get; }
    public string? Details { get; }
    
    public Plane Plane { get; }
}