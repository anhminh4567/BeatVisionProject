using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class Third : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserProfiles_User_UserId",
                table: "UserProfiles");

            migrationBuilder.DropIndex(
                name: "IX_UserProfiles_UserId",
                table: "UserProfiles");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "UserProfiles",
                newName: "TotalTrack");

            migrationBuilder.AddColumn<string>(
                name: "AccountStatus",
                table: "UserProfiles",
                type: "nvarchar(30)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "Birthday",
                table: "UserProfiles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Caption",
                table: "UserProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Facebook",
                table: "UserProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Fullname",
                table: "UserProfiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "IdentityId",
                table: "UserProfiles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Instagram",
                table: "UserProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfileBlobPath",
                table: "UserProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SoundCloud",
                table: "UserProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalAlbumn",
                table: "UserProfiles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Youtube",
                table: "UserProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Albums",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OwnerId = table.Column<int>(type: "int", nullable: false),
                    PlayCount = table.Column<int>(type: "int", nullable: false),
                    IsPrivate = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Albums", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Albums_UserProfiles_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CartItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    ItemType = table.Column<string>(type: "nvarchar(30)", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CartItems_UserProfiles_UserId",
                        column: x => x.UserId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MessageName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(30)", nullable: false),
                    CreatorId = table.Column<int>(type: "int", nullable: false),
                    IsServerNotification = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Messages_UserProfiles_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayLists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PlayCount = table.Column<int>(type: "int", nullable: false),
                    OwnerId = table.Column<int>(type: "int", nullable: false),
                    IsPrivate = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayLists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayLists_UserProfiles_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrackLicenses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LicenceName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsWAVSupported = table.Column<bool>(type: "bit", nullable: false),
                    IsMP3Supported = table.Column<bool>(type: "bit", nullable: false),
                    DefaultPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CurrentPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DistributionLimit = table.Column<int>(type: "int", nullable: false),
                    StreamLimit = table.Column<int>(type: "int", nullable: false),
                    IsProducerTagged = table.Column<bool>(type: "bit", nullable: false),
                    LicensePdfBlobPath = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackLicenses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tracks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TrackName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OwnerId = table.Column<int>(type: "int", nullable: false),
                    SecondLenghth = table.Column<int>(type: "int", nullable: false),
                    PlayCount = table.Column<int>(type: "int", nullable: false),
                    IsPrivate = table.Column<bool>(type: "bit", nullable: false),
                    IsRemoved = table.Column<bool>(type: "bit", nullable: false),
                    IsForSale = table.Column<bool>(type: "bit", nullable: false),
                    AudioBlobPath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BannerBlobPath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tracks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tracks_UserProfiles_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserProfileUserProfile",
                columns: table => new
                {
                    FollowersId = table.Column<int>(type: "int", nullable: false),
                    FollowingsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfileUserProfile", x => new { x.FollowersId, x.FollowingsId });
                    table.ForeignKey(
                        name: "FK_UserProfileUserProfile_UserProfiles_FollowersId",
                        column: x => x.FollowersId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserProfileUserProfile_UserProfiles_FollowingsId",
                        column: x => x.FollowingsId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    ReceiverId = table.Column<int>(type: "int", nullable: false),
                    MessageId = table.Column<int>(type: "int", nullable: false),
                    IsImportant = table.Column<bool>(type: "bit", nullable: false),
                    IsReaded = table.Column<bool>(type: "bit", nullable: false),
                    ExpiredDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => new { x.MessageId, x.ReceiverId });
                    table.ForeignKey(
                        name: "FK_Notifications_Messages_MessageId",
                        column: x => x.MessageId,
                        principalTable: "Messages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Notifications_UserProfiles_ReceiverId",
                        column: x => x.ReceiverId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AlbumTag",
                columns: table => new
                {
                    AlbumsId = table.Column<int>(type: "int", nullable: false),
                    tagsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlbumTag", x => new { x.AlbumsId, x.tagsId });
                    table.ForeignKey(
                        name: "FK_AlbumTag_Albums_AlbumsId",
                        column: x => x.AlbumsId,
                        principalTable: "Albums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlbumTag_Tags_tagsId",
                        column: x => x.tagsId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AlbumTrack",
                columns: table => new
                {
                    TrackId = table.Column<int>(type: "int", nullable: false),
                    AlbumId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlbumTrack", x => new { x.TrackId, x.AlbumId });
                    table.ForeignKey(
                        name: "FK_AlbumTrack_Albums_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Albums",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AlbumTrack_Tracks_TrackId",
                        column: x => x.TrackId,
                        principalTable: "Tracks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AuthorId = table.Column<int>(type: "int", nullable: false),
                    LikesCount = table.Column<int>(type: "int", nullable: false),
                    TrackId = table.Column<int>(type: "int", nullable: true),
                    AlbumId = table.Column<int>(type: "int", nullable: false),
                    ReplyToCommentId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comments_Albums_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Albums",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Comments_Comments_ReplyToCommentId",
                        column: x => x.ReplyToCommentId,
                        principalTable: "Comments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Comments_Tracks_TrackId",
                        column: x => x.TrackId,
                        principalTable: "Tracks",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Comments_UserProfiles_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayListTrack",
                columns: table => new
                {
                    PlayListsId = table.Column<int>(type: "int", nullable: false),
                    TracksId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayListTrack", x => new { x.PlayListsId, x.TracksId });
                    table.ForeignKey(
                        name: "FK_PlayListTrack_PlayLists_PlayListsId",
                        column: x => x.PlayListsId,
                        principalTable: "PlayLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayListTrack_Tracks_TracksId",
                        column: x => x.TracksId,
                        principalTable: "Tracks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TagTrack",
                columns: table => new
                {
                    TagsId = table.Column<int>(type: "int", nullable: false),
                    TracksId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagTrack", x => new { x.TagsId, x.TracksId });
                    table.ForeignKey(
                        name: "FK_TagTrack_Tags_TagsId",
                        column: x => x.TagsId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TagTrack_Tracks_TracksId",
                        column: x => x.TracksId,
                        principalTable: "Tracks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrackTrackLicense",
                columns: table => new
                {
                    LicensesId = table.Column<int>(type: "int", nullable: false),
                    TracksRelatedId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackTrackLicense", x => new { x.LicensesId, x.TracksRelatedId });
                    table.ForeignKey(
                        name: "FK_TrackTrackLicense_TrackLicenses_LicensesId",
                        column: x => x.LicensesId,
                        principalTable: "TrackLicenses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrackTrackLicense_Tracks_TracksRelatedId",
                        column: x => x.TracksRelatedId,
                        principalTable: "Tracks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_IdentityId",
                table: "UserProfiles",
                column: "IdentityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Albums_OwnerId",
                table: "Albums",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_AlbumTag_tagsId",
                table: "AlbumTag",
                column: "tagsId");

            migrationBuilder.CreateIndex(
                name: "IX_AlbumTrack_AlbumId",
                table: "AlbumTrack",
                column: "AlbumId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_UserId",
                table: "CartItems",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_AlbumId",
                table: "Comments",
                column: "AlbumId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_AuthorId",
                table: "Comments",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_ReplyToCommentId",
                table: "Comments",
                column: "ReplyToCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_TrackId",
                table: "Comments",
                column: "TrackId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_CreatorId",
                table: "Messages",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ReceiverId",
                table: "Notifications",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayLists_OwnerId",
                table: "PlayLists",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayListTrack_TracksId",
                table: "PlayListTrack",
                column: "TracksId");

            migrationBuilder.CreateIndex(
                name: "IX_TagTrack_TracksId",
                table: "TagTrack",
                column: "TracksId");

            migrationBuilder.CreateIndex(
                name: "IX_Tracks_OwnerId",
                table: "Tracks",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackTrackLicense_TracksRelatedId",
                table: "TrackTrackLicense",
                column: "TracksRelatedId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfileUserProfile_FollowingsId",
                table: "UserProfileUserProfile",
                column: "FollowingsId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserProfiles_User_IdentityId",
                table: "UserProfiles",
                column: "IdentityId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserProfiles_User_IdentityId",
                table: "UserProfiles");

            migrationBuilder.DropTable(
                name: "AlbumTag");

            migrationBuilder.DropTable(
                name: "AlbumTrack");

            migrationBuilder.DropTable(
                name: "CartItems");

            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "PlayListTrack");

            migrationBuilder.DropTable(
                name: "TagTrack");

            migrationBuilder.DropTable(
                name: "TrackTrackLicense");

            migrationBuilder.DropTable(
                name: "UserProfileUserProfile");

            migrationBuilder.DropTable(
                name: "Albums");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "PlayLists");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "TrackLicenses");

            migrationBuilder.DropTable(
                name: "Tracks");

            migrationBuilder.DropIndex(
                name: "IX_UserProfiles_IdentityId",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "AccountStatus",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "Birthday",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "Caption",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "Facebook",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "Fullname",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "IdentityId",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "Instagram",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "ProfileBlobPath",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "SoundCloud",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "TotalAlbumn",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "Youtube",
                table: "UserProfiles");

            migrationBuilder.RenameColumn(
                name: "TotalTrack",
                table: "UserProfiles",
                newName: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_UserId",
                table: "UserProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserProfiles_User_UserId",
                table: "UserProfiles",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
