using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Msagl.Core.Layout;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources
{
    class PrivateEndpoint : AzureResource, IAssociateWithNic
    {
        public override bool IsSpecific => true;
        public override bool FetchFull => true;
        public override string Image => "img/lib/azure2/networking/Private_Link.svg";

        public string[] Nics { get; private set; }

        public override void Link(IEnumerable<AzureResource> allResources, GeometryGraph graph)
        {
            allResources
                .OfType<App>().Where(x => x.AccessedViaPrivateEndpoint(this))
                .ForEach(x => base.Link(x, graph));

            allResources
                .OfType<KeyVault>().Where(x => x.AccessedViaPrivateEndpoint(this))
                .ForEach(x => base.Link(x, graph));
        }

        public override Task Enrich(JObject jObject, Dictionary<string, JObject> additionalResources)
        {
            Nics = jObject["properties"]!["networkInterfaces"]!.Select(x => x.Value<string>("id")!).ToArray();
            
            return Task.CompletedTask;
        }
    }

    public static class EnumerableEx
    {
        public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
        {
            foreach (var item in items)
            {
                action(item);
            }
        } 
    }
}