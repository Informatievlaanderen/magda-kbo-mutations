using AssociationRegistry.KboMutations.MutationLambda.Tests.WhenProcessMutationFile.Fixtures;
using Xunit;

namespace AssociationRegistry.KboMutations.MutationLambda.Tests.WhenProcessMutationFile;

public class WithEmptyMutationFileToProcess : IClassFixture<WithEmptyMutationFileToProcessFixture>
{
    private readonly WithEmptyMutationFileToProcessFixture _fixture;

    public WithEmptyMutationFileToProcess(WithEmptyMutationFileToProcessFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void It_Fetched_MutationFile()
    {
    }

    [Fact]
    public void It_Did_Not_Persist_MutationFile()
    {
    }

    [Fact]
    public void It_Archived_MutationFile()
    {
    }
}