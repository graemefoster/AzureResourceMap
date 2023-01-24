using AzureDiagrams.Resources;
using Shouldly;

namespace AzureDiagramsTests.WebApps;

public class WebAppWithSlots
{
    [Fact]
    public async Task CanDrawDiagram()
    {
        var appServicePlanId = "/subscriptions/123/resourceGroups/rg1/providers/Microsoft.Web/serverFarms/TestAppServicePlan";
        var asp = new AppServicePlan(appServicePlanId, "TestAppServicePlan");

        var app1 = new AppServiceApp("/subscriptions/123/resourceGroups/rg1/providers/Microsoft.Web/sites/TestApp1", appServicePlanId,
            "TestApp1", false, Array.Empty<string>(), Array.Empty<string>());

        var app1Slot = new AppServiceApp("/subscriptions/123/resourceGroups/rg1/providers/Microsoft.Web/sites/TestApp1/slots/green", appServicePlanId,
            "TestApp1-green", true, Array.Empty<string>(), Array.Empty<string>());

        var app2 = new AppServiceApp("/subscriptions/123/resourceGroups/rg1/providers/Microsoft.Web/sites/TestApp2", appServicePlanId,
            "TestApp2", false, Array.Empty<string>(), Array.Empty<string>());

        var app2Slot = new AppServiceApp("/subscriptions/123/resourceGroups/rg1/providers/Microsoft.Web/sites/TestApp2/slots/green", appServicePlanId,
            "TestApp2-green", true, Array.Empty<string>(), Array.Empty<string>());
        
        var azureResources = new AzureResource[]
        {
            asp,
            app1,
            app1Slot,
            app2,
            app2Slot
        };
        
        var diagram = await AzureDiagramGenerator.DrawIoDiagramGenerator.DrawDiagram(
            azureResources.Process(),
            false, 
            false, 
            false, 
            true,
            false);
        
        //TODO - shouldn't have to do this in the tests....
        
        diagram.ShouldMatchApproved();
    }
}