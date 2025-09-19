using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class Add_table_GC_TruckExtraDriverLog_and_update_table_GC_Rules_GeneralAccess_GC_TimeLog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IsException",
                table: "GC_TruckDriverLog",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReasonException",
                table: "GC_TruckDriverLog",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IsException",
                table: "GC_TimeLog",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReasonException",
                table: "GC_TimeLog",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AllowFreeInAndOutInTimeRange",
                table: "GC_Rules_GeneralAccess",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsException",
                table: "GC_TruckDriverLog");

            migrationBuilder.DropColumn(
                name: "ReasonException",
                table: "GC_TruckDriverLog");

            migrationBuilder.DropColumn(
                name: "IsException",
                table: "GC_TimeLog");

            migrationBuilder.DropColumn(
                name: "ReasonException",
                table: "GC_TimeLog");

            migrationBuilder.DropColumn(
                name: "AllowFreeInAndOutInTimeRange",
                table: "GC_Rules_GeneralAccess");
        }
    }
}
