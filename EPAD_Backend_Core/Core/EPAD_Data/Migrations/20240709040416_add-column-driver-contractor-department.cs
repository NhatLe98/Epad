using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class addcolumndrivercontractordepartment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsContractorDepartment",
                table: "IC_Department",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDriverDepartment",
                table: "IC_Department",
                type: "bit",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsContractorDepartment",
                table: "IC_Department");

            migrationBuilder.DropColumn(
                name: "IsDriverDepartment",
                table: "IC_Department");
        }
    }
}
