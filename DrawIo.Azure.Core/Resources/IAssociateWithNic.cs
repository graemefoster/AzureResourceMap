namespace DrawIo.Azure.Core.Resources;

internal interface IAssociateWithNic
{
    string[] Nics { get; }
}

internal interface ICanBeAccessedViaHttp
{
    bool CanIAccessYouOnThisHostName(string hostname);
}
