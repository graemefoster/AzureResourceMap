
namespace DrawIo.Azure.Core.Resources;

public class ResourceLink
{
    public ResourceLink(AzureResource @from, AzureResource to, string? details, FlowEmphasis flowEmphasis)
    {
        From = from;
        To = to;
        Details = details;
        FlowEmphasis = flowEmphasis;
    }

    public AzureResource From { get; }
    public AzureResource To { get; }
    public string? Details { get; }
    
    public FlowEmphasis FlowEmphasis { get; }
}