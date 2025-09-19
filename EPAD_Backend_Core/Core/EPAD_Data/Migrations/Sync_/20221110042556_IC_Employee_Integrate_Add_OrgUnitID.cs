using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Data.Migrations.Sync_
{
    public partial class IC_Employee_Integrate_Add_OrgUnitID : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrgUnitID",
                table: "IC_Employee_Integrate",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OrgUnitParentNode",
                table: "IC_Employee_Integrate",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrgUnitID",
                table: "IC_Employee_Integrate");

            migrationBuilder.DropColumn(
                name: "OrgUnitParentNode",
                table: "IC_Employee_Integrate");
        }
    }
}
