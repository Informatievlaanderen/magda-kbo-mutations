#!/bin/sh
dotnet lambda package --region us-east-1
mv ./bin/Release/net7.0/AssociationRegistry.KboMutations.SyncLambda.zip ../../.localstack/lambda/sync.zip

# awslocal lambda delete-function \
#   --function-name sync-lambda \
#   --region us-east-1

# awslocal lambda create-function \
#   --function-name sync-lambda \
#   --runtime provided.al2 \
#   --role arn:aws:iam::000000000000:role/dummy-role \
#   --handler AssociationRegistry.KboMutations.SyncLambda \
#   --zip-file fileb://bin/Release/net6.0/AssociationRegistry.KboMutations.SyncLambda.zip \
#   --region us-east-1
  
