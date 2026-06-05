using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Farmacontrol.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddReceivedBatchesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReceivedBatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PurchaseDetailId = table.Column<int>(type: "INTEGER", nullable: false),
                    LotCode = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    ManufacturingDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UnitCost = table.Column<decimal>(type: "TEXT", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExistingBatchId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceivedBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReceivedBatches_PurchaseDetails_PurchaseDetailId",
                        column: x => x.PurchaseDetailId,
                        principalTable: "PurchaseDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReceivedBatches_ExpirationDate",
                table: "ReceivedBatches",
                column: "ExpirationDate");

            migrationBuilder.CreateIndex(
                name: "IX_ReceivedBatches_LotCode",
                table: "ReceivedBatches",
                column: "LotCode");

            migrationBuilder.CreateIndex(
                name: "IX_ReceivedBatches_PurchaseDetailId",
                table: "ReceivedBatches",
                column: "PurchaseDetailId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReceivedBatches");
        }
    }
}
