using Shouldly;

namespace AzureDiagramsTests.Basic;

public class BasicResources
{
    [Fact]
    public async Task SingleStorageAccount()
    {
        var resources = TestResourcesObjectMother
            .StorageAccount();

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

    [Fact]
    public async Task VNetWithAttachedStoragetAccountInSubNet()
    {
        var resources = (await TestResourcesObjectMother.StorageAccountWithPrivateEndpoint())
            .ToArray();

        var diagram = await AzureDiagramGenerator.DrawIoDiagramGenerator.DrawDiagram(
            resources.ToArray(),
            false, 
            false, 
            false, 
            true,
            false);
        
        diagram.ShouldMatchApproved();
    }

}