-- Add status_change_date column to jobs table
ALTER TABLE jobs ADD COLUMN IF NOT EXISTS status_change_date TIMESTAMP WITH TIME ZONE;

-- Create trigger function to update status_change_date when status changes
CREATE OR REPLACE FUNCTION update_job_status_change_date()
RETURNS TRIGGER AS $$
BEGIN
    -- Only update status_change_date if status has actually changed
    IF NEW.status IS DISTINCT FROM OLD.status THEN
        NEW.status_change_date = NOW();
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Create trigger on jobs table
DROP TRIGGER IF EXISTS job_status_change_trigger ON jobs;
CREATE TRIGGER job_status_change_trigger
    BEFORE UPDATE ON jobs
    FOR EACH ROW
    EXECUTE FUNCTION update_job_status_change_date();

-- Set initial status_change_date for new inserts
CREATE OR REPLACE FUNCTION set_initial_status_change_date()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW.status_change_date IS NULL THEN
        NEW.status_change_date = NOW();
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS job_initial_status_trigger ON jobs;
CREATE TRIGGER job_initial_status_trigger
    BEFORE INSERT ON jobs
    FOR EACH ROW
    EXECUTE FUNCTION set_initial_status_change_date();

-- Update existing rows to set status_change_date to their date_inserted
UPDATE jobs
SET status_change_date = date_inserted
WHERE status_change_date IS NULL;

COMMENT ON COLUMN jobs.status_change_date IS 'Automatically updated whenever the status field changes';
COMMENT ON TRIGGER job_status_change_trigger ON jobs IS 'Automatically updates status_change_date when status is modified';
COMMENT ON TRIGGER job_initial_status_trigger ON jobs IS 'Sets initial status_change_date when job is first created';
