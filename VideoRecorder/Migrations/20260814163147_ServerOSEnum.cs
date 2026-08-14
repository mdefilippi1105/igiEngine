using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VideoRecorder.Migrations
{
    /// <inheritdoc />
    public partial class ServerOSEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsUnix",
                table: "Server",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Os",
                table: "Server",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsUnix",
                table: "Server");

            migrationBuilder.DropColumn(
                name: "Os",
                table: "Server");
        }
    }
}
