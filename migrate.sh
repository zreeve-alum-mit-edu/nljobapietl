#!/bin/bash
set -e

echo "=== Running Database Migrations ==="
echo ""

# Load environment variables
if [ -f .env ]; then
    echo "Loading environment variables from .env..."
    export $(cat .env | grep -v '^#' | xargs)
else
    echo "Warning: .env file not found"
fi

# Run migrations
echo "Applying migrations to database..."
cd JobApi.Etl
dotnet ef database update --project ../JobApi.Common/JobApi.Common.csproj --startup-project JobApi.Etl.csproj

echo ""
echo "=== Migrations Complete ==="
