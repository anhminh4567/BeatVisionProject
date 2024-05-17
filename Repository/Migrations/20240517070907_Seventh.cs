using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class Seventh : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "AudioLenghtSeconds",
                table: "Tracks",
                type: "float",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "AudioBitPerSample",
                table: "Tracks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AudioChannels",
                table: "Tracks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AudioSampleRate",
                table: "Tracks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublished",
                table: "Tracks",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PublishDateTime",
                table: "Tracks",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "GeneratedName",
                table: "BlobFileData",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsPaidContent",
                table: "BlobFileData",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OriginalFileName",
                table: "BlobFileData",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AudioBitPerSample",
                table: "Tracks");

            migrationBuilder.DropColumn(
                name: "AudioChannels",
                table: "Tracks");

            migrationBuilder.DropColumn(
                name: "AudioSampleRate",
                table: "Tracks");

            migrationBuilder.DropColumn(
                name: "IsPublished",
                table: "Tracks");

            migrationBuilder.DropColumn(
                name: "PublishDateTime",
                table: "Tracks");

            migrationBuilder.DropColumn(
                name: "GeneratedName",
                table: "BlobFileData");

            migrationBuilder.DropColumn(
                name: "IsPaidContent",
                table: "BlobFileData");

            migrationBuilder.DropColumn(
                name: "OriginalFileName",
                table: "BlobFileData");

            migrationBuilder.AlterColumn<int>(
                name: "AudioLenghtSeconds",
                table: "Tracks",
                type: "int",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");
        }
    }
}
