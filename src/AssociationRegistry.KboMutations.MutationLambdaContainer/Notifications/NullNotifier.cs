namespace AssociationRegistry.KboMutations.MutationLambdaContainer.Notifications;

public class NullNotifier : INotifier
{
    public Task NotifySuccess(int numberOfFiles)
    {
        return Task.CompletedTask;
    }

    public Task NotifyFailure(string reason)
    {
        return Task.CompletedTask;
    }
}