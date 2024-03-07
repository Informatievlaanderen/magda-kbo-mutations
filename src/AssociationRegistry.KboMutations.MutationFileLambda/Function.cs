using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.Lambda.SQSEvents;
using Amazon.S3;
using Amazon.SimpleSystemsManagement;
using Amazon.SQS;
using AssocationRegistry.KboMutations;
using AssociationRegistry.KboMutations.Configuration;
using AssociationRegistry.KboMutations.Notifications;
using Microsoft.Extensions.Configuration;

namespace AssociationRegistry.KboMutations.MutationFileLambda;

public class Function
{
    private static MessageProcessor? _processor;

    private static async Task<int> Main()
    {
        var handler = FunctionHandler;
        await LambdaBootstrapBuilder.Create(handler, new SourceGeneratorLambdaJsonSerializer<LambdaFunctionJsonSerializerContext>())
            .Build()
            .RunAsync();

        return 0;
    }

    /// <summary>
    ///     This method is called for every Lambda invocation. This method takes in an SQS event object and can be used
    ///     to respond to SQS messages.
    /// </summary>
    /// <param name="event"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public static async Task FunctionHandler(SQSEvent @event, ILambdaContext context)
    {
        var configurationRoot = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", true, true)
            .AddEnvironmentVariables()
            .Build();
        
        var amazonKboSyncConfiguration = GetAmazonKboSyncConfiguration(configurationRoot);
        var ssmClientWrapper = new SsmClientWrapper(new AmazonSimpleSystemsManagementClient());
        var paramNamesConfiguration = GetParamNamesConfiguration(configurationRoot);
        var notifier = await new NotifierFactory(ssmClientWrapper, paramNamesConfiguration, context.Logger).TryCreate();
        var s3Client = new AmazonS3Client();
        var sqsClient = new AmazonSQSClient();

        _processor = new MessageProcessor(s3Client, sqsClient, notifier, amazonKboSyncConfiguration);

        await notifier.Notify(new KboMutationFileLambdaGestart(@event.Records.Count));
        await _processor!.ProcessMessage(@event, context.Logger, CancellationToken.None);
        await notifier.Notify(new KboMutationFileLambdaGestart(@event.Records.Count));
    }

    private static ParamNamesConfiguration GetParamNamesConfiguration(IConfigurationRoot configurationRoot)
    {
        var paramNamesConfiguration = configurationRoot
            .GetSection(ParamNamesConfiguration.Section)
            .Get<ParamNamesConfiguration>();

        if (paramNamesConfiguration is null)
            throw new ApplicationException("Could not load ParamNamesConfiguration");
        return paramNamesConfiguration;
    }

    private static AmazonKboSyncConfiguration GetAmazonKboSyncConfiguration(IConfigurationRoot configurationRoot)
    {
        var awsConfigurationSection = configurationRoot.GetSection("AWS");

        var amazonKboSyncConfiguration = new AmazonKboSyncConfiguration
        {
            MutationFileBucketName = awsConfigurationSection[nameof(WellKnownBucketNames.MutationFileBucketName)],
            MutationFileQueueUrl = awsConfigurationSection[nameof(WellKnownQueueNames.MutationFileQueueUrl)],
            SyncQueueUrl = awsConfigurationSection[nameof(WellKnownQueueNames.SyncQueueUrl)]!
        };

        if (string.IsNullOrWhiteSpace(amazonKboSyncConfiguration.SyncQueueUrl))
            throw new ArgumentException($"{nameof(amazonKboSyncConfiguration.SyncQueueUrl)} cannot be null or empty");
        
        if (string.IsNullOrWhiteSpace(amazonKboSyncConfiguration.MutationFileQueueUrl))
            throw new ArgumentException($"{nameof(amazonKboSyncConfiguration.MutationFileQueueUrl)} cannot be null or empty");
        
        if (string.IsNullOrWhiteSpace(amazonKboSyncConfiguration.MutationFileBucketName))
            throw new ArgumentException($"{nameof(amazonKboSyncConfiguration.MutationFileBucketName)} cannot be null or empty");
        
        return amazonKboSyncConfiguration;
    }
}

/// <summary>
/// This class is used to register the input event and return type for the FunctionHandler method with the System.Text.Json source generator.
/// There must be a JsonSerializable attribute for each type used as the input and return type or a runtime error will occur 
/// from the JSON serializer unable to find the serialization information for unknown types.
/// </summary>
[JsonSerializable(typeof(SQSEvent))]
public partial class LambdaFunctionJsonSerializerContext : JsonSerializerContext
{
    // By using this partial class derived from JsonSerializerContext, we can generate reflection free JSON Serializer code at compile time
    // which can deserialize our class and properties. However, we must attribute this class to tell it what types to generate serialization code for.
    // See https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-source-generation
}