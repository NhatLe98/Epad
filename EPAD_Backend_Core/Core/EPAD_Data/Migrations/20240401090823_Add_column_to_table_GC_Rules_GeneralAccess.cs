using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class Add_column_to_table_GC_Rules_GeneralAccess : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AllowCheckOutInWorkingTimeRange",
                table: "GC_Rules_GeneralAccess",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowCheckOutInWorkingTimeRange",
                table: "GC_Rules_GeneralAccess");
        }
    }
}
