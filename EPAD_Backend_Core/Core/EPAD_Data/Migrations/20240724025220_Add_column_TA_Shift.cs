using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class Add_column_TA_Shift : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "BreakStartOvernightTime",
                table: "TA_Shift",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "OTStartOvernightTime",
                table: "TA_Shift",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BreakStartOvernightTime",
                table: "TA_Shift");

            migrationBuilder.DropColumn(
                name: "OTStartOvernightTime",
                table: "TA_Shift");
        }
    }
}
