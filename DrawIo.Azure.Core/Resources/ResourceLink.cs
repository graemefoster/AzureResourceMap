namespace DrawIo.Azure.Core.Resources;

public class ResourceLink
{
    public ResourceLink(AzureResource from, AzureResource to)
    {
        From = from;
        To = to;
    }

    public AzureResource From { get; }
    public AzureResource To { get; }
}