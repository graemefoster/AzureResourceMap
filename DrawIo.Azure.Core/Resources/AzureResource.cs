using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources
{
    class AzureResource
    {
        public virtual bool IsSpecific => false;
        public virtual bool FetchFull => false;
        public string Id { get; set; }
        public string Name { get; set; }
        public virtual string Image { get; set; }
        public string Type { get; set; }
        public string Location { get; set; }
        public virtual string ApiVersion => "2020-11-01";

        public virtual void Enrich(JObject full)
        {
        }

        public virtual IEnumerable<string> ToDrawIo(int x, int y)
        {
            var cellStyle = string.IsNullOrEmpty(Image)
                ? "rounded=0;whiteSpace=wrap;html=1;"
                : $"html=1;image;image={Image};fontSize=12;labelPosition=bottom";
            return new[]
            {
                @$"<mxCell id=""{Id}"" value=""{Name}"" style=""{cellStyle}"" vertex=""1"" parent=""1"">
                <mxGeometry x=""{ExpectedX(x)}"" y=""{ExpectedY(y)}"" width=""{ExpectedWidth()}"" height=""{ExpectedHeight()}"" as=""geometry"" />
                </mxCell>"
            };
        }

        public virtual IEnumerable<string> Link(IEnumerable<AzureResource> allResources)
        {
            return Array.Empty<string>();
        }

        protected const int Width = 50;
        protected const int Height = 50;

        protected int ExpectedX(int x)
        {
            return x * (Width + 10);
        }

        protected int ExpectedY(int y)
        {
            return y * (Height + 10);
        }

        protected virtual int ExpectedWidth()
        {
            return Width;
        }

        protected virtual int ExpectedHeight()
        {
            return Height;
        }

        protected string Link(AzureResource to)
        {
            return $@"
<mxCell id=""{Guid.NewGuid().ToString().Replace("-", "")}"" style=""edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;entryX=-0.047;entryY=0.476;entryDx=0;entryDy=0;entryPerimeter=0;"" edge=""1"" parent=""1"" source=""{Id}"" target=""{to.Id}"">
    <mxGeometry relative=""1"" as=""geometry"" />
</mxCell>";
        }
    }
}