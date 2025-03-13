using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sandbox.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddClosedOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClosedOrder_Wallets_WalletId",
                table: "ClosedOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_ClosedPosition_Wallets_WalletId",
                table: "ClosedPosition");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ClosedPosition",
                table: "ClosedPosition");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ClosedOrder",
                table: "ClosedOrder");

            migrationBuilder.RenameTable(
                name: "ClosedPosition",
                newName: "ClosedPositions");

            migrationBuilder.RenameTable(
                name: "ClosedOrder",
                newName: "ClosedOrders");

            migrationBuilder.RenameIndex(
                name: "IX_ClosedPosition_WalletId",
                table: "ClosedPositions",
                newName: "IX_ClosedPositions_WalletId");

            migrationBuilder.RenameIndex(
                name: "IX_ClosedOrder_WalletId",
                table: "ClosedOrders",
                newName: "IX_ClosedOrders_WalletId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ClosedPositions",
                table: "ClosedPositions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ClosedOrders",
                table: "ClosedOrders",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ClosedOrders_Wallets_WalletId",
                table: "ClosedOrders",
                column: "WalletId",
                principalTable: "Wallets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ClosedPositions_Wallets_WalletId",
                table: "ClosedPositions",
                column: "WalletId",
                principalTable: "Wallets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClosedOrders_Wallets_WalletId",
                table: "ClosedOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_ClosedPositions_Wallets_WalletId",
                table: "ClosedPositions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ClosedPositions",
                table: "ClosedPositions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ClosedOrders",
                table: "ClosedOrders");

            migrationBuilder.RenameTable(
                name: "ClosedPositions",
                newName: "ClosedPosition");

            migrationBuilder.RenameTable(
                name: "ClosedOrders",
                newName: "ClosedOrder");

            migrationBuilder.RenameIndex(
                name: "IX_ClosedPositions_WalletId",
                table: "ClosedPosition",
                newName: "IX_ClosedPosition_WalletId");

            migrationBuilder.RenameIndex(
                name: "IX_ClosedOrders_WalletId",
                table: "ClosedOrder",
                newName: "IX_ClosedOrder_WalletId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ClosedPosition",
                table: "ClosedPosition",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ClosedOrder",
                table: "ClosedOrder",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ClosedOrder_Wallets_WalletId",
                table: "ClosedOrder",
                column: "WalletId",
                principalTable: "Wallets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ClosedPosition_Wallets_WalletId",
                table: "ClosedPosition",
                column: "WalletId",
                principalTable: "Wallets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
