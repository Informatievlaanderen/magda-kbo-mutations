using System.Globalization;
using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.S3;
using Amazon.SQS;
using Amazon.SQS.Model;
using AssocationRegistry.KboMutations;
using AssocationRegistry.KboMutations.Messages;
using AssocationRegistry.KboMutations.Models;
using AssocationRegistry.KboMutations.Notifications;
using AssociationRegistry.KboMutations.Notifications;
using CsvHelper;
using CsvHelper.Configuration;

namespace AssociationRegistry.KboMutations.MutationFileLambda;

public class MessageProcessor
{
    private readonly AmazonKboSyncConfiguration _amazonKboSyncConfiguration;
    private readonly IAmazonS3 _s3Client;
    private readonly IAmazonSQS _sqsClient;
    private readonly INotifier _notifier;

    public MessageProcessor(IAmazonS3 s3Client, 
        IAmazonSQS sqsClient,
        INotifier notifier,
        AmazonKboSyncConfiguration amazonKboSyncConfiguration)
    {
        _s3Client = s3Client;
        _sqsClient = sqsClient;
        _notifier = notifier;
        _amazonKboSyncConfiguration = amazonKboSyncConfiguration;
    }

    public async Task ProcessMessage(SQSEvent sqsEvent, 
        ILambdaLogger contextLogger,
        CancellationToken cancellationToken)
    {
        contextLogger.LogInformation($"{nameof(_amazonKboSyncConfiguration.MutationFileBucketName)}:{_amazonKboSyncConfiguration.MutationFileBucketName}");
        contextLogger.LogInformation($"{nameof(_amazonKboSyncConfiguration.MutationFileQueueUrl)}:{_amazonKboSyncConfiguration.MutationFileQueueUrl}");
        contextLogger.LogInformation($"{nameof(_amazonKboSyncConfiguration.SyncQueueUrl)}:{_amazonKboSyncConfiguration.SyncQueueUrl}");

        foreach (var record in sqsEvent.Records)
        {
            contextLogger.LogInformation("Processing record body: " + record.Body);   
            
            var message = JsonSerializer.Deserialize<TeVerwerkenMutatieBestandMessage>(record.Body);

            try
            {
                var response = await Handle(contextLogger, message, cancellationToken);

                if (response.Failed.Any())
                {
                    foreach (var fail in response.Failed)
                    {
                        contextLogger.LogWarning($"Kbo Mutatie File Lambda kon message '{fail.Id}' niet verzenden: '{fail.Message}'");
                    }

                    await _notifier.Notify(new KboMutationFileLambdaKonSqsBerichtBatchNietVersturen(response.Failed.Count));
                }
            }
            catch(Exception ex)
            {
                await _notifier.Notify(new KboMutationFileLambdaMessageProcessorGefaald(ex));
                
                throw;
            }
        }
    }

    private async Task<SendMessageBatchResponse> Handle(ILambdaLogger contextLogger, 
        TeVerwerkenMutatieBestandMessage? message,
        CancellationToken cancellationToken)
    {
        var fetchMutatieBestandResponse = await _s3Client.GetObjectAsync(
            _amazonKboSyncConfiguration.MutationFileBucketName, 
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
            
            var messageBody = JsonSerializer.Serialize(new TeSynchroniserenKboNummerMessage(mutatielijn.Ondernemingsnummer, fetchMutatieBestandResponse.Key));
            
            messagesToSend.Add(new SendMessageBatchRequestEntry(
                $"{mutatielijn.Ondernemingsnummer}-{mutatielijn.DatumModificatie.Ticks}", messageBody));
        }

        var response = await _sqsClient.SendMessageBatchAsync(_amazonKboSyncConfiguration.SyncQueueUrl, messagesToSend, cancellationToken);

        await _s3Client.DeleteObjectAsync(_amazonKboSyncConfiguration.MutationFileBucketName, message.Key, cancellationToken);

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