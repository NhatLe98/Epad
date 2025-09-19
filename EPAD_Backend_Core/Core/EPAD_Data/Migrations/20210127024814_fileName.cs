using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Data.Migrations
{
    public partial class fileName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HR_EmployeeReport",
                columns: table => new
                {
                    EmployeeATID = table.Column<string>(nullable: false),
                    CompanyIndex = table.Column<int>(nullable: false),
                    EmployeeCode = table.Column<string>(nullable: true),
                    CardNumber = table.Column<string>(nullable: true),
                    FullName = table.Column<string>(nullable: true),
                    Gender = table.Column<short>(nullable: true),
                    NameOnMachine = table.Column<string>(nullable: true),
                    DepartmentIndex = table.Column<int>(nullable: true),
                    DepartmentName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HR_EmployeeReport", x => x.EmployeeATID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HR_EmployeeReport");
        }
    }
}
