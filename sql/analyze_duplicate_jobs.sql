-- Analyze duplicate jobs by description hash
-- This creates a JSON output showing duplicate hashes with their locations and URLs

WITH duplicate_hashes AS (
    SELECT job_description_hash, COUNT(*) as job_count
    FROM jobs
    WHERE job_description_hash IS NOT NULL
    GROUP BY job_description_hash
    HAVING COUNT(*) > 1
),
job_details AS (
    SELECT
        j.job_description_hash,
        j.generated_city,
        j.generated_state,
        j.generated_country,
        j.job_url
    FROM jobs j
    INNER JOIN duplicate_hashes dh ON dh.job_description_hash = j.job_description_hash
),
location_groups AS (
    SELECT
        job_description_hash,
        COALESCE(generated_city, 'NULL') as city,
        COALESCE(generated_state, 'NULL') as state,
        COALESCE(generated_country, 'NULL') as country,
        json_agg(job_url ORDER BY job_url) FILTER (WHERE job_url IS NOT NULL) as urls
    FROM job_details
    GROUP BY job_description_hash, generated_city, generated_state, generated_country
)
SELECT
    json_agg(
        json_build_object(
            'job_description_hash', job_description_hash,
            'total_jobs', (
                SELECT COUNT(*)
                FROM job_details jd
                WHERE jd.job_description_hash = lg.job_description_hash
            ),
            'locations', (
                SELECT json_agg(
                    json_build_object(
                        'generated_city', NULLIF(city, 'NULL'),
                        'generated_state', NULLIF(state, 'NULL'),
                        'generated_country', NULLIF(country, 'NULL'),
                        'urls', urls
                    ) ORDER BY country, state, city
                )
                FROM location_groups lg2
                WHERE lg2.job_description_hash = lg.job_description_hash
            )
        ) ORDER BY (
            SELECT COUNT(*)
            FROM job_details jd
            WHERE jd.job_description_hash = lg.job_description_hash
        ) DESC
    ) as analysis
FROM (SELECT DISTINCT job_description_hash FROM location_groups) lg;
