-- Enable pgvector extension
CREATE EXTENSION IF NOT EXISTS vector;

-- Create files table
CREATE TABLE files (
    id UUID PRIMARY KEY,
    filename VARCHAR(255) NOT NULL,
    date_processed TIMESTAMP WITH TIME ZONE NOT NULL
);

-- Create jobs table with all columns
CREATE TABLE jobs (
    id UUID PRIMARY KEY,
    date_inserted TIMESTAMP WITH TIME ZONE NOT NULL,
    status VARCHAR(50) NOT NULL DEFAULT 'pending',
    status_change_date TIMESTAMP WITH TIME ZONE,
    is_valid BOOLEAN NOT NULL DEFAULT true,
    file_id UUID NOT NULL,
    portal VARCHAR(100),
    source VARCHAR(100),
    sourcecc VARCHAR(10),
    isduplicate BOOLEAN NOT NULL DEFAULT false,
    locale VARCHAR(10),
    job_title VARCHAR(500),
    job_url VARCHAR(1000),
    job_description TEXT,
    location VARCHAR(500),
    country VARCHAR(100),
    region VARCHAR(100),
    locality VARCHAR(100),
    postcode VARCHAR(20),
    latitude NUMERIC(10,7),
    longitude NUMERIC(10,7),
    date_posted TIMESTAMP WITH TIME ZONE,
    employment_type VARCHAR(100),
    company_name VARCHAR(500),
    company_url VARCHAR(1000),
    validthrough TIMESTAMP WITH TIME ZONE,
    workplace_type VARCHAR(50),
    generated_workplace VARCHAR(20),
    generated_workplace_inferred BOOLEAN,
    generated_workplace_confidence VARCHAR(20),
    generated_city VARCHAR(100),
    generated_state VARCHAR(2),
    generated_country VARCHAR(2),
    embedding vector(1536),
    CONSTRAINT "FK_jobs_files_file_id" FOREIGN KEY (file_id) REFERENCES files(id) ON DELETE RESTRICT
);

-- Create indexes on jobs table
CREATE INDEX "IX_jobs_file_id" ON jobs(file_id);
CREATE INDEX "IX_jobs_date_inserted" ON jobs(date_inserted);
CREATE INDEX "IX_jobs_date_posted" ON jobs(date_posted);
CREATE INDEX "IX_jobs_country" ON jobs(country);
CREATE INDEX "IX_jobs_employment_type" ON jobs(employment_type);
CREATE INDEX "IX_jobs_isduplicate" ON jobs(isduplicate);
CREATE INDEX idx_jobs_status ON jobs(status);
CREATE INDEX idx_jobs_is_valid ON jobs(is_valid);
CREATE INDEX idx_jobs_generated_city ON jobs(generated_city);
CREATE INDEX idx_jobs_generated_state ON jobs(generated_state);
CREATE INDEX idx_jobs_generated_country ON jobs(generated_country);
CREATE INDEX idx_jobs_generated_workplace ON jobs(generated_workplace);

-- Create HNSW index for vector similarity search
CREATE INDEX "IX_jobs_embedding" ON jobs USING hnsw (embedding vector_cosine_ops);

-- Create trigger function for status_change_date on INSERT
CREATE OR REPLACE FUNCTION set_initial_status_change_date()
RETURNS TRIGGER AS $$
BEGIN
    NEW.status_change_date := NEW.date_inserted;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Create trigger function for status_change_date on UPDATE
CREATE OR REPLACE FUNCTION update_job_status_change_date()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW.status IS DISTINCT FROM OLD.status THEN
        NEW.status_change_date := NOW();
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Attach triggers to jobs table
CREATE TRIGGER job_initial_status_trigger
    BEFORE INSERT ON jobs
    FOR EACH ROW
    EXECUTE FUNCTION set_initial_status_change_date();

CREATE TRIGGER job_status_change_trigger
    BEFORE UPDATE ON jobs
    FOR EACH ROW
    EXECUTE FUNCTION update_job_status_change_date();
