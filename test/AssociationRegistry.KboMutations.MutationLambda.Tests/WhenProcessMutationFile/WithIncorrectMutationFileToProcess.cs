using AssociationRegistry.KboMutations.MutationLambda.Tests.WhenProcessMutationFile.Fixtures;
using Xunit;

namespace AssociationRegistry.KboMutations.MutationLambda.Tests.WhenProcessMutationFile;

public class WithIncorrectMutationFileToProcess : IClassFixture<WithIncorrectMutationFileToProcessFixture>
{
    private readonly WithIncorrectMutationFileToProcessFixture _fixture;

    public WithIncorrectMutationFileToProcess(WithIncorrectMutationFileToProcessFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void It_Archived_MutationFile()
    {
    }
}