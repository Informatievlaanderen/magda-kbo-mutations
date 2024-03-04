using Amazon.Lambda.TestUtilities;
using Xunit;

namespace AssociationRegistry.KboMutations.SyncLambda.Tests;

public class FunctionTest
{
    [Fact]
    public void It_Should_Succeed()
    {
        var context = new TestLambdaContext();
        // var result = Function.FunctionHandler("", context);

        // Assert.False(result.IsFaulted);
    }
}