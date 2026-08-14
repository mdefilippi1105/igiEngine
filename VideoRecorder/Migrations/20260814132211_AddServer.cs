using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VideoRecorder.Migrations
{
    /// <inheritdoc />
    public partial class AddServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ServerId",
                table: "Camera",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Server",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Hostname = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Port = table.Column<int>(type: "int", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LastSeenUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Server", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Camera_ServerId",
                table: "Camera",
                column: "ServerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Camera_Server_ServerId",
                table: "Camera",
                column: "ServerId",
                principalTable: "Server",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Camera_Server_ServerId",
                table: "Camera");

            migrationBuilder.DropTable(
                name: "Server");

            migrationBuilder.DropIndex(
                name: "IX_Camera_ServerId",
                table: "Camera");

            migrationBuilder.DropColumn(
                name: "ServerId",
                table: "Camera");
        }
    }
}
