namespace DrawIo.Azure.Core.Resources
{
    class AzureList<T> where T : AzureResource
    {
        public string NextLink { get; set; }
        public T[] Value { get; set; }
    }
}