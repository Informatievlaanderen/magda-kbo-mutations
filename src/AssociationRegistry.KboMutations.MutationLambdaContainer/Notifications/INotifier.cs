namespace AssociationRegistry.KboMutations.MutationLambdaContainer.Notifications;

public interface INotifier
{
    Task NotifySuccess(int numberOfFiles);
    Task NotifyFailure(string reason);
}