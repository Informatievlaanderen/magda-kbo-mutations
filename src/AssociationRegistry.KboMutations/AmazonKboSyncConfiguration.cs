namespace AssocationRegistry.KboMutations;

public record AmazonKboSyncConfiguration
{
    public string? MutationFileBucketUrl { get; set; }
    public string MutationFileQueueUrl { get; set; }
    public string SyncQueueUrl { get; set; }
}