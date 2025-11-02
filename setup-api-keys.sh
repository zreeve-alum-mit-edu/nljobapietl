#!/bin/bash
set -e

REGION="us-east-2"
API_ID="42r7s00kck"
API_NAME="nl-job-search-api"

echo "Setting up API key authentication..."

# Step 1: Update the route to require API key
echo "Step 1: Enabling API key requirement on route..."
ROUTE_ID=$(aws apigatewayv2 get-routes --api-id $API_ID --query "Items[?RouteKey=='POST /search'].RouteId" --output text --region $REGION)

aws apigatewayv2 update-route \
  --api-id $API_ID \
  --route-id $ROUTE_ID \
  --api-key-required \
  --region $REGION > /dev/null

echo "✓ Route now requires API key"

# Step 2: Create usage plan
echo ""
echo "Step 2: Creating usage plan..."
USAGE_PLAN_ID=$(aws apigateway create-usage-plan \
  --name "${API_NAME}-usage-plan" \
  --description "Usage plan for NL Job Search API" \
  --throttle burstLimit=100,rateLimit=50 \
  --quota limit=100000,period=MONTH \
  --region $REGION \
  --query 'id' \
  --output text)

echo "✓ Created usage plan: $USAGE_PLAN_ID"

# Step 3: Associate API with usage plan
echo ""
echo "Step 3: Associating API with usage plan..."
aws apigateway create-usage-plan-key \
  --usage-plan-id $USAGE_PLAN_ID \
  --key-type API_KEY \
  --key-id dummy \
  --region $REGION 2>/dev/null || true

# Actually we need to create API keys first, then associate them
# Let me create the keys

echo ""
echo "Step 4: Creating 5 API keys..."
echo ""
echo "=========================================="
echo "API KEYS (Save these securely!)"
echo "=========================================="

for i in {1..5}; do
  KEY_NAME="${API_NAME}-key-${i}"
  
  API_KEY_DATA=$(aws apigateway create-api-key \
    --name "$KEY_NAME" \
    --enabled \
    --region $REGION)
  
  KEY_ID=$(echo $API_KEY_DATA | jq -r '.id')
  KEY_VALUE=$(echo $API_KEY_DATA | jq -r '.value')
  
  # Associate key with usage plan
  aws apigateway create-usage-plan-key \
    --usage-plan-id $USAGE_PLAN_ID \
    --key-type API_KEY \
    --key-id $KEY_ID \
    --region $REGION > /dev/null
  
  echo ""
  echo "Key $i:"
  echo "  Name:  $KEY_NAME"
  echo "  Value: $KEY_VALUE"
done

echo ""
echo "=========================================="
echo "Setup Complete!"
echo "=========================================="
echo ""
echo "Test with:"
echo "curl -X POST https://${API_ID}.execute-api.${REGION}.amazonaws.com/search \\"
echo "  -H 'Content-Type: application/json' \\"
echo "  -H 'x-api-key: YOUR_KEY_HERE' \\"
echo "  -d '{\"prompt\":\"software engineer\",\"filters\":[{\"workplaceType\":\"remote\"}],\"limit\":3}'"
echo ""

