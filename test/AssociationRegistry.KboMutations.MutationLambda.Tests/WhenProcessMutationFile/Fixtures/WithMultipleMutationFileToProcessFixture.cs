namespace AssociationRegistry.KboMutations.MutationLambda.Tests.WhenProcessMutationFile.Fixtures;

public class WithMultipleMutationFileToProcessFixture : WithMutationFileToProcessFixture
{
    // protected override Mock<IKboMutationsFetcher> SetupFetcherMock(Mock<IKboMutationsFetcher> mock)
    // {
    //     var mutationFileFaker = new MutatieBestandFaker();
    //     mock
    //         .Setup(m => m.GetFiles())
    //         .Returns(new[] { mutationFileFaker.GenerateWithLineCount(3) });
    //     return mock;
    // }
}