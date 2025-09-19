using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Data.Migrations
{
    public partial class addtableIC_PrivilegeDepartment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IC_PrivilegeDepartment",
                columns: table => new
                {
                    PrivilegeIndex = table.Column<int>(nullable: false),
                    DepartmentIndex = table.Column<long>(nullable: false),
                    Role = table.Column<string>(maxLength: 20, nullable: false),
                    CompanyIndex = table.Column<int>(nullable: false),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedUser = table.Column<string>(maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IC_PrivilegeDepartment", x => new { x.PrivilegeIndex, x.CompanyIndex, x.DepartmentIndex, x.Role });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IC_PrivilegeDepartment");
        }
    }
}
