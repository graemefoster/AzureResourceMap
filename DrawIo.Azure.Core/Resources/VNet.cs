using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DrawIo.Azure.Core.Resources.Diagrams;
using Microsoft.Msagl.Core.Layout;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources
{
    class VNet : AzureResource
    {
        class AzureResourceVNetNodeBuilder : IAzureNodeBuilder
        {
            
            public IEnumerable<Node> CreateNodes(AzureResource resource)
            {
                var vnetResource = (VNet)resource;

                var vnetNode = AzureResourceRectangleDrawer.CreateContainerRectangleNode(resource.Name);
                yield return vnetNode;

                foreach (var subnet in vnetResource.Subnets)
                {
                    var subnetNode = AzureResourceRectangleDrawer.CreateContainerRectangleNode(subnet.Name);
                    vnetNode.AddChild(subnetNode);

                    var emptyContents = AzureResourceRectangleDrawer.CreateSimpleRectangleNode(Guid.NewGuid().ToString());
                    subnetNode.AddChild(emptyContents);

                    yield return subnetNode;
                    yield return emptyContents;
                }
            }

        }

        public override IAzureNodeBuilder CreateNodeBuilder()
        {
            return new AzureResourceVNetNodeBuilder();
        }

        public override bool IsSpecific => true;
        public override bool FetchFull => true;
        public override string ApiVersion => "2021-02-01";
        private Subnet[] Subnets { get; set; }

        class Subnet
        {
            public string Name { get; set; }
            public string AddressPrefix { get; set; }
            internal List<AzureResource> ContainedResources { get; } = new();
        }

        public override string Image => "img/lib/azure2/networking/Virtual_Networks.svg";

        public override Task Enrich(JObject full, Dictionary<string, JObject> additionalResources)
        {
            Subnets = full["properties"]!["subnets"]!.Select(x => new Subnet
            {
                Name = x.Value<string>("name")!,
                AddressPrefix = x["properties"]!.Value<string>("addressPrefix")!
            }).ToArray();

            return Task.CompletedTask;
        }

        //
        // public void AddToVNet(AzureResource resource, string subnetName)
        // {
        //     var subnet = Subnets.Single(x => x.Name == subnetName);.ContainedResources.Add();
        //     resource.SetParent(_subnets.Single(x => ))
        // }

        public void AddToVNet(AzureResource resource, string subnet)
        {
            Subnets.Single(x => x.Name == subnet).ContainedResources.Add(resource);
        }
    }
}