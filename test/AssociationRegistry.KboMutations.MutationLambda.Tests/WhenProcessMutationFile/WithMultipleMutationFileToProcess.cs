using AssociationRegistry.KboMutations.MutationLambda.Tests.WhenProcessMutationFile.Fixtures;
using Xunit;

namespace AssociationRegistry.KboMutations.MutationLambda.Tests.WhenProcessMutationFile;

public class WithMultipleMutationFileToProcess : IClassFixture<WithMultipleMutationFileToProcessFixture>
{
    private readonly WithMultipleMutationFileToProcessFixture _fixture;

    public WithMultipleMutationFileToProcess(WithMultipleMutationFileToProcessFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void It_Fetched_MutationFile_Collection()
    {
    }

    [Fact]
    public void It_Persisted_MutationFile_Collection()
    {
    }

    [Fact]
    public void It_Archived_MutationFile_Collection()
    {
    }
}