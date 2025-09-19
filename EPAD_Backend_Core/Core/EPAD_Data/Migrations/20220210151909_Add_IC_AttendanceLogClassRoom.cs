using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class Add_IC_AttendanceLogClassRoom : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IC_Employee");

            migrationBuilder.CreateTable(
                name: "IC_AttendanceLogClassRoom",
                columns: table => new
                {
                    EmployeeATID = table.Column<string>(type: "varchar(100)", nullable: false),
                    RoomName = table.Column<string>(type: "varchar(150)", nullable: false),
                    CheckTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false),
                    VerifyMode = table.Column<short>(type: "smallint", nullable: true),
                    InOutMode = table.Column<short>(type: "smallint", nullable: false),
                    WorkCode = table.Column<int>(type: "int", nullable: true),
                    Reserve1 = table.Column<int>(type: "int", nullable: true),
                    FaceMask = table.Column<int>(type: "int", nullable: true),
                    BodyTemperature = table.Column<double>(type: "float", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedUser = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Function = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IC_AttendanceLogClassRoom", x => new { x.EmployeeATID, x.CompanyIndex, x.CheckTime, x.RoomName });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IC_AttendanceLogClassRoom");

            migrationBuilder.CreateTable(
                name: "IC_Employee",
                columns: table => new
                {
                    EmployeeATID = table.Column<string>(type: "varchar(100)", nullable: false),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false),
                    CardNumber = table.Column<string>(type: "varchar(30)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DepartmentIndex = table.Column<int>(type: "int", nullable: false),
                    EmployeeCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Gender = table.Column<short>(type: "smallint", nullable: true),
                    JoinedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NameOnMachine = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    StoppedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedUser = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IC_Employee", x => new { x.EmployeeATID, x.CompanyIndex });
                });
        }
    }
}
