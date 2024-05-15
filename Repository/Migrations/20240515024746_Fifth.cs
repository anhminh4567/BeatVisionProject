using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class Fifth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfileBlobPath",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "AudioBlobPath",
                table: "Tracks");

            migrationBuilder.DropColumn(
                name: "BannerBlobPath",
                table: "Tracks");

            migrationBuilder.RenameColumn(
                name: "IsRemoved",
                table: "Tracks",
                newName: "IsAudioRemoved");

            migrationBuilder.RenameColumn(
                name: "IsPrivate",
                table: "Tracks",
                newName: "IsAudioPrivate");

            migrationBuilder.RenameColumn(
                name: "IsForSale",
                table: "Tracks",
                newName: "IsAudioForSale");

            migrationBuilder.AddColumn<int>(
                name: "BannerBlobId",
                table: "UserProfiles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AudioBlobId",
                table: "Tracks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AudioBpm",
                table: "Tracks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AudioLenghtSeconds",
                table: "Tracks",
                type: "int",
                nullable: false,
                defaultValue: 0);

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

            migrationBuilder.CreateTable(
                name: "BlobFileData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SizeMb = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PathUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DirectoryType = table.Column<string>(type: "nvarchar(30)", nullable: false),
                    FileExtension = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsPublicAccess = table.Column<bool>(type: "bit", nullable: false),
                    UploadedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlobFileData", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_BannerBlobId",
                table: "UserProfiles",
                column: "BannerBlobId",
                unique: true,
                filter: "[BannerBlobId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Tracks_AudioBlobId",
                table: "Tracks",
                column: "AudioBlobId",
                unique: true);

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
                name: "FK_Tracks_BlobFileData_AudioBlobId",
                table: "Tracks",
                column: "AudioBlobId",
                principalTable: "BlobFileData",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Albums_BlobFileData_BannerBlobId",
                table: "Albums");

            migrationBuilder.DropForeignKey(
                name: "FK_Tracks_BlobFileData_AudioBlobId",
                table: "Tracks");

            migrationBuilder.DropForeignKey(
                name: "FK_Tracks_BlobFileData_BannerBlobId",
                table: "Tracks");

            migrationBuilder.DropForeignKey(
                name: "FK_UserProfiles_BlobFileData_BannerBlobId",
                table: "UserProfiles");

            migrationBuilder.DropTable(
                name: "BlobFileData");

            migrationBuilder.DropIndex(
                name: "IX_UserProfiles_BannerBlobId",
                table: "UserProfiles");

            migrationBuilder.DropIndex(
                name: "IX_Tracks_AudioBlobId",
                table: "Tracks");

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
                name: "AudioBlobId",
                table: "Tracks");

            migrationBuilder.DropColumn(
                name: "AudioBpm",
                table: "Tracks");

            migrationBuilder.DropColumn(
                name: "AudioLenghtSeconds",
                table: "Tracks");

            migrationBuilder.DropColumn(
                name: "BannerBlobId",
                table: "Tracks");

            migrationBuilder.DropColumn(
                name: "BannerBlobId",
                table: "Albums");

            migrationBuilder.RenameColumn(
                name: "IsAudioRemoved",
                table: "Tracks",
                newName: "IsRemoved");

            migrationBuilder.RenameColumn(
                name: "IsAudioPrivate",
                table: "Tracks",
                newName: "IsPrivate");

            migrationBuilder.RenameColumn(
                name: "IsAudioForSale",
                table: "Tracks",
                newName: "IsForSale");

            migrationBuilder.AddColumn<string>(
                name: "ProfileBlobPath",
                table: "UserProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AudioBlobPath",
                table: "Tracks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BannerBlobPath",
                table: "Tracks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
