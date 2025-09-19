using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class GC_Employee_Accessed_Group : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GC_Employee_AccessedGroup",
                columns: table => new
                {
                    EmployeeATID = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FromDate = table.Column<DateTime>(type: "smalldatetime", nullable: false),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false),
                    ToDate = table.Column<DateTime>(type: "smalldatetime", nullable: true),
                    AccessedGroupIndex = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedUser = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GC_Employee_AccessedGroup", x => new { x.CompanyIndex, x.EmployeeATID, x.FromDate });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GC_Employee_AccessedGroup");
        }
    }
}
