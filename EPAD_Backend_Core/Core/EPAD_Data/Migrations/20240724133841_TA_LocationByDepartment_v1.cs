using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class TA_LocationByDepartment_v1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "TA_LocationByDepartment");

            migrationBuilder.RenameColumn(
                name: "LocationName",
                table: "TA_LocationByDepartment",
                newName: "LocationIndex");

            migrationBuilder.RenameColumn(
                name: "DepartmentName",
                table: "TA_LocationByDepartment",
                newName: "DepartmentIndex");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LocationIndex",
                table: "TA_LocationByDepartment",
                newName: "LocationName");

            migrationBuilder.RenameColumn(
                name: "DepartmentIndex",
                table: "TA_LocationByDepartment",
                newName: "DepartmentName");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "TA_LocationByDepartment",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
