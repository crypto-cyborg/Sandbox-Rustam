using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sandbox.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TrailingStop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "StopLossPrice",
                table: "Positions",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TrailingStopDistance",
                table: "Positions",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StopLossPrice",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "TrailingStopDistance",
                table: "Positions");
        }
    }
}
