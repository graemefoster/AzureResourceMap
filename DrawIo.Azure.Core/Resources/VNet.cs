using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources
{
    class VNet : AzureResource
    {
        public override bool IsSpecific => true;
        public override bool FetchFull => true;
        public override string ApiVersion => "2021-02-01";
        public Subnet[] Subnets { get; set; }

        public override void Enrich(JObject full)
        {
            Subnets = full["properties"]["subnets"].Select(x => new Subnet
            {
                Name = x.Value<string>("name"),
                AddressPrefix = x["properties"].Value<string>("addressPrefix")
            }).ToArray();
        }

        public class Subnet
        {
            public string Name { get; set; }
            public string AddressPrefix { get; set; }
        }

        public override IEnumerable<string> ToDrawIo(int x, int y)
        {
            var subnetIndex = 1;
            return base.ToDrawIo(x, y)
                .Union(new[]
                {
                    @$"
<mxCell id=""{Id}-image"" value=""""  parent=""{Id}"" style=""html=1;image;image=img/lib/azure2/networking/Virtual_Networks.svg;fontSize=12;labelPosition=bottom"" vertex=""1"">
<mxGeometry x=""10"" y=""10"" width=""148"" height=""30"" as=""geometry"" />
</mxCell>"
                })
                .Union(Subnets.Select(s =>
                    @$"
<mxCell id=""{Id}.{subnetIndex++}"" value=""{s.Name}"" parent=""{Id}"" style=""text"" vertex=""1""><mxGeometry x=""10"" y=""{subnetIndex * 25}"" width=""148"" height=""30"" as=""geometry"" />
</mxCell>"));
        }

        protected override int ExpectedHeight()
        {
            return Height * Subnets.Length;
        }
    }
}