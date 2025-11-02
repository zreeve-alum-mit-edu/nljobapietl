#!/bin/bash
set -e

# Configuration
REGION="us-east-2"
FUNCTION_NAME="NLJobSearch"
ROLE_NAME="NLJobSearchRole"
API_NAME="nl-job-search-api"
HANDLER="JobApi.Lambda::JobApi.Lambda.Handlers.ApiGatewayHandler::HandleRequest"

echo "======================================"
echo "Deploying Job Search Lambda API"
echo "======================================"

# Step 1: Build and publish the Lambda
echo ""
echo "Step 1: Building Lambda function..."
cd JobApi.Lambda
dotnet publish -c Release -o publish

# Step 2: Create deployment package
echo ""
echo "Step 2: Creating deployment package..."
cd publish
zip -r ../lambda-deployment.zip .
cd ..

# Step 3: Check if IAM role exists, create if not
echo ""
echo "Step 3: Checking IAM role..."
if aws iam get-role --role-name $ROLE_NAME --region $REGION 2>/dev/null; then
    echo "IAM role $ROLE_NAME already exists"
    ROLE_ARN=$(aws iam get-role --role-name $ROLE_NAME --query 'Role.Arn' --output text --region $REGION)
else
    echo "Creating IAM role $ROLE_NAME..."

    # Create trust policy
    cat > /tmp/trust-policy.json <<EOF
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Principal": {
        "Service": "lambda.amazonaws.com"
      },
      "Action": "sts:AssumeRole"
    }
  ]
}
EOF

    # Create role
    ROLE_ARN=$(aws iam create-role \
        --role-name $ROLE_NAME \
        --assume-role-policy-document file:///tmp/trust-policy.json \
        --query 'Role.Arn' \
        --output text \
        --region $REGION)

    echo "Created role: $ROLE_ARN"

    # Attach basic Lambda execution policy
    aws iam attach-role-policy \
        --role-name $ROLE_NAME \
        --policy-arn arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole \
        --region $REGION

    echo "Waiting 10 seconds for IAM role to propagate..."
    sleep 10
fi

# Step 4: Deploy/Update Lambda function
echo ""
echo "Step 4: Deploying Lambda function..."
if aws lambda get-function --function-name $FUNCTION_NAME --region $REGION 2>/dev/null; then
    echo "Updating existing Lambda function..."
    aws lambda update-function-code \
        --function-name $FUNCTION_NAME \
        --zip-file fileb://lambda-deployment.zip \
        --region $REGION

    echo "Updating function configuration..."
    aws lambda update-function-configuration \
        --function-name $FUNCTION_NAME \
        --timeout 30 \
        --memory-size 512 \
        --environment "Variables={
            DB_HOST=${DB_HOST},
            DB_USER=${DB_USER},
            DB_PASSWORD=${DB_PASSWORD},
            DB_PORT=${DB_PORT:-5432},
            DB_NAME=${DB_NAME:-postgres},
            OPENAI_API_KEY=${OPENAI_API_KEY},
            HNSW_EF_SEARCH=${HNSW_EF_SEARCH:-500}
        }" \
        --region $REGION
else
    echo "Creating new Lambda function..."

    # Load environment variables from .env file
    if [ -f "../.env" ]; then
        export $(cat ../.env | grep -v '^#' | xargs)
    fi

    aws lambda create-function \
        --function-name $FUNCTION_NAME \
        --runtime dotnet8 \
        --role $ROLE_ARN \
        --handler $HANDLER \
        --zip-file fileb://lambda-deployment.zip \
        --timeout 30 \
        --memory-size 512 \
        --environment "Variables={
            DB_HOST=${DB_HOST},
            DB_USER=${DB_USER},
            DB_PASSWORD=${DB_PASSWORD},
            DB_PORT=${DB_PORT:-5432},
            DB_NAME=${DB_NAME:-postgres},
            OPENAI_API_KEY=${OPENAI_API_KEY},
            HNSW_EF_SEARCH=${HNSW_EF_SEARCH:-500}
        }" \
        --region $REGION
fi

# Get Lambda ARN
LAMBDA_ARN=$(aws lambda get-function --function-name $FUNCTION_NAME --query 'Configuration.FunctionArn' --output text --region $REGION)
echo "Lambda ARN: $LAMBDA_ARN"

# Step 5: Create HTTP API Gateway (if it doesn't exist)
echo ""
echo "Step 5: Setting up API Gateway..."
API_ID=$(aws apigatewayv2 get-apis --query "Items[?Name=='$API_NAME'].ApiId" --output text --region $REGION)

if [ -z "$API_ID" ]; then
    echo "Creating HTTP API Gateway..."
    API_ID=$(aws apigatewayv2 create-api \
        --name $API_NAME \
        --protocol-type HTTP \
        --target $LAMBDA_ARN \
        --query 'ApiId' \
        --output text \
        --region $REGION)
    echo "Created API with ID: $API_ID"
else
    echo "API Gateway already exists with ID: $API_ID"
fi

# Step 6: Create integration
echo ""
echo "Step 6: Creating integration..."
INTEGRATION_ID=$(aws apigatewayv2 get-integrations --api-id $API_ID --query "Items[0].IntegrationId" --output text --region $REGION)

if [ "$INTEGRATION_ID" == "None" ] || [ -z "$INTEGRATION_ID" ]; then
    INTEGRATION_ID=$(aws apigatewayv2 create-integration \
        --api-id $API_ID \
        --integration-type AWS_PROXY \
        --integration-uri $LAMBDA_ARN \
        --payload-format-version 2.0 \
        --query 'IntegrationId' \
        --output text \
        --region $REGION)
    echo "Created integration: $INTEGRATION_ID"
else
    echo "Integration already exists: $INTEGRATION_ID"
fi

# Step 7: Create route
echo ""
echo "Step 7: Creating route..."
ROUTE_ID=$(aws apigatewayv2 get-routes --api-id $API_ID --query "Items[?RouteKey=='POST /search'].RouteId" --output text --region $REGION)

if [ -z "$ROUTE_ID" ]; then
    aws apigatewayv2 create-route \
        --api-id $API_ID \
        --route-key "POST /search" \
        --target "integrations/$INTEGRATION_ID" \
        --region $REGION
    echo "Created route: POST /search"
else
    echo "Route already exists"
fi

# Step 8: Grant API Gateway permission to invoke Lambda
echo ""
echo "Step 8: Granting API Gateway invoke permission..."
ACCOUNT_ID=$(aws sts get-caller-identity --query Account --output text)
aws lambda add-permission \
    --function-name $FUNCTION_NAME \
    --statement-id apigateway-invoke-$(date +%s) \
    --action lambda:InvokeFunction \
    --principal apigateway.amazonaws.com \
    --source-arn "arn:aws:execute-api:$REGION:$ACCOUNT_ID:$API_ID/*/*" \
    --region $REGION 2>/dev/null || echo "Permission may already exist"

# Step 9: Get API endpoint
echo ""
echo "======================================"
echo "Deployment Complete!"
echo "======================================"
API_ENDPOINT=$(aws apigatewayv2 get-apis --query "Items[?Name=='$API_NAME'].ApiEndpoint" --output text --region $REGION)
echo ""
echo "API Endpoint: ${API_ENDPOINT}/search"
echo ""
echo "Test with:"
echo "curl -X POST ${API_ENDPOINT}/search \\"
echo "  -H 'Content-Type: application/json' \\"
echo "  -d '{\"prompt\":\"software engineer\",\"filters\":[{\"workplaceType\":\"remote\"}],\"limit\":5}'"
echo ""

# Cleanup
rm -f /tmp/trust-policy.json
