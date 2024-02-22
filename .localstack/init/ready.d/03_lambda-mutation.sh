#!/bin/bash
#
#awslocal lambda delete-function --function-name mutation-lambda
#awslocal lambda create-function \
#    --function-name mutation-lambda \
#    --zip-file fileb:///etc/localstack/lambda/mutation.zip \
#    --handler AssociationRegistry.KboMutations.MutationLambda::AssociationRegistry.KboMutations.MutationLambda.Function::FunctionHandler \
#    --role arn:aws:iam::000000000000:role/irrelevant-role \
#    --runtime dotnet6 \
#    --package-type zip \
#    --timeout 10 \
#    --memory-size 128 \
#    --region us-east-1 
#    
