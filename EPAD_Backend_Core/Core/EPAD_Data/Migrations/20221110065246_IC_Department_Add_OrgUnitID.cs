using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class IC_Department_Add_OrgUnitID : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrgUnitID",
                table: "IC_Department",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrgUnitParentNode",
                table: "IC_Department",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrgUnitID",
                table: "IC_Department");

            migrationBuilder.DropColumn(
                name: "OrgUnitParentNode",
                table: "IC_Department");
        }
    }
}
