-- Remove file_id foreign key constraint and column from location_batches table
-- This allows location batches to be created without being tied to a specific file

-- Drop the foreign key constraint
ALTER TABLE location_batches DROP CONSTRAINT IF EXISTS location_batches_file_id_fkey;

-- Drop the index on file_id
DROP INDEX IF EXISTS idx_location_batches_file_id;

-- Drop the file_id column
ALTER TABLE location_batches DROP COLUMN IF EXISTS file_id;
