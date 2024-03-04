using System.Text.Json.Serialization;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.S3;
using Amazon.SimpleSystemsManagement;
using Amazon.SQS;
using AssocationRegistry.KboMutations;
using AssociationRegistry.KboMutations.MutationLambdaContainer.Configuration;
using AssociationRegistry.KboMutations.MutationLambdaContainer.Ftps;
using Microsoft.Extensions.Configuration;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace AssociationRegistry.KboMutations.MutationLambdaContainer;

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
        var configurationRoot = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", true, true)
            .AddEnvironmentVariables()
            .Build();

        var awsConfigurationSection = configurationRoot
            .GetSection("AWS");

        var kboMutationsConfiguration = configurationRoot
            .GetSection(KboMutationsConfiguration.Section)
            .Get<KboMutationsConfiguration>();

        var certProvider = new CertificatesProvider(
            new AmazonSimpleSystemsManagementClient(),
            configurationRoot
                .GetSection(ParamNamesConfiguration.Section)
                .Get<ParamNamesConfiguration>(),
            kboMutationsConfiguration);
        
        await certProvider.WriteCertificatesToFileSystem();

        var mutatieBestandProcessor = new MutatieBestandProcessor(
            context.Logger,
            new CurlFtpsClient(context.Logger, kboMutationsConfiguration),
            new AmazonS3Client(),
            new AmazonSQSClient(),
            kboMutationsConfiguration,
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
