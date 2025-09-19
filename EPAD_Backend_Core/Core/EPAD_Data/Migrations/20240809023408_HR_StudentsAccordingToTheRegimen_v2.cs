using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class HR_StudentsAccordingToTheRegimen_v2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EmployeATID",
                table: "HR_StudentsAccordingToTheRegimen",
                newName: "EmployeeATID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EmployeeATID",
                table: "HR_StudentsAccordingToTheRegimen",
                newName: "EmployeATID");
        }
    }
}
