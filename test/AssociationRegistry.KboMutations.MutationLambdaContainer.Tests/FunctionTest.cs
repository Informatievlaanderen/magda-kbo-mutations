using Xunit;
using Amazon.Lambda.TestUtilities;

namespace AssociationRegistry.KboMutations.MutationLambdaContainer.Tests;

public class FunctionTest
{
    [Fact]
    public void TestToUpperFunction()
    {

        // Invoke the lambda function and confirm the string was upper cased.
        var context = new TestLambdaContext();
        var casing = Function.SharedFunctionHandler(context);

        // Assert.Equal("hello world", casing.Lower);
        // Assert.Equal("HELLO WORLD", casing.Upper);
    }
}