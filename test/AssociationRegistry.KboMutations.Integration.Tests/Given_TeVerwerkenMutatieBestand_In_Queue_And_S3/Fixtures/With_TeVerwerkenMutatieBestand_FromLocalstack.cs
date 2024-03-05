using System.Diagnostics;
using Amazon.Lambda.TestUtilities;
using Amazon.SQS.Model;
using AssocationRegistry.KboMutations;
using AssociationRegistry.EventStore;
using AssociationRegistry.Framework;
using AssociationRegistry.Kbo;
using AssociationRegistry.KboMutations.MutationLambdaContainer;
using AssociationRegistry.KboMutations.MutationLambdaContainer.Abstractions;
using AssociationRegistry.KboMutations.MutationLambdaContainer.Configuration;
using AssociationRegistry.KboMutations.MutationLambdaContainer.Ftps;
using AssociationRegistry.KboMutations.Tests.Fixtures;
using AssociationRegistry.Vereniging;
using AutoBogus;
using FluentAssertions;
using Marten;
using Marten.Events;
using NodaTime;
using Npgsql;
using Weasel.Core;

namespace AssociationRegistry.KboMutations.Integration.Tests.Given_TeVerwerkenMutatieBestand_In_Queue_And_S3.Fixtures;

public class With_TeVerwerkenMutatieBestand_FromLocalstack : WithLocalstackFixture
{
    public static KboNummer KboNummerBekendeVereniging = KboNummer.Create("0442528054");
    public static KboNummer KboNummerOnbekendeVereniging = KboNummer.Create("0000000097");
    
    public static Dictionary<KboNummer, VCode> KboNummersToSeed =
    new()
    {
        { KboNummerBekendeVereniging, VCode.Create("V0001001")},
        { KboNummerOnbekendeVereniging, VCode.Create("V0001002")},
    };

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
            AdditionalParams = "-k"
        };

        await SeedVerenigingen(KboNummersToSeed);

        await ClearQueue(KboSyncConfiguration.MutationFileQueueUrl);
        await ClearQueue(KboSyncConfiguration.SyncQueueUrl);

        SecureFtpClient = new CurlFtpsClient(logger, kboMutationsConfiguration);

        var mutatieBestandProcessor = new MutatieBestandProcessor(logger, SecureFtpClient, AmazonS3Client,
            AmazonSqsClient, kboMutationsConfiguration,
            KboSyncConfiguration);

        await mutatieBestandProcessor.ProcessAsync();
        // ReceivedMessages = await FetchMessages(KboSyncConfiguration.SyncQueueUrl);
    }

    private static async Task SeedVerenigingen(Dictionary<KboNummer, VCode> kboNummersToSeed)
    {
        var documentStore = CreateDocumentStore();
        
        await documentStore.Storage.ApplyAllConfiguredChangesToDatabaseAsync();
        
        await documentStore.Storage.Database.DeleteAllEventDataAsync();

        var repo = new VerenigingsRepository(new EventStore.EventStore(documentStore));
        foreach (var (kboNummer, vCode) in kboNummersToSeed)
        {
            var verenigingMetRechtspersoonlijkheid = VerenigingMetRechtspersoonlijkheid.Registreer(
                vCode,
                new VerenigingVolgensKbo
                {
                    KboNummer = KboNummer.Create(kboNummer),
                    Adres = new AdresVolgensKbo(),
                    Contactgegevens = new ContactgegevensVolgensKbo(),
                    Naam = $"Bedrijf {kboNummer}",
                    Startdatum = DateOnly.MinValue,
                    Type = Verenigingstype.VZW,
                    Vertegenwoordigers = Array.Empty<VertegenwoordigerVolgensKbo>(),
                    KorteNaam = $"{{B-{kboNummer}}}"
                });
            var result = await repo.Save(verenigingMetRechtspersoonlijkheid, new CommandMetadata("OVO002949", new Instant(), Guid.NewGuid()));

            result.Sequence.Should().BeGreaterThan(0);
        }
    }

    public static DocumentStore CreateDocumentStore()
    {
        var documentStore = DocumentStore.For(opts =>
        {
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder();
            connectionStringBuilder.Host = "localhost";
            connectionStringBuilder.Database = "verenigingsregister";
            connectionStringBuilder.Username = "root";
            connectionStringBuilder.Password = "root";

            opts.Connection(connectionStringBuilder.ToString());
            opts.Events.StreamIdentity = StreamIdentity.AsString;
            // opts.Serializer(CreateCustomMartenSerializer());
            opts.Events.MetadataConfig.EnableAll();
            opts.AutoCreateSchemaObjects = AutoCreate.All;
        });
        return documentStore;
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