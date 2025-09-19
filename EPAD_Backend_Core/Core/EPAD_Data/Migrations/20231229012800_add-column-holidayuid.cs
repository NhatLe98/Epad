using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class addcolumnholidayuid : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HolidayUID",
                table: "AC_AccHoliday",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HolidayUID",
                table: "AC_AccHoliday");
        }
    }
}
