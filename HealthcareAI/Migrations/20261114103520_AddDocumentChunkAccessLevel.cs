using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthcareAI.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentChunkAccessLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccessLevel",
                table: "DocumentChunks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccessLevel",
                table: "DocumentChunks");
        }
    }
}
