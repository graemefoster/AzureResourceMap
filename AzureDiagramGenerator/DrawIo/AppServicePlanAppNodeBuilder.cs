using DrawIo.Azure.Core.Resources;

namespace AzureDiagramGenerator.DrawIo;

public class AppServicePlanAppNodeBuilder : AzureResourceNodeBuilder
{
    public AppServicePlanAppNodeBuilder(ASP resource) : base(resource)
    {
    }
}