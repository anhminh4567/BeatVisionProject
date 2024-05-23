using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class Tenth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tracks_UserProfiles_OwnerId",
                table: "Tracks");

            migrationBuilder.DropIndex(
                name: "IX_Tracks_OwnerId",
                table: "Tracks");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Tracks");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OwnerId",
                table: "Tracks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Tracks_OwnerId",
                table: "Tracks",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tracks_UserProfiles_OwnerId",
                table: "Tracks",
                column: "OwnerId",
                principalTable: "UserProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
