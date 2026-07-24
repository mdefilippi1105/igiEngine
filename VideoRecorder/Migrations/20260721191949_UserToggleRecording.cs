using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VideoRecorder.Migrations
{
    /// <inheritdoc />
    public partial class UserToggleRecording : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "UserToggledRecording",
                table: "Camera",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserToggledRecording",
                table: "Camera");
        }
    }
}
