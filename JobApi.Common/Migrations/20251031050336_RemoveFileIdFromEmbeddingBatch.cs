using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobApi.Common.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFileIdFromEmbeddingBatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_embedding_batches_files_file_id",
                table: "embedding_batches");

            migrationBuilder.DropIndex(
                name: "IX_embedding_batches_file_id",
                table: "embedding_batches");

            migrationBuilder.DropColumn(
                name: "file_id",
                table: "embedding_batches");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "file_id",
                table: "embedding_batches",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_embedding_batches_file_id",
                table: "embedding_batches",
                column: "file_id");

            migrationBuilder.AddForeignKey(
                name: "FK_embedding_batches_files_file_id",
                table: "embedding_batches",
                column: "file_id",
                principalTable: "files",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
