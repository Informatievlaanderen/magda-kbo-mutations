using AssociationRegistry.Notifications;

namespace AssocationRegistry.KboMutations.Notifications;

public class NullNotifier : INotifier
{
    public Task Notify(IMessage message) => Task.CompletedTask;
}