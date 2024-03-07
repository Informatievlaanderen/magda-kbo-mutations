using System.Text.Json.Serialization;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.Lambda.SQSEvents;
using Amazon.S3;
using Amazon.SimpleSystemsManagement;
using Amazon.SQS;
using AssocationRegistry.KboMutations;
using AssocationRegistry.KboMutations.Configuration;
using AssociationRegistry.Events;
using AssociationRegistry.EventStore;
using AssociationRegistry.KboMutations.SyncLambda.Aws;
using AssociationRegistry.KboMutations.SyncLambda.Configuration;
using AssociationRegistry.Magda;
using AssociationRegistry.Magda.Configuration;
using AssociationRegistry.Magda.Models;
using AssociationRegistry.Vereniging;
using Marten;
using Marten.Events;
using Marten.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Npgsql;
using Weasel.Core;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace AssociationRegistry.KboMutations.SyncLambda;

public class Function
{
    private static async Task Main()
    {
        var handler = FunctionHandler;
        await LambdaBootstrapBuilder.Create(handler, new SourceGeneratorLambdaJsonSerializer<LambdaFunctionJsonSerializerContext>())
            .Build()
            .RunAsync();
    }

    private static async Task FunctionHandler(SQSEvent @event, ILambdaContext context)
    {
        var s3Client = new AmazonS3Client();
        var sqsClient = new AmazonSQSClient();

        var configurationBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", true, true)
            .AddEnvironmentVariables();

        var configuration = configurationBuilder.Build();
        var awsConfigurationSection = configuration
            .GetSection("AWS");

        var paramNamesConfiguration = configuration
            .GetSection(ParamNamesConfiguration.Section)
            .Get<ParamNamesConfiguration>();

        var processor = new MessageProcessor(s3Client, sqsClient, new KboSyncConfiguration
        {
            MutationFileQueueUrl = awsConfigurationSection[nameof(WellKnownQueueNames.MutationFileQueueUrl)],
            SyncQueueUrl = awsConfigurationSection[nameof(WellKnownQueueNames.SyncQueueUrl)]!
        });

        var ssmClientWrapper = new SsmClientWrapper(new AmazonSimpleSystemsManagementClient());
        var magdaOptions = await GetMagdaOptions(configuration, ssmClientWrapper, paramNamesConfiguration);

        var store = await SetUpDocumentStore(configuration, context.Logger, ssmClientWrapper, paramNamesConfiguration);
        
        var repository = new VerenigingsRepository(new EventStore.EventStore(store));
        
        var loggerFactory = LoggerFactory.Create(builder => 
        {
            builder.AddProvider(new LambdaLoggerProvider(context.Logger));
        });
        
        var geefOndernemingService = new MagdaGeefVerenigingService(
            new MagdaCallReferenceRepository(store.LightweightSession()),
            new MagdaFacade(magdaOptions, loggerFactory.CreateLogger<MagdaFacade>()),
            new TemporaryMagdaVertegenwoordigersSection(),
            loggerFactory.CreateLogger<MagdaGeefVerenigingService>());

        context.Logger.LogInformation(JsonSerializer.Serialize(magdaOptions));

        context.Logger.LogInformation($"{@event.Records.Count} RECORDS RECEIVED INSIDE SQS EVENT");
        await processor!.ProcessMessage(@event, context.Logger, geefOndernemingService, repository,
            CancellationToken.None);
        context.Logger.LogInformation($"{@event.Records.Count} RECORDS PROCESSED BY THE MESSAGE PROCESSOR");
    }

    
    private static async Task<MagdaOptionsSection> GetMagdaOptions(IConfiguration config,
        SsmClientWrapper ssmClient, 
        ParamNamesConfiguration? paramNamesConfiguration)
    {
        var magdaOptions = config.GetSection(MagdaOptionsSection.SectionName)
            .Get<MagdaOptionsSection>();

        if (magdaOptions is null)
            throw new ArgumentException("Could not load MagdaOptions");

        magdaOptions.ClientCertificate = await ssmClient.GetParameterAsync(paramNamesConfiguration.MagdaCertificate);
        magdaOptions.ClientCertificatePassword =
            await ssmClient.GetParameterAsync(paramNamesConfiguration.MagdaCertificatePassword);
        return magdaOptions;
    }

    private static async Task<DocumentStore> SetUpDocumentStore(IConfiguration config,
        ILambdaLogger contextLogger,
        SsmClientWrapper ssmClientWrapper,
        ParamNamesConfiguration paramNames)
    {
        var postgresSection =
            config.GetSection(PostgreSqlOptionsSection.SectionName)
                .Get<PostgreSqlOptionsSection>();

        if (!postgresSection.IsComplete)
            throw new ApplicationException("PostgresSqlOptions is missing some values");

        var opts = new StoreOptions();
        var connectionStringBuilder = new NpgsqlConnectionStringBuilder();
        connectionStringBuilder.Host = postgresSection.Host;
        connectionStringBuilder.Database = postgresSection.Database;
        connectionStringBuilder.Username = postgresSection.Username;
        connectionStringBuilder.Port = 5432;
        connectionStringBuilder.Password = await ssmClientWrapper.GetParameterAsync(paramNames.PostgresPassword);
        opts.Schema.For<MagdaCallReference>().Identity(x => x.Reference);

        var connectionString = connectionStringBuilder.ToString();
            
        contextLogger.LogInformation(connectionString);
            
        opts.Connection(connectionString);
        opts.Events.StreamIdentity = StreamIdentity.AsString;
        opts.Serializer(CreateCustomMartenSerializer());
        opts.Events.MetadataConfig.EnableAll();
        opts.AutoCreateSchemaObjects = AutoCreate.None;
        var store = new DocumentStore(opts);
        return store;
    }
    
    public static JsonNetSerializer CreateCustomMartenSerializer()
    {
        var jsonNetSerializer = new JsonNetSerializer();

        jsonNetSerializer.Customize(
            s =>
            {
                s.DateParseHandling = DateParseHandling.None;
                s.Converters.Add(new NullableDateOnlyJsonConvertor(WellknownFormats.DateOnly));
                s.Converters.Add(new DateOnlyJsonConvertor(WellknownFormats.DateOnly));
            });

        return jsonNetSerializer;
    }
}

[JsonSerializable(typeof(SQSEvent))]
public partial class LambdaFunctionJsonSerializerContext : JsonSerializerContext
{
}