namespace AssociationRegistry.KboMutations.Notifications;

public class NullNotifier : INotifier
{
    public Task NotifyLambdaTriggered() => Task.CompletedTask;

    public Task NotifyLambdaFinished() => Task.CompletedTask;

    public Task NotifyLambdaFailed(string exceptionMessage) => Task.CompletedTask;

    public Task NotifyDownloadFileSuccess(int numberOfFiles) => Task.CompletedTask;

    public Task NotifyFailure(string reason) => Task.CompletedTask;
}