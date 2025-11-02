using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobApi.Common.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueJobUrlConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_jobs_job_url",
                table: "jobs",
                column: "job_url",
                unique: true,
                filter: "job_url IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_jobs_job_url",
                table: "jobs");
        }
    }
}
