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
        public string Type { get; set; }
        public string Location { get; set; }
        public virtual string ApiVersion => "2020-11-01";

        public virtual void Enrich(JObject full)
        {
        }

        public virtual IEnumerable<string> ToDrawIo(int x, int y)
        {
            return new[]
            {
                @$"<mxCell id=""{Id}"" value=""{Name}"" style=""rounded=0;whiteSpace=wrap;html=1;"" vertex=""1"" parent=""1"">
                <mxGeometry x=""{ExpectedX(x)}"" y=""{ExpectedY(y)}"" width=""{ExpectedWidth()}"" height=""{ExpectedHeight()}"" as=""geometry"" />
                </mxCell>"
            };
        }

        protected const int Width = 200;
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
    }
}