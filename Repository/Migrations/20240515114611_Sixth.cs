using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class Sixth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Albums_BlobFileData_BannerBlobId",
                table: "Albums");

            migrationBuilder.DropForeignKey(
                name: "FK_Tracks_BlobFileData_BannerBlobId",
                table: "Tracks");

            migrationBuilder.DropForeignKey(
                name: "FK_UserProfiles_BlobFileData_BannerBlobId",
                table: "UserProfiles");

            migrationBuilder.DropIndex(
                name: "IX_UserProfiles_BannerBlobId",
                table: "UserProfiles");

            migrationBuilder.DropIndex(
                name: "IX_Tracks_BannerBlobId",
                table: "Tracks");

            migrationBuilder.DropIndex(
                name: "IX_Albums_BannerBlobId",
                table: "Albums");

            migrationBuilder.DropColumn(
                name: "BannerBlobId",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "BannerBlobId",
                table: "Tracks");

            migrationBuilder.DropColumn(
                name: "BannerBlobId",
                table: "Albums");

            migrationBuilder.AddColumn<string>(
                name: "ProfileBlobUrl",
                table: "UserProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfileBlobUrl",
                table: "Tracks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfileBlobUrl",
                table: "Albums",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfileBlobUrl",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "ProfileBlobUrl",
                table: "Tracks");

            migrationBuilder.DropColumn(
                name: "ProfileBlobUrl",
                table: "Albums");

            migrationBuilder.AddColumn<int>(
                name: "BannerBlobId",
                table: "UserProfiles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BannerBlobId",
                table: "Tracks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BannerBlobId",
                table: "Albums",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_BannerBlobId",
                table: "UserProfiles",
                column: "BannerBlobId",
                unique: true,
                filter: "[BannerBlobId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Tracks_BannerBlobId",
                table: "Tracks",
                column: "BannerBlobId",
                unique: true,
                filter: "[BannerBlobId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Albums_BannerBlobId",
                table: "Albums",
                column: "BannerBlobId",
                unique: true,
                filter: "[BannerBlobId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Albums_BlobFileData_BannerBlobId",
                table: "Albums",
                column: "BannerBlobId",
                principalTable: "BlobFileData",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tracks_BlobFileData_BannerBlobId",
                table: "Tracks",
                column: "BannerBlobId",
                principalTable: "BlobFileData",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserProfiles_BlobFileData_BannerBlobId",
                table: "UserProfiles",
                column: "BannerBlobId",
                principalTable: "BlobFileData",
                principalColumn: "Id");
        }
    }
}
