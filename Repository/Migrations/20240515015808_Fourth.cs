using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class Fourth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AlbumTrack_Albums_AlbumId",
                table: "AlbumTrack");

            migrationBuilder.DropForeignKey(
                name: "FK_Tracks_UserProfiles_OwnerId",
                table: "Tracks");

            migrationBuilder.AlterColumn<int>(
                name: "AlbumId",
                table: "Comments",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "CommentType",
                table: "Comments",
                type: "nvarchar(30)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_AlbumTrack_Albums_AlbumId",
                table: "AlbumTrack",
                column: "AlbumId",
                principalTable: "Albums",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tracks_UserProfiles_OwnerId",
                table: "Tracks",
                column: "OwnerId",
                principalTable: "UserProfiles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AlbumTrack_Albums_AlbumId",
                table: "AlbumTrack");

            migrationBuilder.DropForeignKey(
                name: "FK_Tracks_UserProfiles_OwnerId",
                table: "Tracks");

            migrationBuilder.DropColumn(
                name: "CommentType",
                table: "Comments");

            migrationBuilder.AlterColumn<int>(
                name: "AlbumId",
                table: "Comments",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AlbumTrack_Albums_AlbumId",
                table: "AlbumTrack",
                column: "AlbumId",
                principalTable: "Albums",
                principalColumn: "Id");

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
