using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;
using Amazon.S3;
using Amazon.SQS;
using AssocationRegistry.KboMutations;
using AssociationRegistry.KboMutations.MutationLambda.Configuration;
using AssociationRegistry.KboMutations.MutationLambda.Ftps;
using AssociationRegistry.KboMutations.Tests.Fakers;
using AssociationRegistry.KboMutations.Tests.Fixtures;
using Moq;

namespace AssociationRegistry.KboMutations.MutationLambda.Tests.WhenProcessMutationFile.Fixtures;

public abstract class WithMutationFileToProcessFixture : WithLocalstackFixture
{
    private Mock<IFtpsClient> _secureFtpClientMock;
    private Mock<IAmazonS3> _s3ClientMock;
    private Mock<IAmazonSQS> _sqsClientMock;
    public ILambdaLogger LambdaLogger { get; }
    public KboMutationsConfiguration KboMutationsConfiguration { get; }
    public AmazonKboSyncConfiguration KboSyncConfiguration { get; }
    
    protected override Task SetupAsync()
    {
        _secureFtpClientMock = SetupSecureFtpClientMock(new Mock<IFtpsClient>());
        _s3ClientMock = SetupS3ClientMock(new Mock<IAmazonS3>());
        _sqsClientMock = SetupSqsClientMock(new Mock<IAmazonSQS>());
        
        Processor = new MutatieBestandProcessor(
            LambdaLogger,
            ConfigureSecureFtpClient(),
            ConfigureS3Client(),
            ConfigureSqsClient(),
            KboMutationsConfiguration,
            KboSyncConfiguration);
        return Task.CompletedTask;
    }

    public MutatieBestandProcessor Processor { get; set; }
    
    protected virtual IFtpsClient ConfigureSecureFtpClient() => _secureFtpClientMock.Object;
    protected virtual IAmazonS3 ConfigureS3Client() => _s3ClientMock.Object;
    protected virtual IAmazonSQS ConfigureSqsClient() => _sqsClientMock.Object;
    protected virtual Mock<IFtpsClient> SetupSecureFtpClientMock(Mock<IFtpsClient> mock) => mock;
    protected virtual Mock<IAmazonS3> SetupS3ClientMock(Mock<IAmazonS3> mock) => mock;
    protected virtual Mock<IAmazonSQS> SetupSqsClientMock(Mock<IAmazonSQS> mock) => mock;

}