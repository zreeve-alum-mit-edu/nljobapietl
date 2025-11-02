-- Create location_batches table to track batch submissions to OpenAI
CREATE TABLE IF NOT EXISTS location_batches (
    id UUID PRIMARY KEY,
    file_id UUID NOT NULL REFERENCES files(id),
    batch_file_path VARCHAR(500) NOT NULL,
    openai_batch_id VARCHAR(100),
    openai_input_file_id VARCHAR(100),
    status VARCHAR(50) NOT NULL DEFAULT 'pending',
    created_at TIMESTAMP NOT NULL,
    submitted_at TIMESTAMP,
    completed_at TIMESTAMP,
    error_message TEXT
);

-- Create indexes for common queries
CREATE INDEX IF NOT EXISTS idx_location_batches_file_id ON location_batches(file_id);
CREATE INDEX IF NOT EXISTS idx_location_batches_status ON location_batches(status);
CREATE INDEX IF NOT EXISTS idx_location_batches_openai_batch_id ON location_batches(openai_batch_id);
