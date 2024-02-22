using System.Text.Json.Serialization;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.S3;
using Amazon.SQS;
using AssocationRegistry.KboMutations;
using AssociationRegistry.KboMutations.MutationLambda.Configuration;
using AssociationRegistry.KboMutations.MutationLambda.Ftps;
using Microsoft.Extensions.Configuration;

namespace AssociationRegistry.KboMutations.MutationLambda;

public static class Function
{
    private static async Task Main()
    {
        var handler = FunctionHandler;
        await LambdaBootstrapBuilder.Create(handler, new SourceGeneratorLambdaJsonSerializer<LambdaFunctionJsonSerializerContext>())
            .Build()
            .RunAsync();
    }

    private static async Task FunctionHandler(string input, ILambdaContext context) => await SharedFunctionHandler(context);

    public static async Task SharedFunctionHandler(ILambdaContext context)
    {
        var awsConfigurationSection = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", true, true)
            .AddEnvironmentVariables()
            .Build()
            .GetSection("AWS");

        var mutatieBestandProcessor = new MutatieBestandProcessor(
            context.Logger,
            new CurlFtpsClient(context.Logger, new()),
            new AmazonS3Client(),
            new AmazonSQSClient(),
            new KboMutationsConfiguration(),
            new AmazonKboSyncConfiguration()
            {
                MutationFileBucketUrl = awsConfigurationSection["MutationFileBucketName"],
                MutationFileQueueUrl = awsConfigurationSection["MutationFileQueueUrl"]!
            });

        context.Logger.LogInformation($"MUTATION FILE PROCESSOR STARTED");
        await mutatieBestandProcessor.ProcessAsync();
        context.Logger.LogInformation($"MUTATION FILE PROCESSOR COMPLETED");
    }
}

[JsonSerializable(typeof(string))]
public partial class LambdaFunctionJsonSerializerContext : JsonSerializerContext
{
}
