using System.Text.Json.Serialization;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.S3;
using Amazon.SimpleSystemsManagement;
using Amazon.SQS;
using AssocationRegistry.KboMutations;
using AssocationRegistry.KboMutations.Notifications;
using AssociationRegistry.KboMutations.Configuration;
using AssociationRegistry.KboMutations.MutationLambdaContainer.Configuration;
using AssociationRegistry.KboMutations.MutationLambdaContainer.Ftps;
using AssociationRegistry.KboMutations.Notifications;
using Microsoft.Extensions.Configuration;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace AssociationRegistry.KboMutations.MutationLambdaContainer;

public static class Function
{
    private static async Task Main()
    {
        var handler = FunctionHandler;
        await LambdaBootstrapBuilder.Create(handler,
                new SourceGeneratorLambdaJsonSerializer<LambdaFunctionJsonSerializerContext>())
            .Build()
            .RunAsync();
    }

    private static async Task FunctionHandler(string input, ILambdaContext context)
    {
        await SharedFunctionHandler(context);
    }

    public static async Task SharedFunctionHandler(ILambdaContext context)
    {
        var configurationRoot = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", true, true)
            .AddEnvironmentVariables()
            .Build();

        var ssmClientWrapper = new SsmClientWrapper(new AmazonSimpleSystemsManagementClient());
        var paramNamesConfiguration = GetParamNamesConfiguration(configurationRoot);

        var notifier = await new NotifierFactory(ssmClientWrapper, paramNamesConfiguration, context.Logger)
            .TryCreate();

        try
        {
            await notifier.Notify(new KboMutationLambdaGestart());
            var mutatieBestandProcessor = await SetUpFunction(
                context,
                GetKboMutationsConfiguration(configurationRoot),
                configurationRoot.GetSection("AWS"),
                notifier);

            await mutatieBestandProcessor.ProcessAsync();
            await notifier.Notify(new KboMutationLambdaVoltooid());
        }
        catch (Exception ex)
        {
            await notifier.Notify(new KboMutationLambdaGefaald(ex));
            throw;
        }
    }

    private static async Task<MutatieBestandProcessor> SetUpFunction(
        ILambdaContext context,
        KboMutationsConfiguration kboMutationsConfiguration,
        IConfiguration awsConfigurationSection,
        INotifier notifier)
    {
        var certProvider = new CertificatesProvider(kboMutationsConfiguration);

        var amazonS3Client = new AmazonS3Client();
        await certProvider.WriteCertificatesToFileSystem(context.Logger, amazonS3Client);

        var mutatieBestandProcessor = new MutatieBestandProcessor(
            context.Logger,
            new CurlFtpsClient(context.Logger, kboMutationsConfiguration),
            amazonS3Client,
            new AmazonSQSClient(),
            kboMutationsConfiguration,
            new AmazonKboSyncConfiguration
            {
                MutationFileBucketName = awsConfigurationSection["MutationFileBucketName"],
                MutationFileQueueUrl = awsConfigurationSection["MutationFileQueueUrl"]!
            },
            notifier);

        return mutatieBestandProcessor;
    }

    private static KboMutationsConfiguration GetKboMutationsConfiguration(IConfigurationRoot configurationRoot)
    {
        var kboMutationsConfiguration = configurationRoot
            .GetSection(KboMutationsConfiguration.Section)
            .Get<KboMutationsConfiguration>();

        if (kboMutationsConfiguration is null)
            throw new ApplicationException("Could not load KboMutationsConfiguration");
        return kboMutationsConfiguration;
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
}

[JsonSerializable(typeof(string))]
public partial class LambdaFunctionJsonSerializerContext : JsonSerializerContext
{
}