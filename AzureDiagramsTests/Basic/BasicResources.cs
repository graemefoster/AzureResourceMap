using AzureDiagrams.Resources;
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
    public async Task SingleStorageAccountSimpleConstructor()
    {
        var diagram = await AzureDiagramGenerator.DrawIoDiagramGenerator.DrawDiagram(
            new []
            {
                new StorageAccount("6e89f6aa-1b83-42f1-ad92-786b47d9fdf7", "test-storage", Array.Empty<string>())
            },
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