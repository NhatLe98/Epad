using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class TA_ScheduleFixedByEmployee : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CanteenIndex",
                table: "TA_EmployeeShift");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CanteenIndex",
                table: "TA_EmployeeShift",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
