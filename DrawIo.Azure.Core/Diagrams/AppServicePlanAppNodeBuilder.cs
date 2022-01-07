using DrawIo.Azure.Core.Resources;

namespace DrawIo.Azure.Core.Diagrams;

public class AppServicePlanAppNodeBuilder : AzureResourceNodeBuilder
{
    public AppServicePlanAppNodeBuilder(ASP resource) : base(resource)
    {
    }
}