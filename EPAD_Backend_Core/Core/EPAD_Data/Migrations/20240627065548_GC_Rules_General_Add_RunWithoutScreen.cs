using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class GC_Rules_General_Add_RunWithoutScreen : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IgnoreInLog",
                table: "GC_Rules_General",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RunWithoutScreen",
                table: "GC_Rules_General",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IgnoreInLog",
                table: "GC_Rules_General");

            migrationBuilder.DropColumn(
                name: "RunWithoutScreen",
                table: "GC_Rules_General");
        }
    }
}
