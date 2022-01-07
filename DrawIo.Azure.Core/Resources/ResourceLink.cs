namespace DrawIo.Azure.Core.Resources;

public class ResourceLink
{
    public ResourceLink(AzureResource @from, AzureResource to, string? details)
    {
        From = from;
        To = to;
        Details = details;
    }

    public AzureResource From { get; }
    public AzureResource To { get; }
    public string? Details { get; }
}