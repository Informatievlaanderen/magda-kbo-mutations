using AssocationRegistry.KboMutations.Notifications;

namespace AssociationRegistry.KboMutations.Notifications;

public class NullNotifier : INotifier
{
    public Task Notify(IMessage message) => Task.CompletedTask;
}