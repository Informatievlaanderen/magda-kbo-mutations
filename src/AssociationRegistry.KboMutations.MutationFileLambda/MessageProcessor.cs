using System.Globalization;
using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.S3;
using Amazon.SQS;
using Amazon.SQS.Model;
using AssocationRegistry.KboMutations.Configuration;
using AssocationRegistry.KboMutations.Messages;
using AssocationRegistry.KboMutations.Models;
using AssocationRegistry.KboMutations.Notifications;
using AssociationRegistry.Kbo;
using AssociationRegistry.Notifications;
using CsvHelper;
using CsvHelper.Configuration;

namespace AssociationRegistry.KboMutations.MutationFileLambda;

public class MessageProcessor
{
    private readonly KboSyncConfiguration _kboSyncConfiguration;
    private readonly IAmazonS3 _s3Client;
    private readonly IAmazonSQS _sqsClient;
    private readonly INotifier _notifier;

    public MessageProcessor(IAmazonS3 s3Client,
        IAmazonSQS sqsClient,
        INotifier notifier,
        KboSyncConfiguration kboSyncConfiguration)
    {
        _s3Client = s3Client;
        _sqsClient = sqsClient;
        _notifier = notifier;
        _kboSyncConfiguration = kboSyncConfiguration;
    }

    public async Task ProcessMessage(SQSEvent sqsEvent,
        ILambdaLogger contextLogger,
        CancellationToken cancellationToken)
    {
        contextLogger.LogInformation($"{nameof(_kboSyncConfiguration.MutationFileBucketName)}:{_kboSyncConfiguration.MutationFileBucketName}");
        contextLogger.LogInformation($"{nameof(_kboSyncConfiguration.MutationFileQueueUrl)}:{_kboSyncConfiguration.MutationFileQueueUrl}");
        contextLogger.LogInformation($"{nameof(_kboSyncConfiguration.SyncQueueUrl)}:{_kboSyncConfiguration.SyncQueueUrl}");

        var batchResponse = new SendMessageBatchResponse();
        var encounteredExceptions = new List<Exception>();

        foreach (var record in sqsEvent.Records)
        {
            contextLogger.LogInformation("Processing record body: " + record.Body);

            var message = JsonSerializer.Deserialize<TeVerwerkenMutatieBestandMessage>(record.Body);

            try
            {
                var response = await Handle(contextLogger, message, cancellationToken);
                batchResponse.Successful.AddRange(response.Successful);
                batchResponse.Failed.AddRange(response.Failed);
            }
            catch (Exception ex)
            {
                await _notifier.Notify(new KboMutationFileLambdaMessageProcessorGefaald(ex));
                encounteredExceptions.Add(ex);
            };
        }
        
        await _notifier.Notify(new KboMutationFileLambdaSqsBerichtBatchVerstuurd(batchResponse.Successful));
        await _notifier.Notify(new KboMutationFileLambdaSqsBerichtBatchNietVerstuurd(batchResponse.Failed));

        foreach (var batchResultErrorEntry in batchResponse.Failed)
            contextLogger.LogWarning($"KBO mutatie file lambda kon message '{batchResultErrorEntry.Id}' niet verzenden: '{batchResultErrorEntry.Message}'");

        if (encounteredExceptions.Any())
            throw new AggregateException(encounteredExceptions);
    }

    private async Task<SendMessageBatchResponse> Handle(ILambdaLogger contextLogger,
        TeVerwerkenMutatieBestandMessage? message,
        CancellationToken cancellationToken)
    {
        var fetchMutatieBestandResponse = await _s3Client.GetObjectAsync(
            _kboSyncConfiguration.MutationFileBucketName,
            message.Key,
            cancellationToken);

        var content = await FetchMutationFileContent(fetchMutatieBestandResponse.ResponseStream, cancellationToken);

        contextLogger.LogInformation($"MutatieBestand found");

        var mutatielijnen = ReadMutationLines(contextLogger, content);

        contextLogger.LogInformation($"Found {mutatielijnen.Count} mutatielijnen");

        var messagesToSend = new List<SendMessageBatchRequestEntry>();
        foreach (var mutatielijn in mutatielijnen)
        {
            contextLogger.LogInformation($"Sending {mutatielijn.Ondernemingsnummer} to synchronize queue");

            var messageBody = JsonSerializer.Serialize(
                new TeSynchroniserenKboNummerMessage(mutatielijn.Ondernemingsnummer));

            messagesToSend.Add(new SendMessageBatchRequestEntry(
                $"{mutatielijn.Ondernemingsnummer}-{mutatielijn.DatumModificatie.Ticks}", messageBody));
        }

        var response = await _sqsClient.SendMessageBatchAsync(_kboSyncConfiguration.SyncQueueUrl, messagesToSend, cancellationToken);

        await _s3Client.DeleteObjectAsync(_kboSyncConfiguration.MutationFileBucketName, message.Key, cancellationToken);

        return response;
    }

    private static async Task<string> FetchMutationFileContent(
        Stream mutatieBestandStream,
        CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(mutatieBestandStream);

        return await reader.ReadToEndAsync(cancellationToken);
    }

    private static List<MutatieLijn> ReadMutationLines(
        ILambdaLogger contextLogger,
        string content)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null
        };

        using var stringReader = new StringReader(content);
        using var csv = new CsvReader(stringReader, config);

        return csv.GetRecords<MutatieLijn>().ToList();
    }
}