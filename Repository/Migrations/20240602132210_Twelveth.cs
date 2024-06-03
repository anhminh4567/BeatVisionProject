using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class Twelveth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentPrice",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "IsSale",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "TrackLength",
                table: "OrderItems");

            migrationBuilder.RenameColumn(
                name: "OrignalTrackId",
                table: "OrderItems",
                newName: "TrackId");

            migrationBuilder.AlterColumn<int>(
                name: "OriginalPrice",
                table: "Orders",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateDate",
                table: "Orders",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelAt",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancellationReasons",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Price",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PricePaid",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PriceRemain",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OrderTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<int>(type: "int", nullable: false),
                    AccountNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TransactionDateTime = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VirtualAccountName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VirtualAccountNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CounterAccountBankId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CounterAccountBankName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CounterAccountName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CounterAccountNumber = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderTransactions_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_TrackId",
                table: "OrderItems",
                column: "TrackId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderTransactions_OrderId",
                table: "OrderTransactions",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Tracks_TrackId",
                table: "OrderItems",
                column: "TrackId",
                principalTable: "Tracks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Tracks_TrackId",
                table: "OrderItems");

            migrationBuilder.DropTable(
                name: "OrderTransactions");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_TrackId",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "CancelAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CancellationReasons",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PricePaid",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PriceRemain",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "TrackId",
                table: "OrderItems",
                newName: "OrignalTrackId");

            migrationBuilder.AlterColumn<decimal>(
                name: "OriginalPrice",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateDate",
                table: "Orders",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CurrentPrice",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsSale",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TrackLength",
                table: "OrderItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
