using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class Add_tables_relate_to_Dorm : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HR_DormActivity",
                columns: table => new
                {
                    Index = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedUser = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false, defaultValueSql: "((2))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HR_DormActivity", x => x.Index);
                });

            migrationBuilder.CreateTable(
                name: "HR_DormLeaveType",
                columns: table => new
                {
                    Index = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedUser = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false, defaultValueSql: "((2))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HR_DormLeaveType", x => x.Index);
                });

            migrationBuilder.CreateTable(
                name: "HR_DormRation",
                columns: table => new
                {
                    Index = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedUser = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false, defaultValueSql: "((2))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HR_DormRation", x => x.Index);
                });

            migrationBuilder.CreateTable(
                name: "HR_DormRegister",
                columns: table => new
                {
                    Index = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeATID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FromDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ToDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StayInDorm = table.Column<bool>(type: "bit", nullable: false),
                    DormRoomIndex = table.Column<int>(type: "int", nullable: false),
                    DormEmployeeCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DormLeaveIndex = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedUser = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false, defaultValueSql: "((2))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HR_DormRegister", x => x.Index);
                });

            migrationBuilder.CreateTable(
                name: "HR_DormRegister_Activity",
                columns: table => new
                {
                    DormRegisterIndex = table.Column<int>(type: "int", nullable: false),
                    DormActivityIndex = table.Column<int>(type: "int", nullable: false),
                    DormAccessMode = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedUser = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false, defaultValueSql: "((2))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HR_DormRegister_Activity", x => new { x.DormRegisterIndex, x.DormActivityIndex, x.DormAccessMode });
                });

            migrationBuilder.CreateTable(
                name: "HR_DormRegister_Ration",
                columns: table => new
                {
                    DormRegisterIndex = table.Column<int>(type: "int", nullable: false),
                    DormRationIndex = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedUser = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false, defaultValueSql: "((2))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HR_DormRegister_Ration", x => new { x.DormRegisterIndex, x.DormRationIndex });
                });

            migrationBuilder.CreateTable(
                name: "HR_DormRoom",
                columns: table => new
                {
                    Index = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FloorLevelIndex = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedUser = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false, defaultValueSql: "((2))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HR_DormRoom", x => x.Index);
                });

            migrationBuilder.CreateTable(
                name: "HR_FloorLevel",
                columns: table => new
                {
                    Index = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedUser = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false, defaultValueSql: "((2))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HR_FloorLevel", x => x.Index);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HR_DormActivity");

            migrationBuilder.DropTable(
                name: "HR_DormLeaveType");

            migrationBuilder.DropTable(
                name: "HR_DormRation");

            migrationBuilder.DropTable(
                name: "HR_DormRegister");

            migrationBuilder.DropTable(
                name: "HR_DormRegister_Activity");

            migrationBuilder.DropTable(
                name: "HR_DormRegister_Ration");

            migrationBuilder.DropTable(
                name: "HR_DormRoom");

            migrationBuilder.DropTable(
                name: "HR_FloorLevel");
        }
    }
}
