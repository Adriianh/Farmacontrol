using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Farmacontrol.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusToPurchase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Purchases",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Purchases");
        }
    }
}
