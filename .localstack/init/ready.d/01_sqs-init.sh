#!/bin/sh

# Set variables
REGION="us-east-1"
ACCOUNT_ID="000000000000" # Default account ID used by LocalStack
MAX_RECEIVE_COUNT=3

# DLQ names
DLQ1_NAME="verenigingsregister-kbomutations-mutationfile-dlq"
DLQ2_NAME="verenigingsregister-kbomutations-sync-dlq"

# Construct DLQ ARNs
DLQ1_ARN="arn:aws:sqs:$REGION:$ACCOUNT_ID:$DLQ1_NAME"
DLQ2_ARN="arn:aws:sqs:$REGION:$ACCOUNT_ID:$DLQ2_NAME"

# Create DLQs
awslocal sqs create-queue --region $REGION --queue-name $DLQ1_NAME
awslocal sqs create-queue --region $REGION --queue-name $DLQ2_NAME

awslocal sqs create-queue --region $REGION --queue-name verenigingsregister-kbomutations-mutationfile --attribute file:///etc/localstack/init/ready.d/01_sqs-init.redrive-mutation-file.json
awslocal sqs create-queue --region $REGION --queue-name verenigingsregister-kbomutations-sync --attribute file:///etc/localstack/init/ready.d/01_sqs-init.redrive-sync.json
