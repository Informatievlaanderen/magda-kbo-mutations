using AssociationRegistry.KboMutations.MutationLambda.Tests.WhenProcessMutationFile.Fixtures;
using Xunit;

namespace AssociationRegistry.KboMutations.MutationLambda.Tests.WhenProcessMutationFile;

public class WithSingleMutationFileToProcess : IClassFixture<WithSingleMutationFileToProcessFixture>
{
    private readonly WithSingleMutationFileToProcessFixture _fixture;

    public WithSingleMutationFileToProcess(WithSingleMutationFileToProcessFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void It_Fetched_MutationFile()
    {
    }

    [Fact]
    public void It_Persisted_MutationFile()
    {
    }

    [Fact]
    public void It_Archived_MutationFile()
    {
    }
}