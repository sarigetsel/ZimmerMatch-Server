using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZimmerMatch.Migrations
{
    /// <inheritdoc />
    public partial class init1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Availabilities_Zimmers_ZimmersZimmerId",
                table: "Availabilities");

            migrationBuilder.DropColumn(
                name: "ZiimerId",
                table: "Availabilities");

            migrationBuilder.RenameColumn(
                name: "ZimmersZimmerId",
                table: "Availabilities",
                newName: "ZimmerId");

            migrationBuilder.RenameIndex(
                name: "IX_Availabilities_ZimmersZimmerId",
                table: "Availabilities",
                newName: "IX_Availabilities_ZimmerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Availabilities_Zimmers_ZimmerId",
                table: "Availabilities",
                column: "ZimmerId",
                principalTable: "Zimmers",
                principalColumn: "ZimmerId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Availabilities_Zimmers_ZimmerId",
                table: "Availabilities");

            migrationBuilder.RenameColumn(
                name: "ZimmerId",
                table: "Availabilities",
                newName: "ZimmersZimmerId");

            migrationBuilder.RenameIndex(
                name: "IX_Availabilities_ZimmerId",
                table: "Availabilities",
                newName: "IX_Availabilities_ZimmersZimmerId");

            migrationBuilder.AddColumn<int>(
                name: "ZiimerId",
                table: "Availabilities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Availabilities_Zimmers_ZimmersZimmerId",
                table: "Availabilities",
                column: "ZimmersZimmerId",
                principalTable: "Zimmers",
                principalColumn: "ZimmerId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
