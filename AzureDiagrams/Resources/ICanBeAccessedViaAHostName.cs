namespace DrawIo.Azure.Core.Resources;

internal interface ICanBeAccessedViaAHostName
{
    bool CanIAccessYouOnThisHostName(string hostname);
}