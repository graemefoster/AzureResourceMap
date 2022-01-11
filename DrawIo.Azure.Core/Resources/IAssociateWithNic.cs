namespace DrawIo.Azure.Core.Resources;

internal interface IAssociateWithNic
{
    string[] Nics { get; }
}

internal interface ICanBeAccessedViaAHostName
{
    bool CanIAccessYouOnThisHostName(string hostname);
}