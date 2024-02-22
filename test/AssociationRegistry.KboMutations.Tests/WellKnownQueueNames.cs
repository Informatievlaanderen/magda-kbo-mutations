namespace AssociationRegistry.KboMutations.Tests;

public static class WellKnownQueueNames
{
    public const string MutationFileQueue = $"{AssocationRegistry.KboMutations.WellKnownQueueNames.MutationFileQueueUrl}-test";
    public const string SyncQueue = $"{AssocationRegistry.KboMutations.WellKnownQueueNames.SyncQueueUrl}-test";
}