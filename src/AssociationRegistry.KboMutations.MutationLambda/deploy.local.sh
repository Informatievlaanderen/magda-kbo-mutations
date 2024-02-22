#!/bin/sh
dotnet lambda package --region us-east-1
mv ./bin/Release/net7.0/AssociationRegistry.KboMutations.MutationLambda.zip ../../.localstack/lambda/mutation.zip

# awslocal lambda delete-function \
#   --function-name mutation-lambda \
#   --region us-east-1

# awslocal lambda create-function \
#   --function-name mutation-lambda \
#   --runtime provided.al2 \
#   --role arn:aws:iam::000000000000:role/dummy-role \
#   --handler AssociationRegistry.KboMutations.MutationLambda \
#   --zip-file fileb://bin/Release/net6.0/AssociationRegistry.KboMutations.MutationLambda.zip \
#   --region us-east-1
