using Amazon.Lambda.Core;
using AssociationRegistry.KboMutations.MutationLambda;
using AssociationRegistry.KboMutations.MutationLambda.Configuration;
using AssociationRegistry.KboMutations.MutationLambda.Ftps;

namespace AssociationRegistry.KboMutations.MutationLambdaContainer;

public class Function
{
    public async Task FunctionHandler(string input, ILambdaContext context) => await MutationLambda.Function.SharedFunctionHandler(context);
}
