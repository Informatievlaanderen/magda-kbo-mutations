using Amazon.Runtime;
using Amazon.S3;
using Amazon.SQS;
using AssocationRegistry.KboMutations;
using Xunit;

namespace AssociationRegistry.KboMutations.Tests.Fixtures;

public abstract class WithLocalstackFixture : IAsyncLifetime
{
    public WithLocalstackFixture(
        string mutationFileBucket = WellKnownBucketNames.MutationFileBucket,
        string mutationFileQueue = WellKnownQueueNames.MutationFileQueue,
        string syncQueue = WellKnownQueueNames.SyncQueue
        )
    {
        var credentials = new BasicAWSCredentials("123", "123");

        AmazonS3Client = new AmazonS3Client(credentials, new AmazonS3Config
        {
            ServiceURL = "http://localhost:4566", 
            ForcePathStyle = true
        });
        AmazonSqsClient = new AmazonSQSClient(credentials, new AmazonSQSConfig
        {
            ServiceURL = "http://localhost:4566",
        });
        
        KboSyncConfiguration = new AmazonKboSyncConfiguration
        {
            MutationFileBucketName = mutationFileBucket,
            MutationFileQueueUrl = mutationFileQueue,
            SyncQueueUrl = syncQueue,
        };
    }
    
    public IAmazonS3 AmazonS3Client { get; }
    public IAmazonSQS AmazonSqsClient { get; }
    public AmazonKboSyncConfiguration KboSyncConfiguration { get; private set; }

    public async Task InitializeAsync()
    {
        await AmazonS3Client.PutBucketAsync(KboSyncConfiguration.MutationFileBucketName);
        var mutationFileQueueResponse = await AmazonSqsClient.CreateQueueAsync(KboSyncConfiguration.MutationFileQueueUrl);
        var syncQueueResponse = await AmazonSqsClient.CreateQueueAsync(KboSyncConfiguration.SyncQueueUrl);
        
        KboSyncConfiguration = KboSyncConfiguration with
        {
            MutationFileQueueUrl = 
                mutationFileQueueResponse.QueueUrl,
            
            SyncQueueUrl = 
                syncQueueResponse.QueueUrl,
        };
        
        await SetupAsync();
    }

    public async Task DisposeAsync()
    {
        // if (KboSyncConfiguration.MutationFileBucket.Contains("test"))
        // {
        //     var listObjectsResponse = await AmazonS3Client.ListObjectsAsync(KboSyncConfiguration.MutationFileBucket);
        //     foreach (var s3Object in listObjectsResponse.S3Objects)
        //         await AmazonS3Client.DeleteObjectAsync(s3Object.BucketName, s3Object.Key);
        //     await AmazonS3Client.DeleteBucketAsync(KboSyncConfiguration.MutationFileBucket);
        // }
        // await AmazonSQSClient.DeleteQueueAsync(KboSyncConfiguration.MutationFileQueueUrl);
        // await AmazonSQSClient.DeleteQueueAsync(KboSyncConfiguration.SyncQueueUrl);
    }

    protected abstract Task SetupAsync();
}