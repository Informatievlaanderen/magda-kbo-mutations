# Kbo Mutations File Lambda

Triggered by items on the kbo mutations file queue. 
Splits the files into lines, and places them on the kbo sync queue to be processed by kbo sync lambda.

## Main dependencies
- kbo mutations file bucket
- kbo mutations file queue
- kbo sync queue

## Here are some steps to follow to get started from the command line:

Once you have edited your template and code you can deploy your application using
the [Amazon.Lambda.Tools Global Tool](https://github.com/aws/aws-extensions-for-dotnet-cli#aws-lambda-amazonlambdatools)
from the command line.

Install Amazon.Lambda.Tools Global Tools if not already installed.

```
    dotnet tool install -g Amazon.Lambda.Tools
```

If already installed check if new version is available.

```
    dotnet tool update -g Amazon.Lambda.Tools
```

Execute unit tests

```
    cd "AssociationRegistry.KboMutations.SyncLambda/test/AssociationRegistry.KboMutations.SyncLambda.Tests"
    dotnet test
```

Deploy function to AWS Lambda

```
    cd "AssociationRegistry.KboMutations.SyncLambda/src/AssociationRegistry.KboMutations.SyncLambda"
    dotnet lambda deploy-function
```
