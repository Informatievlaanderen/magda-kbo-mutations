#!/bin/bash

#awslocal lambda delete-function --function-name sync-lambda
#awslocal lambda create-function \
#    --function-name sync-lambda \
#    --zip-file fileb:///etc/localstack/lambda/sync.zip \
#    --handler AssociationRegistry.KboMutations.SyncLambda::AssociationRegistry.KboMutations.SyncLambda.Function::FunctionHandler \
#    --role arn:aws:iam::000000000000:role/irrelevant-role \
#    --runtime dotnet6 \
#    --package-type zip \
#    --timeout 10 \
#    --memory-size 128 \
#    --region us-east-1 
#    
#awslocal lambda create-event-source-mapping \
#    --function-name sync-lambda \
#    --event-source-arn arn:aws:sqs:us-east-1:000000000000:verenigingsregister-kbomutations-sync \
#    --region us-east-1 \
#    --batch-size 1

