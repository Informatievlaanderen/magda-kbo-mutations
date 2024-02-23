using System.Text.Json.Serialization;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.Lambda.SQSEvents;
using AssociationRegistry.EventStore;
using Marten;
using Microsoft.Extensions.Configuration;

namespace AssociationRegistry.KboMutations.SyncLambda;

public class Function
{
    internal static IConfiguration Configuration;
    internal static MessageProcessor MessageProcessor;

    private static async Task Main()
    {
        Configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", true, true)
            .AddEnvironmentVariables()
            .Build();

        MessageProcessor = ConfigureMessageProcessor();

        var handler = FunctionHandler;
        await LambdaBootstrapBuilder.Create(handler, new SourceGeneratorLambdaJsonSerializer<LambdaFunctionJsonSerializerContext>())
            .Build()
            .RunAsync();
    }

    private static MessageProcessor ConfigureMessageProcessor()
    {
        var awsConfigurationSection = Configuration
            .GetSection("AWS");

        var magdaGeefVerenigingService = new MagdaGeefVerenigingService();
        
        var verenigingsRepository = new VerenigingsRepository(
            new EventStore.EventStore(
                new DocumentStore(
                    new StoreOptions
                    {
                        
                    })));

        return new MessageProcessor(magdaGeefVerenigingService, verenigingsRepository);
    }

    /// <summary>
    ///     This method is called for every Lambda invocation. This method takes in an SQS event object and can be used
    ///     to respond to SQS messages.
    /// </summary>
    /// <param name="event"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    private static async Task FunctionHandler(SQSEvent @event, ILambdaContext context)
    {
        context.Logger.LogInformation($"{@event.Records.Count} RECORDS RECEIVED INSIDE SQS EVENT");
        await MessageProcessor!.ProcessMessage(@event, context.Logger, CancellationToken.None);
        context.Logger.LogInformation($"{@event.Records.Count} RECORDS PROCESSED BY THE MESSAGE PROCESSOR");
    }
}

/// <summary>
/// This class is used to register the input event and return type for the FunctionHandler method with the System.Text.Json source generator.
/// There must be a JsonSerializable attribute for each type used as the input and return type or a runtime error will occur 
/// from the JSON serializer unable to find the serialization information for unknown types.
/// </summary>
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(SQSEvent))]
public partial class LambdaFunctionJsonSerializerContext : JsonSerializerContext
{
    // By using this partial class derived from JsonSerializerContext, we can generate reflection free JSON Serializer code at compile time
    // which can deserialize our class and properties. However, we must attribute this class to tell it what types to generate serialization code for.
    // See https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-source-generation
}