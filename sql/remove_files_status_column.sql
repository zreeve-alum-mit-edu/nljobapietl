-- Remove status column from files table (status tracking moved to jobs table)
ALTER TABLE files DROP COLUMN IF EXISTS status;
