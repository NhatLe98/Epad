using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class Add_column_table_GC_Rules_GeneralAccess : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MissionMaxEarlyCheckOutMinute",
                table: "GC_Rules_GeneralAccess",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MissionMaxLateCheckInMinute",
                table: "GC_Rules_GeneralAccess",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MissionMaxEarlyCheckOutMinute",
                table: "GC_Rules_GeneralAccess");

            migrationBuilder.DropColumn(
                name: "MissionMaxLateCheckInMinute",
                table: "GC_Rules_GeneralAccess");
        }
    }
}
