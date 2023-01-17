using AzureDiagrams.Resources;
using Shouldly;

namespace AzureDiagramsTests.WebApps;

public class WebAppWithSlots
{
    [Fact]
    public async Task CanDrawDiagram()
    {
        var asp = new AppServicePlan("ef49d009-8f26-4a51-9e5e-f486a17eb168", "TestAppServicePlan");

        var app1 = new AppServiceApp("00056f4d-e9cb-49a0-a918-a25071ac8f69", "ef49d009-8f26-4a51-9e5e-f486a17eb168",
            "TestApp", false, Array.Empty<string>(), Array.Empty<string>())
        {
            Type = "microsoft.web/sites"
        };

        var app1Slot = new AppServiceApp("00056f4d-e9cb-49a0-a918-a25071ac8f79", "ef49d009-8f26-4a51-9e5e-f486a17eb168",
            "TestApp-green", true, Array.Empty<string>(), Array.Empty<string>());

        var app2 = new AppServiceApp("00056f4d-e9cb-49a0-a918-a25071ac8f89", "ef49d009-8f26-4a51-9e5e-f486a17eb168",
            "TestApp", false, Array.Empty<string>(), Array.Empty<string>());

        var app2Slot = new AppServiceApp("00056f4d-e9cb-49a0-a918-a25071ac8f99", "ef49d009-8f26-4a51-9e5e-f486a17eb168",
            "TestApp", true, Array.Empty<string>(), Array.Empty<string>());
        
        var azureResources = new AzureResource[]
        {
            asp,
            app1,
            app1Slot,
            app2,
            app2Slot
        };
        azureResources.BuildRelationships();
        
        var diagram = await AzureDiagramGenerator.DrawIoDiagramGenerator.DrawDiagram(
            azureResources,
            false, 
            false, 
            false, 
            true,
            false);
        
        //TODO - shouldn't have to do this in the tests....
        
        diagram.ShouldMatchApproved();
    }
}