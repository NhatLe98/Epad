using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class Add_column_table_GC_Lines_and_GC_TruckDriverLog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsInactive",
                table: "GC_TruckDriverLog",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "LineForCustomerIssuanceReturnCard",
                table: "GC_Lines",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "LineForDriverIssuanceReturnCard",
                table: "GC_Lines",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsInactive",
                table: "GC_TruckDriverLog");

            migrationBuilder.DropColumn(
                name: "LineForCustomerIssuanceReturnCard",
                table: "GC_Lines");

            migrationBuilder.DropColumn(
                name: "LineForDriverIssuanceReturnCard",
                table: "GC_Lines");
        }
    }
}
