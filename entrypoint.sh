#!/bin/bash

# Set variables
LOCALSTACK_ENDPOINT="http://localhost:4566"
PATH=$PATH:/root/.dotnet/tools
REGION="us-east-1"
ROLE_ARN="arn:aws:iam::000000000000:role/dummy-role"
BATCH_SIZE=1

# Install Lambda tools
dotnet tool install -g Amazon.Lambda.Tools

UpdateLocalstackLambda() {
  # Navigate into correct folder
  pushd $HANDLER
  echo "Package Lambda function: $FUNCTION_NAME" 
  dotnet lambda package --region $REGION
  popd

  # Delete the Lambda function if it exists
  if aws lambda get-function --endpoint-url=$LOCALSTACK_ENDPOINT --function-name $FUNCTION_NAME --region $REGION; then
      echo "Deleting existing Lambda function: $FUNCTION_NAME"
      aws lambda delete-function --endpoint-url=$LOCALSTACK_ENDPOINT --function-name $FUNCTION_NAME --region $REGION
  fi
  
  # Create the Lambda function
  echo "Creating Lambda function: $FUNCTION_NAME"
  aws lambda create-function \
      --endpoint-url=$LOCALSTACK_ENDPOINT \
      --function-name $FUNCTION_NAME \
      --runtime provided.al2 \
      --role $ROLE_ARN \
      --environment "Variables={POSTGRESQLOPTIONS__HOST=db,POSTGRESQLOPTIONS__USERNAME=root,POSTGRESQLOPTIONS__PASSWORD=root,POSTGRESQLOPTIONS__DATABASE=verenigingsregister}" \
      --handler $HANDLER \
      --zip-file $ZIP_FILE_PATH \
      --timeout 900 --memory-size 1024 \
      --region $REGION --no-paginate
  
  # Attempt to find and delete the existing event source mapping for the Lambda function
  EVENT_SOURCE_MAPPING_UUID=$(aws lambda list-event-source-mappings --endpoint-url=$LOCALSTACK_ENDPOINT --function-name $FUNCTION_NAME --region $REGION | jq -r '.EventSourceMappings[] | select(.EventSourceArn=="'"$EVENT_SOURCE_ARN"'") | .UUID')
  if [ ! -z "$EVENT_SOURCE_MAPPING_UUID" ]; then
      echo "Deleting existing event source mapping: $EVENT_SOURCE_MAPPING_UUID"
      aws lambda delete-event-source-mapping --endpoint-url=$LOCALSTACK_ENDPOINT --uuid $EVENT_SOURCE_MAPPING_UUID --region $REGION
  fi
  
  # Create a new event source mapping for the Lambda function
  echo "Creating event source mapping for Lambda function: $FUNCTION_NAME"
  aws lambda create-event-source-mapping \
      --endpoint-url=$LOCALSTACK_ENDPOINT \
      --function-name $FUNCTION_NAME \
      --event-source-arn $EVENT_SOURCE_ARN \
      --region $REGION \
      --batch-size $BATCH_SIZE --no-paginate
}

# Set variables
FUNCTION_NAME="mutationfile-lambda"
HANDLER="AssociationRegistry.KboMutations.MutationFileLambda"
ZIP_FILE_PATH="fileb://AssociationRegistry.KboMutations.MutationFileLambda.zip"
EVENT_SOURCE_ARN="arn:aws:sqs:us-east-1:000000000000:verenigingsregister-kbomutations-file"

# Update LocalStack
UpdateLocalstackLambda

# Set variables
FUNCTION_NAME="sync-lambda"
HANDLER="AssociationRegistry.KboMutations.SyncLambda"
ZIP_FILE_PATH="fileb://AssociationRegistry.KboMutations.SyncLambda.zip"
EVENT_SOURCE_ARN="arn:aws:sqs:us-east-1:000000000000:verenigingsregister-kbomutations-sync"

# Update LocalStack
UpdateLocalstackLambda

echo "Setup complete."

