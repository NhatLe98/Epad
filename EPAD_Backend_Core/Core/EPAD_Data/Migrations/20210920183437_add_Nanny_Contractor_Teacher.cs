using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class add_Nanny_Contractor_Teacher : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HR_ContractorInfo",
                columns: table => new
                {
                    EmployeeATID = table.Column<string>(type: "varchar(100)", nullable: false),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false),
                    NRIC = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Company = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ContactPerson = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    FromTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ToTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedUser = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HR_ContractorInfo", x => new { x.EmployeeATID, x.CompanyIndex });
                });

            migrationBuilder.CreateTable(
                name: "HR_NannyInfo",
                columns: table => new
                {
                    EmployeeATID = table.Column<string>(type: "varchar(100)", nullable: false),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false),
                    ClassID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedUser = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HR_NannyInfo", x => new { x.EmployeeATID, x.CompanyIndex });
                });

            migrationBuilder.CreateTable(
                name: "HR_TeacherInfo",
                columns: table => new
                {
                    EmployeeATID = table.Column<string>(type: "varchar(100)", nullable: false),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false),
                    ClassID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedUser = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HR_TeacherInfo", x => new { x.EmployeeATID, x.CompanyIndex });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HR_ContractorInfo");

            migrationBuilder.DropTable(
                name: "HR_NannyInfo");

            migrationBuilder.DropTable(
                name: "HR_TeacherInfo");
        }
    }
}
