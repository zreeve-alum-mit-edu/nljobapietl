using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace JobApi.Common.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:vector", ",,");

            migrationBuilder.CreateTable(
                name: "files",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    filename = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    dateprocessed = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_files", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "embedding_batches",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_id = table.Column<Guid>(type: "uuid", nullable: false),
                    batch_file_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    openai_batch_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    openai_input_file_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    submitted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    error_message = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_embedding_batches", x => x.id);
                    table.ForeignKey(
                        name: "FK_embedding_batches_files_file_id",
                        column: x => x.file_id,
                        principalTable: "files",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "jobs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_inserted = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status_change_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_valid = table.Column<bool>(type: "boolean", nullable: false),
                    file_id = table.Column<Guid>(type: "uuid", nullable: false),
                    portal = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    sourcecc = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    isduplicate = table.Column<bool>(type: "boolean", nullable: false),
                    locale = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    job_title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    job_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    job_description = table.Column<string>(type: "text", nullable: true),
                    location = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    region = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    locality = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    postcode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    latitude = table.Column<decimal>(type: "numeric(10,7)", precision: 10, scale: 7, nullable: true),
                    longitude = table.Column<decimal>(type: "numeric(10,7)", precision: 10, scale: 7, nullable: true),
                    date_posted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    employment_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    company_name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    company_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    validthrough = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    workplace_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    generated_workplace = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    generated_workplace_inferred = table.Column<bool>(type: "boolean", nullable: true),
                    generated_workplace_confidence = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    generated_city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    generated_state = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    generated_country = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    embedding = table.Column<Vector>(type: "vector(1536)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_jobs", x => x.id);
                    table.ForeignKey(
                        name: "FK_jobs_files_file_id",
                        column: x => x.file_id,
                        principalTable: "files",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "location_batches",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_id = table.Column<Guid>(type: "uuid", nullable: false),
                    batch_file_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    openai_batch_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    openai_input_file_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    submitted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    error_message = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_location_batches", x => x.id);
                    table.ForeignKey(
                        name: "FK_location_batches_files_file_id",
                        column: x => x.file_id,
                        principalTable: "files",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "workplace_batches",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_id = table.Column<Guid>(type: "uuid", nullable: false),
                    batch_file_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    openai_batch_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    openai_input_file_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    submitted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    error_message = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workplace_batches", x => x.id);
                    table.ForeignKey(
                        name: "FK_workplace_batches_files_file_id",
                        column: x => x.file_id,
                        principalTable: "files",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_embedding_batches_file_id",
                table: "embedding_batches",
                column: "file_id");

            migrationBuilder.CreateIndex(
                name: "IX_embedding_batches_openai_batch_id",
                table: "embedding_batches",
                column: "openai_batch_id");

            migrationBuilder.CreateIndex(
                name: "IX_embedding_batches_status",
                table: "embedding_batches",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_files_filename",
                table: "files",
                column: "filename");

            migrationBuilder.CreateIndex(
                name: "IX_jobs_country",
                table: "jobs",
                column: "country");

            migrationBuilder.CreateIndex(
                name: "IX_jobs_date_inserted",
                table: "jobs",
                column: "date_inserted");

            migrationBuilder.CreateIndex(
                name: "IX_jobs_date_posted",
                table: "jobs",
                column: "date_posted");

            migrationBuilder.CreateIndex(
                name: "IX_jobs_embedding",
                table: "jobs",
                column: "embedding")
                .Annotation("Npgsql:IndexMethod", "hnsw")
                .Annotation("Npgsql:IndexOperators", new[] { "vector_cosine_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_jobs_employment_type",
                table: "jobs",
                column: "employment_type");

            migrationBuilder.CreateIndex(
                name: "IX_jobs_file_id",
                table: "jobs",
                column: "file_id");

            migrationBuilder.CreateIndex(
                name: "IX_jobs_isduplicate",
                table: "jobs",
                column: "isduplicate");

            migrationBuilder.CreateIndex(
                name: "IX_location_batches_file_id",
                table: "location_batches",
                column: "file_id");

            migrationBuilder.CreateIndex(
                name: "IX_location_batches_openai_batch_id",
                table: "location_batches",
                column: "openai_batch_id");

            migrationBuilder.CreateIndex(
                name: "IX_location_batches_status",
                table: "location_batches",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_workplace_batches_file_id",
                table: "workplace_batches",
                column: "file_id");

            migrationBuilder.CreateIndex(
                name: "IX_workplace_batches_openai_batch_id",
                table: "workplace_batches",
                column: "openai_batch_id");

            migrationBuilder.CreateIndex(
                name: "IX_workplace_batches_status",
                table: "workplace_batches",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "embedding_batches");

            migrationBuilder.DropTable(
                name: "jobs");

            migrationBuilder.DropTable(
                name: "location_batches");

            migrationBuilder.DropTable(
                name: "workplace_batches");

            migrationBuilder.DropTable(
                name: "files");
        }
    }
}
