using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class Add_table_TA_LeaveDateType_LeaveRegistration_Holiday_BusinessRegistration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaximumAnnualLeaveRegisterByMonth",
                table: "TA_Rules_Global",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TA_BusinessRegistration",
                columns: table => new
                {
                    Index = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeATID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false, defaultValueSql: "((2))"),
                    UpdatedUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LeaveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BusinessType = table.Column<int>(type: "int", nullable: false),
                    BusinessPlace = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FromTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ToTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TA_BusinessRegistration", x => x.Index);
                });

            migrationBuilder.CreateTable(
                name: "TA_Holiday",
                columns: table => new
                {
                    Index = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsPaidWhenNotWorking = table.Column<bool>(type: "bit", nullable: false),
                    IsRepeatAnnually = table.Column<bool>(type: "bit", nullable: false),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false, defaultValueSql: "((2))"),
                    UpdatedUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TA_Holiday", x => x.Index);
                });

            migrationBuilder.CreateTable(
                name: "TA_LeaveDateType",
                columns: table => new
                {
                    Index = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsWorkedTimeHoliday = table.Column<bool>(type: "bit", nullable: false),
                    IsPaidLeave = table.Column<bool>(type: "bit", nullable: false),
                    IsOptionHoliday = table.Column<bool>(type: "bit", nullable: false),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false, defaultValueSql: "((2))"),
                    UpdatedUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TA_LeaveDateType", x => x.Index);
                });

            migrationBuilder.CreateTable(
                name: "TA_LeaveRegistration",
                columns: table => new
                {
                    Index = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeATID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false, defaultValueSql: "((2))"),
                    UpdatedUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LeaveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LeaveDateType = table.Column<int>(type: "int", nullable: false),
                    LeaveDurationType = table.Column<int>(type: "int", nullable: false),
                    HaftLeaveType = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TA_LeaveRegistration", x => x.Index);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TA_BusinessRegistration");

            migrationBuilder.DropTable(
                name: "TA_Holiday");

            migrationBuilder.DropTable(
                name: "TA_LeaveDateType");

            migrationBuilder.DropTable(
                name: "TA_LeaveRegistration");

            migrationBuilder.DropColumn(
                name: "MaximumAnnualLeaveRegisterByMonth",
                table: "TA_Rules_Global");
        }
    }
}
