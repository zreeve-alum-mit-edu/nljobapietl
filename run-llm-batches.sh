#!/bin/bash

echo "=== LLM Batch Processing Loop ==="
echo "This script will process all 93 workplace classification batches"
echo "- Max 2 batches in flight at once"
echo "- Will stop immediately on any errors or failed batches"
echo ""

cd JobApi.Etl

CYCLE=0
MAX_RETRIES=3

# Helper function to run psql commands with retry logic
retry_psql() {
  local sql_query="$1"
  local retry_count=0

  while [ $retry_count -lt $MAX_RETRIES ]; do
    local result=$(PGPASSWORD="mxofoyLVkiV2aQACxIbJ" psql -h nljobsearchapi.c74ioigi2bn4.us-east-2.rds.amazonaws.com -U JSadmin -d nljobsearch -t -c "$sql_query" 2>&1)
    local exit_code=$?

    if [ $exit_code -eq 0 ]; then
      echo "$result"
      return 0
    fi

    retry_count=$((retry_count + 1))
    if [ $retry_count -lt $MAX_RETRIES ]; then
      echo "  Database query failed (timeout/connection error). Retry $retry_count/$MAX_RETRIES..." >&2
      echo "  Waiting 2 minutes before retrying..." >&2
      sleep 120
    else
      echo "ERROR: Database query failed after $MAX_RETRIES attempts!" >&2
      echo "Query: $sql_query" >&2
      echo "Error: $result" >&2
      exit 1
    fi
  done
}

while true; do
  CYCLE=$((CYCLE + 1))
  echo ""
  echo "=========================================="
  echo "CYCLE $CYCLE - $(date '+%Y-%m-%d %H:%M:%S')"
  echo "=========================================="

  # Check for any failed batches
  echo ""
  echo "Checking for failed batches..."
  FAILED_COUNT=$(retry_psql "SELECT COUNT(*) FROM workplace_batches WHERE status = 'failed';")
  FAILED_COUNT=$(echo $FAILED_COUNT | xargs) # trim whitespace

  if [ "$FAILED_COUNT" -gt 0 ]; then
    echo "ERROR: Found $FAILED_COUNT failed batch(es) in database!"
    echo "Stopping process to prevent bad data from being processed."
    echo ""
    echo "Failed batches:"
    PGPASSWORD="mxofoyLVkiV2aQACxIbJ" psql -h nljobsearchapi.c74ioigi2bn4.us-east-2.rds.amazonaws.com -U JSadmin -d nljobsearch -c "SELECT id, batch_file_path, error_message FROM workplace_batches WHERE status = 'failed';"
    exit 1
  fi
  echo "No failed batches found. Continuing..."

  # Get current batch status counts
  echo ""
  echo "Current batch status:"
  PGPASSWORD="mxofoyLVkiV2aQACxIbJ" psql -h nljobsearchapi.c74ioigi2bn4.us-east-2.rds.amazonaws.com -U JSadmin -d nljobsearch -c "SELECT status, COUNT(*) FROM workplace_batches GROUP BY status ORDER BY status;"

  # Check if we're done
  PENDING_COUNT=$(retry_psql "SELECT COUNT(*) FROM workplace_batches WHERE status = 'pending';")
  PENDING_COUNT=$(echo $PENDING_COUNT | xargs)

  SUBMITTED_COUNT=$(retry_psql "SELECT COUNT(*) FROM workplace_batches WHERE status = 'submitted';")
  SUBMITTED_COUNT=$(echo $SUBMITTED_COUNT | xargs)

  if [ "$PENDING_COUNT" -eq 0 ] && [ "$SUBMITTED_COUNT" -eq 0 ]; then
    echo ""
    echo "=========================================="
    echo "ALL BATCHES COMPLETE!"
    echo "=========================================="
    echo "Final status:"
    PGPASSWORD="mxofoyLVkiV2aQACxIbJ" psql -h nljobsearchapi.c74ioigi2bn4.us-east-2.rds.amazonaws.com -U JSadmin -d nljobsearch -c "SELECT status, COUNT(*) FROM workplace_batches GROUP BY status ORDER BY status;"
    exit 0
  fi

  # Stage 3: Submit batches (up to 2 concurrent)
  echo ""
  echo ">>> Running Stage 3: LLM Batch Submit <<<"
  RETRY_COUNT=0
  MAX_RETRIES=3
  while [ $RETRY_COUNT -lt $MAX_RETRIES ]; do
    dotnet run llm-submit
    if [ $? -eq 0 ]; then
      break
    fi
    RETRY_COUNT=$((RETRY_COUNT + 1))
    if [ $RETRY_COUNT -lt $MAX_RETRIES ]; then
      echo "ERROR: Stage 3 (llm-submit) failed! Retry $RETRY_COUNT/$MAX_RETRIES"
      echo "Waiting 5 minutes before retrying..."
      sleep 300
    else
      echo "ERROR: Stage 3 (llm-submit) failed after $MAX_RETRIES attempts!"
      exit 1
    fi
  done

  echo ""
  echo "Waiting 60 seconds before checking batch status..."
  sleep 60

  # Stage 4: Check batch status and download completed results
  echo ""
  echo ">>> Running Stage 4: LLM Batch Check <<<"
  RETRY_COUNT=0
  while [ $RETRY_COUNT -lt $MAX_RETRIES ]; do
    dotnet run llm-check
    if [ $? -eq 0 ]; then
      break
    fi
    RETRY_COUNT=$((RETRY_COUNT + 1))
    if [ $RETRY_COUNT -lt $MAX_RETRIES ]; then
      echo "ERROR: Stage 4 (llm-check) failed! Retry $RETRY_COUNT/$MAX_RETRIES"
      echo "Waiting 5 minutes before retrying..."
      sleep 300
    else
      echo "ERROR: Stage 4 (llm-check) failed after $MAX_RETRIES attempts!"
      exit 1
    fi
  done

  # Stage 5: Process downloaded results and update database
  echo ""
  echo ">>> Running Stage 5: LLM Results Processing <<<"
  RETRY_COUNT=0
  while [ $RETRY_COUNT -lt $MAX_RETRIES ]; do
    dotnet run llm-results
    if [ $? -eq 0 ]; then
      break
    fi
    RETRY_COUNT=$((RETRY_COUNT + 1))
    if [ $RETRY_COUNT -lt $MAX_RETRIES ]; then
      echo "ERROR: Stage 5 (llm-results) failed! Retry $RETRY_COUNT/$MAX_RETRIES"
      echo "Waiting 5 minutes before retrying..."
      sleep 300
    else
      echo "ERROR: Stage 5 (llm-results) failed after $MAX_RETRIES attempts!"
      exit 1
    fi
  done

  echo ""
  echo "Waiting 10 seconds before next cycle..."
  sleep 10
done
