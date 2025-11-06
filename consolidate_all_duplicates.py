#!/usr/bin/env python3
import psycopg2
import subprocess
import sys
from datetime import datetime

# Database connection details
DB_HOST = "nljobsearchapi.c74ioigi2bn4.us-east-2.rds.amazonaws.com"
DB_NAME = "nljobsearch"
DB_USER = "JSadmin"
DB_PASSWORD = "mxofoyLVkiV2aQACxIbJ"
DB_PORT = 5432

SQL_SCRIPT_PATH = "/mnt/c/GIT/JobApi.New/sql/create_consolidated_records.sql"

def get_duplicate_hashes(conn):
    """Query for all duplicate job_description_hash values"""
    print("Fetching duplicate hashes...")
    with conn.cursor() as cur:
        cur.execute("""
            SELECT job_description_hash, COUNT(*) as count
            FROM jobs
            WHERE job_description_hash IS NOT NULL
            GROUP BY job_description_hash
            HAVING COUNT(*) > 1
            ORDER BY COUNT(*) DESC
        """)
        results = cur.fetchall()
    print(f"Found {len(results)} duplicate hashes to process")
    return results

def consolidate_hash(job_hash, count):
    """Run consolidation script for a single hash"""
    try:
        # Build the psql command
        cmd = [
            "psql",
            "-h", DB_HOST,
            "-U", DB_USER,
            "-d", DB_NAME,
            "-v", f"job_hash={job_hash}",
            "-f", SQL_SCRIPT_PATH
        ]

        env = {"PGPASSWORD": DB_PASSWORD}

        # Run the command
        result = subprocess.run(
            cmd,
            env=env,
            capture_output=True,
            text=True,
            timeout=300  # 5 minute timeout per hash
        )

        if result.returncode != 0:
            print(f"FAILED: {job_hash} (count: {count})")
            print(f"  Error: {result.stderr}")
            return False
        else:
            print(f"SUCCESS: {job_hash} (count: {count})")
            return True

    except subprocess.TimeoutExpired:
        print(f"TIMEOUT: {job_hash} (count: {count}) - exceeded 5 minutes")
        return False
    except Exception as e:
        print(f"ERROR: {job_hash} (count: {count}) - {str(e)}")
        return False

def main():
    print(f"Starting consolidation at {datetime.now()}")
    print("=" * 80)

    try:
        # Connect to database
        conn = psycopg2.connect(
            host=DB_HOST,
            database=DB_NAME,
            user=DB_USER,
            password=DB_PASSWORD,
            port=DB_PORT
        )

        # Get all duplicate hashes
        duplicate_hashes = get_duplicate_hashes(conn)
        conn.close()

        # Process each hash
        processed = 0
        failed = 0

        for job_hash, count in duplicate_hashes:
            processed += 1
            print(f"[{processed}/{len(duplicate_hashes)}] Processing hash: {job_hash}")

            success = consolidate_hash(job_hash, count)

            if not success:
                failed += 1
                print(f"\nERROR: Consolidation failed for hash {job_hash}")
                print(f"Processed: {processed}/{len(duplicate_hashes)}")
                print(f"Failed: {failed}")
                sys.exit(1)

            # Print progress every 100 hashes
            if processed % 100 == 0:
                print(f"\n--- Progress: {processed}/{len(duplicate_hashes)} ---\n")

        print("\n" + "=" * 80)
        print(f"Consolidation completed successfully at {datetime.now()}")
        print(f"Total processed: {processed}")
        print(f"Total failed: {failed}")

    except Exception as e:
        print(f"\nFATAL ERROR: {str(e)}")
        sys.exit(1)

if __name__ == "__main__":
    main()
