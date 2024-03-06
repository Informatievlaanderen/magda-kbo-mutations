using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using AssocationRegistry.KboMutations;
using AssocationRegistry.KboMutations.Messages;
using AssocationRegistry.KboMutations.Models;
using AssociationRegistry.KboMutations.MutationLambdaContainer.Abstractions;
using AssociationRegistry.KboMutations.MutationLambdaContainer.Configuration;
using AssociationRegistry.KboMutations.MutationLambdaContainer.Ftps;
using AssociationRegistry.KboMutations.MutationLambdaContainer.Notifications;

namespace AssociationRegistry.KboMutations.MutationLambdaContainer;

public class MutatieBestandProcessor
{
    private readonly ILambdaLogger _logger;
    private readonly IFtpsClient _ftpsClient;
    private readonly IAmazonS3 _s3Client;
    private readonly IAmazonSQS _sqsClient;
    private readonly KboMutationsConfiguration _kboMutationsConfiguration;
    private readonly AmazonKboSyncConfiguration _kboSyncConfiguration;
    private readonly INotifier _notifier;
    private readonly FtpUriBuilder _baseUriBuilder;

    public MutatieBestandProcessor(ILambdaLogger logger, IFtpsClient ftpsClient, IAmazonS3 s3Client,
        IAmazonSQS sqsClient, KboMutationsConfiguration kboMutationsConfiguration,
        AmazonKboSyncConfiguration kboSyncConfiguration, INotifier notifier)
    {
        _logger = logger;
        _ftpsClient = ftpsClient;
        _s3Client = s3Client;
        _sqsClient = sqsClient;
        _kboMutationsConfiguration = kboMutationsConfiguration;
        _kboSyncConfiguration = kboSyncConfiguration;
        _notifier = notifier;

        _baseUriBuilder = new FtpUriBuilder(_kboMutationsConfiguration.Host, _kboMutationsConfiguration.Port);
    }

    public async Task ProcessAsync(CancellationToken cancellationToken = default)
    {
        var magdaMutatieBestanden = GetMagdaMutatieBestanden()
            .ToList();
        
        foreach (var magdaMutatieBestand in magdaMutatieBestanden)
        {
            using var stream = new MemoryStream();
            
            // Download file from MAGDA
            var fullNameUri = _baseUriBuilder.WithPath(magdaMutatieBestand.FtpPath).ToString();
            var fileName = Path.GetFileName(fullNameUri);
            var localDestinationFilePath = Path.Join(_kboMutationsConfiguration.DownloadPath, fileName);
            
            if(!_ftpsClient.Download(stream, fullNameUri, localDestinationFilePath))
                   throw new ApplicationException($"Bestand {magdaMutatieBestand.Name} kon niet opgehaald worden");
            stream.Seek(0, SeekOrigin.Begin);
            
            // Save file on S3
            await _s3Client.PutObjectAsync(new PutObjectRequest
            {
                BucketName = _kboSyncConfiguration.MutationFileBucketName,
                Key = magdaMutatieBestand.Name,
                InputStream = stream
            }, cancellationToken);
            
            // Notify about file on SQS
            await _sqsClient.SendMessageAsync(new SendMessageRequest(
                _kboSyncConfiguration.MutationFileQueueUrl, 
                JsonSerializer.Serialize(new TeVerwerkenMutatieBestandMessage(magdaMutatieBestand.Name))
            )
            {
                MessageGroupId = Guid.NewGuid().ToString()
            }, cancellationToken);
            
            // Archive file from MAGDA
            ArchiveerMagdaMutatieBestand(magdaMutatieBestand);
        }
        await _notifier.NotifySuccess(magdaMutatieBestanden.Count);
    }

    private IEnumerable<MagdaMutatieBestand> GetMagdaMutatieBestanden()
    {
        _logger.LogInformation($"Fetching mutation files from folder {_kboMutationsConfiguration.SourcePath}");

        var sourceDirectoryUri = _baseUriBuilder.AppendDir(_kboMutationsConfiguration.SourcePath); 
        var curlListResult = _ftpsClient.GetListing(sourceDirectoryUri.ToString());
        var mutationFiles =
            FtpsListParser.Parse(sourceDirectoryUri, curlListResult)
                .Select(ftpsListItem => new MagdaMutatieBestand(ftpsListItem.FullName, ftpsListItem.Name))
                .OrderBy(item => item.FtpPath)
                .ToList();

        _logger.LogInformation($"Found {mutationFiles.Count} mutation files to process");
        return mutationFiles;
    }
    
    private void ArchiveerMagdaMutatieBestand(MagdaMutatieBestand magdaMutatieBestand)
    {
        _logger.LogInformation($"Archiving {magdaMutatieBestand.FtpPath} to {_kboMutationsConfiguration.CachePath}");

        var sourceFullNameUri = _baseUriBuilder.WithPath(magdaMutatieBestand.FtpPath);
        var destinationFullNameUri = _baseUriBuilder
            .AppendDir(_kboMutationsConfiguration.CachePath)
            .AppendFileName(sourceFullNameUri.FileName);

        _ftpsClient.MoveFile(_baseUriBuilder.ToString(),
            sourceFullNameUri.Path,
            destinationFullNameUri.Path);
    }
}