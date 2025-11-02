# Database Connection Reference

## PostgreSQL Connection Details

**ALWAYS use these exact connection details from .env:**

```bash
DB_HOST=nljobsearchapi.c74ioigi2bn4.us-east-2.rds.amazonaws.com
DB_NAME=nljobsearch
DB_USER=JSadmin
DB_PASSWORD=mxofoyLVkiV2aQACxIbJ
DB_PORT=5432
```

## Correct psql Command Format

```bash
PGPASSWORD="mxofoyLVkiV2aQACxIbJ" psql -h nljobsearchapi.c74ioigi2bn4.us-east-2.rds.amazonaws.com -U JSadmin -d nljobsearch -c "SQL_QUERY_HERE"
```

## Common Queries

### Job Status Breakdown
```sql
SELECT status, COUNT(*) as count
FROM jobs
GROUP BY status
ORDER BY count DESC;
```

### Workplace Batches Status
```sql
SELECT status, COUNT(*) as count
FROM workplace_batches
GROUP BY status
ORDER BY count DESC;
```

### Check Pending Batches
```sql
SELECT id, status, created_at
FROM workplace_batches
WHERE status = 'pending'
ORDER BY created_at
LIMIT 10;
```

### Database Size
```sql
SELECT
    pg_database.datname,
    pg_size_pretty(pg_database_size(pg_database.datname)) AS size
FROM pg_database
WHERE datname = 'nljobsearch';
```

### Table Sizes
```sql
SELECT
    schemaname,
    tablename,
    pg_size_pretty(pg_total_relation_size(schemaname||'.'||tablename)) AS size
FROM pg_tables
WHERE schemaname = 'public'
ORDER BY pg_total_relation_size(schemaname||'.'||tablename) DESC;
```

### Jobs with Specific Status
```sql
SELECT COUNT(*) as total, COUNT(generated_workplace) as with_workplace
FROM jobs
WHERE status = 'llm_batches_generated';
```

## Important Notes

- The database is on AWS RDS in us-east-2
- Use PGPASSWORD environment variable to avoid password prompts
- Always quote the password and SQL queries
- The jobs table does NOT have a workplace_batch_id column
- The workplace_batches table tracks batch metadata separately
