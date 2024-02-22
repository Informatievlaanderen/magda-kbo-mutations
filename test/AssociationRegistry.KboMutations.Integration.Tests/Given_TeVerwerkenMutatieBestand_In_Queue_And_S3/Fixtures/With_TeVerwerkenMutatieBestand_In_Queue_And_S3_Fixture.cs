using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Text.Json;
using Amazon.S3.Model;
using Amazon.SQS.Model;
using AssocationRegistry.KboMutations;
using AssocationRegistry.KboMutations.Messages;
using AssocationRegistry.KboMutations.Models;
using AssociationRegistry.KboMutations.Tests.Customizations;
using AssociationRegistry.KboMutations.Tests.Fixtures;
using AutoFixture;
using CsvHelper;
using FluentAssertions;
using Xunit.Abstractions;

namespace AssociationRegistry.KboMutations.Integration.Tests.Given_TeVerwerkenMutatieBestand_In_Queue_And_S3.Fixtures;

public class With_TeVerwerkenMutatieBestand_In_Queue_And_S3_Fixture : WithLocalstackFixture
{
    public TeVerwerkenMutatieBestandMessage TeVerwerkenMutatieBestandMessage { get; }
    public MutatieLijn[] MutatieLijnen { get; set; }

    public With_TeVerwerkenMutatieBestand_In_Queue_And_S3_Fixture() : base(
        WellKnownBucketNames.MutationFileBucketName,
        WellKnownQueueNames.MutationFileQueueUrl,
        WellKnownQueueNames.SyncQueueUrl
 )
    {
        TeVerwerkenMutatieBestandMessage = CustomFixture.Default.Create<TeVerwerkenMutatieBestandMessage>();
        MutatieLijnen = CustomFixture.Default
            .CreateMany<MutatieLijn>()
            .ToArray();
    }

    protected override async Task SetupAsync()
    {
        await ClearQueue(KboSyncConfiguration.MutationFileQueueUrl);
        await ClearQueue(KboSyncConfiguration.SyncQueueUrl);
        
        await WriteMutationFileToBucket(
            TeVerwerkenMutatieBestandMessage, 
            KboSyncConfiguration.MutationFileBucketUrl!);

        await SendMutationFileToQueue(
            TeVerwerkenMutatieBestandMessage, 
            KboSyncConfiguration.MutationFileQueueUrl);
        
        ReceivedMessages = await FetchMessages();
    }

    public async Task<List<Message>> FetchMessages()
    {
        var stopWatch = Stopwatch.StartNew();
        var allReceivedMessages = new List<Message>();
        const int maxWaitTimeSeconds = 3; 
        const int totalOperationTimeSeconds = 3;

        while (stopWatch.Elapsed < TimeSpan.FromSeconds(totalOperationTimeSeconds))
        {
            var response = await AmazonSqsClient.ReceiveMessageAsync(
                new ReceiveMessageRequest(KboSyncConfiguration.SyncQueueUrl)
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

    private async Task WriteMutationFileToBucket(TeVerwerkenMutatieBestandMessage mutationFile, string mutationFileBucket)
    {
        await using var writer = new StringWriter();
        await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        await csv.WriteRecordsAsync(MutatieLijnen);

        var putObjectResponse = await AmazonS3Client.PutObjectAsync(new PutObjectRequest
        {
            BucketName = mutationFileBucket,
            Key = mutationFile.Key,
            ContentBody = writer.ToString()
        });
    }

    private async Task SendMutationFileToQueue(TeVerwerkenMutatieBestandMessage mutationFile, string mutationFileQueueUrl)
    {
        var sendMessageResponse = await AmazonSqsClient.SendMessageAsync(new SendMessageRequest
        {
            QueueUrl = mutationFileQueueUrl,
            MessageBody = JsonSerializer.Serialize(mutationFile)
        });

        sendMessageResponse.HttpStatusCode.Should().Be(HttpStatusCode.OK);
    }
}