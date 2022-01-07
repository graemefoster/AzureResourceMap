using DrawIo.Azure.Core.Diagrams;

namespace DrawIo.Azure.Core.Resources;

internal interface ICanWriteToLogAnalyticsWorkspaces
{
    bool DoYouWriteTo(string customerId);
    void CreateFlowBackToMe(LogAnalyticsWorkspace workspace);
}