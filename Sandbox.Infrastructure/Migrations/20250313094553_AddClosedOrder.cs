using System;
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
                name: "FK_ClosedPosition_Orders_StopLossOrderId",
                table: "ClosedPosition");

            migrationBuilder.DropForeignKey(
                name: "FK_ClosedPosition_Orders_TakeProfitOrderId",
                table: "ClosedPosition");

            migrationBuilder.DropIndex(
                name: "IX_ClosedPosition_StopLossOrderId",
                table: "ClosedPosition");

            migrationBuilder.DropIndex(
                name: "IX_ClosedPosition_TakeProfitOrderId",
                table: "ClosedPosition");

            migrationBuilder.DropColumn(
                name: "StopLossOrderId",
                table: "ClosedPosition");

            migrationBuilder.DropColumn(
                name: "TakeProfitOrderId",
                table: "ClosedPosition");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "StopLossOrderId",
                table: "ClosedPosition",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TakeProfitOrderId",
                table: "ClosedPosition",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClosedPosition_StopLossOrderId",
                table: "ClosedPosition",
                column: "StopLossOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ClosedPosition_TakeProfitOrderId",
                table: "ClosedPosition",
                column: "TakeProfitOrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClosedPosition_Orders_StopLossOrderId",
                table: "ClosedPosition",
                column: "StopLossOrderId",
                principalTable: "Orders",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ClosedPosition_Orders_TakeProfitOrderId",
                table: "ClosedPosition",
                column: "TakeProfitOrderId",
                principalTable: "Orders",
                principalColumn: "Id");
        }
    }
}
