using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobApi.Common.Migrations
{
    /// <inheritdoc />
    public partial class AddLlmWorkplaceRetryCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "llm_workplace_retry_count",
                table: "jobs",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "llm_workplace_retry_count",
                table: "jobs");
        }
    }
}
