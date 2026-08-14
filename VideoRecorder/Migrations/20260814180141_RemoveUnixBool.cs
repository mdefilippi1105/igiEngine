using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VideoRecorder.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnixBool : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsUnix",
                table: "Server");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsUnix",
                table: "Server",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
