namespace DrawIo.Azure.Core.Resources;

public interface ICanInjectIntoASubnet
{
    string[] SubnetIdsIAmInjectedInto { get; }
}