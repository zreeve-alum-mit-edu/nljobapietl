using Microsoft.EntityFrameworkCore;
using JobApi.Common.Entities;

namespace JobApi.Common;

public class JobContext : DbContext
{
    public JobContext(DbContextOptions<JobContext> options) : base(options)
    {
    }

    public DbSet<Job> Jobs { get; set; }
    public DbSet<JobEmbedding> JobEmbeddings { get; set; }
    public DbSet<Entities.File> Files { get; set; }
    public DbSet<WorkplaceBatch> WorkplaceBatches { get; set; }
    public DbSet<LocationBatch> LocationBatches { get; set; }
    public DbSet<EmbeddingBatch> EmbeddingBatches { get; set; }
    public DbSet<LocationLookup> LocationLookups { get; set; }
    public DbSet<Geolocation> Geolocations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Enable pgvector extension
        modelBuilder.HasPostgresExtension("vector");

        // File entity configuration
        modelBuilder.Entity<Entities.File>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Filename);
        });

        // Job entity configuration
        modelBuilder.Entity<Job>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Index on FileId for FK queries
            entity.HasIndex(e => e.FileId);

            // Useful indexes for searching/filtering
            entity.HasIndex(e => e.DateInserted);
            entity.HasIndex(e => e.DatePosted);
            entity.HasIndex(e => e.IsDuplicate);
            entity.HasIndex(e => e.Country);
            entity.HasIndex(e => e.EmploymentType);

            // Unique constraint on job_url to prevent duplicates (filtered to exclude nulls)
            entity.HasIndex(e => e.JobUrl)
                .IsUnique()
                .HasFilter("job_url IS NOT NULL");

            // Configure relationship
            entity.HasOne(e => e.File)
                .WithMany(f => f.Jobs)
                .HasForeignKey(e => e.FileId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure decimal precision for coordinates
            entity.Property(e => e.Latitude)
                .HasPrecision(10, 7);

            entity.Property(e => e.Longitude)
                .HasPrecision(10, 7);
        });

        // WorkplaceBatch entity configuration
        modelBuilder.Entity<WorkplaceBatch>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.OpenAiBatchId);
        });

        // LocationBatch entity configuration
        modelBuilder.Entity<LocationBatch>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.OpenAiBatchId);
        });

        // EmbeddingBatch entity configuration
        modelBuilder.Entity<EmbeddingBatch>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.OpenAiBatchId);
        });

        // LocationLookup entity configuration
        modelBuilder.Entity<LocationLookup>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.LocationText).IsUnique();
        });

        // Geolocation entity configuration
        modelBuilder.Entity<Geolocation>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Create composite index on city and state for fast lookups
            entity.HasIndex(e => new { e.City, e.State });

            // Configure decimal precision for coordinates
            entity.Property(e => e.Latitude)
                .HasPrecision(10, 7);

            entity.Property(e => e.Longitude)
                .HasPrecision(10, 7);
        });

        // JobEmbedding entity configuration
        modelBuilder.Entity<JobEmbedding>(entity =>
        {
            entity.HasKey(e => e.JobId);

            // Configure the embedding vector column (1536 dimensions for text-embedding-3-small)
            entity.Property(e => e.Embedding)
                .HasColumnType("vector(1536)");

            // Create HNSW index for fast vector similarity search
            entity.HasIndex(e => e.Embedding)
                .HasMethod("hnsw")
                .HasOperators("vector_cosine_ops");

            // Configure relationship with cascade delete
            entity.HasOne(e => e.Job)
                .WithOne()
                .HasForeignKey<JobEmbedding>(e => e.JobId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    public static string GetConnectionString()
    {
        var host = Environment.GetEnvironmentVariable("DB_HOST");
        var user = Environment.GetEnvironmentVariable("DB_USER");
        var password = Environment.GetEnvironmentVariable("DB_PASSWORD");
        var port = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
        var database = Environment.GetEnvironmentVariable("DB_NAME") ?? "postgres";

        // Keepalive and timeout settings for WSL2 -> AWS RDS
        return $"Host={host};Port={port};Database={database};Username={user};Password={password};Keepalive=30;Timeout=15;Maximum Pool Size=200";
    }

    public static JobContext Create()
    {
        var connectionString = GetConnectionString();

        var optionsBuilder = new DbContextOptionsBuilder<JobContext>();
        optionsBuilder.UseNpgsql(connectionString, npgsql =>
        {
            npgsql.UseVector();
            // EF Core's built-in retry strategy for transient failures
            npgsql.EnableRetryOnFailure(
                maxRetryCount: 6,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorCodesToAdd: null);
            npgsql.CommandTimeout(60);
        });

        return new JobContext(optionsBuilder.Options);
    }
}
