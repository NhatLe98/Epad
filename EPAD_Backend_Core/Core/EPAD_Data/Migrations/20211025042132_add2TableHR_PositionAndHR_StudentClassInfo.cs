using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class add2TableHR_PositionAndHR_StudentClassInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HR_Postion",
                columns: table => new
                {
                    Index = table.Column<long>(type: "bigint", nullable: false),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NameInEng = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaxOverTimeInYear = table.Column<float>(type: "real", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HR_Postion", x => new { x.Index, x.CompanyIndex });
                });

            migrationBuilder.CreateTable(
                name: "HR_StudentClassInfo",
                columns: table => new
                {
                    ClassInfoIndex = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TeacherID = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StudentID = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NannyID = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FromDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ToDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedUser = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HR_StudentClassInfo", x => new { x.StudentID, x.TeacherID, x.ClassInfoIndex });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HR_Postion");

            migrationBuilder.DropTable(
                name: "HR_StudentClassInfo");
        }
    }
}
