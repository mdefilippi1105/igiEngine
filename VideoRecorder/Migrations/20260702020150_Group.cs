using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VideoRecorder.Migrations
{
    /// <inheritdoc />
    public partial class Group : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CameraGroupId",
                table: "Camera",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GroupId",
                table: "Camera",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CameraGroup",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CameraGroup", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Camera_CameraGroupId",
                table: "Camera",
                column: "CameraGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Camera_CameraGroup_CameraGroupId",
                table: "Camera",
                column: "CameraGroupId",
                principalTable: "CameraGroup",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Camera_CameraGroup_CameraGroupId",
                table: "Camera");

            migrationBuilder.DropTable(
                name: "CameraGroup");

            migrationBuilder.DropIndex(
                name: "IX_Camera_CameraGroupId",
                table: "Camera");

            migrationBuilder.DropColumn(
                name: "CameraGroupId",
                table: "Camera");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "Camera");
        }
    }
}
