-- Create job_locations table
CREATE TABLE IF NOT EXISTS job_locations (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    job_id UUID NOT NULL,
    location VARCHAR(500),
    country VARCHAR(100),
    region VARCHAR(100),
    locality VARCHAR(100),
    postcode VARCHAR(20),
    latitude DECIMAL,
    longitude DECIMAL,
    gistlocation GEOGRAPHY(Point, 4326),
    generated_city VARCHAR(100),
    generated_state VARCHAR(2),
    generated_country VARCHAR(2),
    llm_location_retry_count INTEGER NOT NULL DEFAULT 0,
    CONSTRAINT fk_job_locations_jobs FOREIGN KEY (job_id) REFERENCES jobs(id) ON DELETE CASCADE
);

-- Create index on job_id for fast lookups
CREATE INDEX IF NOT EXISTS idx_job_locations_job_id ON job_locations(job_id);

-- Create GIST index for spatial queries
CREATE INDEX IF NOT EXISTS idx_job_locations_gistlocation ON job_locations USING GIST (gistlocation);

-- Create indexes on generated location fields for filtering
CREATE INDEX IF NOT EXISTS idx_job_locations_generated_state ON job_locations(generated_state) WHERE generated_state IS NOT NULL;
CREATE INDEX IF NOT EXISTS idx_job_locations_generated_country ON job_locations(generated_country) WHERE generated_country IS NOT NULL;

-- Create trigger function to auto-update gistlocation from lat/long
CREATE OR REPLACE FUNCTION update_joblocation_gist()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW.latitude IS NOT NULL AND NEW.longitude IS NOT NULL THEN
        NEW.gistlocation := ST_SetSRID(ST_MakePoint(NEW.longitude, NEW.latitude), 4326)::geography;
    ELSE
        NEW.gistlocation := NULL;
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Create trigger to call the function
CREATE TRIGGER trigger_update_joblocation_gist
    BEFORE INSERT OR UPDATE OF latitude, longitude ON job_locations
    FOR EACH ROW
    EXECUTE FUNCTION update_joblocation_gist();

-- Migrate existing location data from jobs table to job_locations
-- This creates one location record per job with existing location data
INSERT INTO job_locations (
    job_id,
    location,
    country,
    region,
    locality,
    postcode,
    latitude,
    longitude,
    generated_city,
    generated_state,
    generated_country,
    llm_location_retry_count
)
SELECT
    id,
    location,
    country,
    region,
    locality,
    postcode,
    latitude,
    longitude,
    generated_city,
    generated_state,
    generated_country,
    llm_location_retry_count
FROM jobs
WHERE location IS NOT NULL
   OR country IS NOT NULL
   OR region IS NOT NULL
   OR locality IS NOT NULL
   OR postcode IS NOT NULL
   OR latitude IS NOT NULL
   OR longitude IS NOT NULL
   OR generated_city IS NOT NULL
   OR generated_state IS NOT NULL
   OR generated_country IS NOT NULL
ON CONFLICT DO NOTHING;

-- Add comment for documentation
COMMENT ON TABLE job_locations IS 'Stores location information for jobs, supporting multiple locations per job';
COMMENT ON COLUMN job_locations.gistlocation IS 'PostGIS geography point automatically computed from latitude/longitude';
