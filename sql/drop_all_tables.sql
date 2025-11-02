-- Drop all tables and start fresh
DROP TABLE IF EXISTS embedding_batches CASCADE;
DROP TABLE IF EXISTS location_batches CASCADE;
DROP TABLE IF EXISTS workplace_batches CASCADE;
DROP TABLE IF EXISTS jobs CASCADE;
DROP TABLE IF EXISTS files CASCADE;

-- Drop any remaining triggers/functions
DROP FUNCTION IF EXISTS set_initial_status_change_date() CASCADE;
DROP FUNCTION IF EXISTS update_job_status_change_date() CASCADE;
