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
}