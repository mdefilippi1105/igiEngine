using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VideoRecorder.Migrations
{
    /// <inheritdoc />
    public partial class CameraGroupNewFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CameraGroupId",
                table: "User",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "CameraGroup",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_User_CameraGroupId",
                table: "User",
                column: "CameraGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_User_CameraGroup_CameraGroupId",
                table: "User",
                column: "CameraGroupId",
                principalTable: "CameraGroup",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_User_CameraGroup_CameraGroupId",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_User_CameraGroupId",
                table: "User");

            migrationBuilder.DropColumn(
                name: "CameraGroupId",
                table: "User");

            migrationBuilder.DropColumn(
                name: "IsEnabled",
                table: "CameraGroup");
        }
    }
}
