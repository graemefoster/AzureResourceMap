using Shouldly;

namespace AzureDiagramsTests.Basic;

public class BasicResources
{
    [Fact]
    public async Task SingleStorageAccount()
    {
        var resources = TestResourcesObjectMother
            .WithPublicAccessibleStorageAccount();

        var diagram = await AzureDiagramGenerator.DrawIoDiagramGenerator.DrawDiagram(
            resources.ToArray(),
            false, 
            false, 
            false, 
            false,
            false);
        
        diagram.ShouldMatchApproved();
    }
    [Fact]
    public async Task VNetWithSubNet()
    {
        var resources = await TestResourcesObjectMother
            .VirtualNetwork("subnet1");

        var diagram = await AzureDiagramGenerator.DrawIoDiagramGenerator.DrawDiagram(
            resources.ToArray(),
            false, 
            false, 
            false, 
            false,
            false);
        
        diagram.ShouldMatchApproved();
    }

}