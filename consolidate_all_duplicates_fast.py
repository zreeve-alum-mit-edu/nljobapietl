#!/usr/bin/env python3
import psycopg2
import sys
from datetime import datetime

# Database connection details
DB_HOST = "nljobsearchapi.c74ioigi2bn4.us-east-2.rds.amazonaws.com"
DB_NAME = "nljobsearch"
DB_USER = "JSadmin"
DB_PASSWORD = "mxofoyLVkiV2aQACxIbJ"
DB_PORT = 5432

def main():
    print(f"Starting consolidation at {datetime.now()}", flush=True)
    print("=" * 80, flush=True)

    try:
        # Connect to database
        conn = psycopg2.connect(
            host=DB_HOST,
            database=DB_NAME,
            user=DB_USER,
            password=DB_PASSWORD,
            port=DB_PORT
        )
        conn.autocommit = False  # We'll manage transactions manually
        cur = conn.cursor()

        # Get all duplicate hashes
        print("Fetching duplicate hashes...", flush=True)
        cur.execute("""
            SELECT job_description_hash, COUNT(*) as count
            FROM jobs
            WHERE job_description_hash IS NOT NULL
            GROUP BY job_description_hash
            HAVING COUNT(*) > 1
            ORDER BY COUNT(*) DESC
        """)
        duplicate_hashes = cur.fetchall()
        print(f"Found {len(duplicate_hashes)} duplicate hashes to process", flush=True)

        # Create temp tables ONCE
        print("Creating temporary tables...", flush=True)
        cur.execute("""
            CREATE TEMP TABLE temp_keeper_job (keeper_job_id UUID);
            CREATE TEMP TABLE temp_unique_locations (
                generated_city VARCHAR(100),
                generated_state VARCHAR(2),
                generated_country VARCHAR(2),
                location VARCHAR(500),
                country VARCHAR(100),
                region VARCHAR(100),
                locality VARCHAR(100),
                postcode VARCHAR(20),
                latitude DECIMAL(10,7),
                longitude DECIMAL(10,7),
                llm_location_retry_count INTEGER
            );
            CREATE TEMP TABLE temp_jobs_with_urls (
                job_id UUID,
                job_url VARCHAR(1000),
                generated_city VARCHAR(100),
                generated_state VARCHAR(2),
                generated_country VARCHAR(2)
            );
        """)
        conn.commit()
        print("Temp tables created", flush=True)

        # Process each hash
        processed = 0
        failed = 0

        for job_hash, count in duplicate_hashes:
            processed += 1
            try:
                # Begin transaction for this hash
                cur.execute("BEGIN")

                # Truncate temp tables
                cur.execute("""
                    TRUNCATE temp_keeper_job;
                    TRUNCATE temp_unique_locations;
                    TRUNCATE temp_jobs_with_urls;
                """)

                # Step 1: Find keeper job
                cur.execute("""
                    INSERT INTO temp_keeper_job (keeper_job_id)
                    SELECT id FROM jobs
                    WHERE job_description_hash = %s
                    ORDER BY date_inserted ASC
                    LIMIT 1
                """, (job_hash,))

                # Step 2: Get unique locations
                cur.execute("""
                    INSERT INTO temp_unique_locations
                    SELECT DISTINCT
                        generated_city, generated_state, generated_country,
                        location, country, region, locality, postcode,
                        latitude, longitude, llm_location_retry_count
                    FROM jobs
                    WHERE job_description_hash = %s
                """, (job_hash,))

                # Step 3: Create job_location records
                cur.execute("""
                    INSERT INTO job_locations (
                        id, job_id, location, country, region, locality, postcode,
                        latitude, longitude, generated_city, generated_state,
                        generated_country, llm_location_retry_count
                    )
                    SELECT
                        gen_random_uuid(), (SELECT keeper_job_id FROM temp_keeper_job),
                        location, country, region, locality, postcode,
                        latitude, longitude, generated_city, generated_state,
                        generated_country, llm_location_retry_count
                    FROM temp_unique_locations
                """)

                # Step 4: Get jobs with URLs
                cur.execute("""
                    INSERT INTO temp_jobs_with_urls
                    SELECT id, job_url, generated_city, generated_state, generated_country
                    FROM jobs
                    WHERE job_description_hash = %s AND job_url IS NOT NULL
                """, (job_hash,))

                # Step 5: Create job_location_url records
                cur.execute("""
                    INSERT INTO job_location_urls (id, job_location_id, url)
                    SELECT DISTINCT
                        gen_random_uuid(), jl.id, jwu.job_url
                    FROM temp_jobs_with_urls jwu
                    INNER JOIN job_locations jl ON
                        jl.job_id = (SELECT keeper_job_id FROM temp_keeper_job)
                        AND COALESCE(jwu.generated_city, '') = COALESCE(jl.generated_city, '')
                        AND COALESCE(jwu.generated_state, '') = COALESCE(jl.generated_state, '')
                        AND COALESCE(jwu.generated_country, '') = COALESCE(jl.generated_country, '')
                """)

                # Step 6: Delete duplicate jobs
                cur.execute("""
                    DELETE FROM jobs
                    WHERE job_description_hash = %s
                      AND id != (SELECT keeper_job_id FROM temp_keeper_job)
                """, (job_hash,))

                # Commit transaction
                conn.commit()
                print(f"[{processed}/{len(duplicate_hashes)}] SUCCESS: {job_hash} (count: {count})", flush=True)

                # Progress marker every 100
                if processed % 100 == 0:
                    print(f"\n--- Progress: {processed}/{len(duplicate_hashes)} ---\n", flush=True)

            except Exception as e:
                conn.rollback()
                failed += 1
                print(f"\n[{processed}/{len(duplicate_hashes)}] FAILED: {job_hash} (count: {count})", flush=True)
                print(f"  Error: {str(e)}", flush=True)
                print(f"Processed: {processed}/{len(duplicate_hashes)}", flush=True)
                print(f"Failed: {failed}", flush=True)
                cur.close()
                conn.close()
                sys.exit(1)

        print("\n" + "=" * 80, flush=True)
        print(f"Consolidation completed successfully at {datetime.now()}", flush=True)
        print(f"Total processed: {processed}", flush=True)
        print(f"Total failed: {failed}", flush=True)

        cur.close()
        conn.close()

    except Exception as e:
        print(f"\nFATAL ERROR: {str(e)}", flush=True)
        sys.exit(1)

if __name__ == "__main__":
    main()
