#!/bin/bash

##awslocal lambda delete-function --function-name mutationfile-lambda
#awslocal lambda create-function \
#    --function-name mutationfile-lambda \
#    --zip-file fileb:///etc/localstack/lambda/mutationfile.zip \
#    --handler AssociationRegistry.KboMutations.MutationFileLambda \
#    --role arn:aws:iam::000000000000:role/irrelevant-role \
#    --runtime provided.al2 \
#    --package-type zip \
#    --timeout 10 \
#    --memory-size 128 \
#    --region us-east-1 
#
#awslocal lambda create-event-source-mapping \
#    --function-name mutationfile-lambda \
#    --event-source-arn arn:aws:sqs:us-east-1:000000000000:verenigingsregister-kbomutations-mutationfile \
#    --region us-east-1 \
#    --batch-size 1
#
