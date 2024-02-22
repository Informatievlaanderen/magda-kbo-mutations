using System.Diagnostics;
using Amazon.Lambda.TestUtilities;
using Amazon.SQS.Model;
using AssocationRegistry.KboMutations;
using AssociationRegistry.KboMutations.MutationLambda;
using AssociationRegistry.KboMutations.MutationLambda.Configuration;
using AssociationRegistry.KboMutations.MutationLambda.Ftps;
using AssociationRegistry.KboMutations.Tests.Fixtures;

namespace AssociationRegistry.KboMutations.Integration.Tests.Given_TeVerwerkenMutatieBestand_In_Queue_And_S3.Fixtures;

public class With_TeVerwerkenMutatieBestand_FromLocalstack : WithLocalstackFixture
{
    public With_TeVerwerkenMutatieBestand_FromLocalstack() : base(
        WellKnownBucketNames.MutationFileBucketName,
        WellKnownQueueNames.MutationFileQueueUrl,
        WellKnownQueueNames.SyncQueueUrl)
    {
        
    }
    
    public IFtpsClient SecureFtpClient { get; private set; }

    protected override async Task SetupAsync()
    {
        var logger = new TestLambdaLogger();
        var sftpPath = "../../../../../sftp";
        var seedFolder = "seed";
        var inFolder = "files/in";

        var certPath = $"{sftpPath}/cert/custom_vsftpd.crt";
        var keyPath = $"{sftpPath}/cert/custom_vsftpd.der";

        foreach (var mutatieBestand in Directory.EnumerateFileSystemEntries(Path.Join(sftpPath, seedFolder)))
        {
            File.Copy(mutatieBestand, Path.Join(sftpPath, inFolder, new FileInfo(mutatieBestand).Name), true);
        }
        
        var kboMutationsConfiguration = new KboMutationsConfiguration
        {
            Host = "localhost",
            Port = 21000,
            Username = "files",
            Password = "FSBhuNOR",
            SourcePath = "in",
            CachePath = "archive",
            CertPath = certPath,
            CaCertPath = string.Empty,
            KeyPath = keyPath,
            KeyType = "DER",
            LockEnabled = false,
            CurlLocation = "curl",
        };
        
        await ClearQueue(KboSyncConfiguration.MutationFileQueueUrl);
        await ClearQueue(KboSyncConfiguration.SyncQueueUrl);

        SecureFtpClient = new CurlFtpsClient(logger, kboMutationsConfiguration);

        var mutatieBestandProcessor = new MutatieBestandProcessor(logger, SecureFtpClient, AmazonS3Client,
            AmazonSqsClient, kboMutationsConfiguration,
            KboSyncConfiguration);

        await mutatieBestandProcessor.ProcessAsync();
        ReceivedMessages = await FetchMessages(KboSyncConfiguration.SyncQueueUrl);
    }

    public async Task<List<Message>> FetchMessages(string syncQueueUrl)
    {
        var stopWatch = Stopwatch.StartNew();
        var allReceivedMessages = new List<Message>();
        const int maxWaitTimeSeconds = 3; 
        const int totalOperationTimeSeconds = 3;

        while (stopWatch.Elapsed < TimeSpan.FromSeconds(totalOperationTimeSeconds))
        {
            var response = await AmazonSqsClient.ReceiveMessageAsync(
                new ReceiveMessageRequest(syncQueueUrl)
                {
                    WaitTimeSeconds = maxWaitTimeSeconds,
                    MaxNumberOfMessages = 10,
                });

            if (response.Messages.Any()) allReceivedMessages.AddRange(response.Messages);
        }

        return allReceivedMessages;
    }

    private async Task ClearQueue(string queueUrl)
    {
        await AmazonSqsClient.PurgeQueueAsync(queueUrl);
    }

    public List<Message> ReceivedMessages { get; set; }
}