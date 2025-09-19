using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Data.Migrations
{
    public partial class remove3table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IC_UserFaceMaster");

            migrationBuilder.DropTable(
                name: "IC_UserFaceV2Master");

            migrationBuilder.DropTable(
                name: "IC_UserFingerMaster");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IC_UserFaceMaster",
                columns: table => new
                {
                    EmployeeATID = table.Column<string>(type: "varchar(10)", nullable: false),
                    CompanyIndex = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    FaceIndex = table.Column<short>(nullable: false),
                    FaceTemplate = table.Column<string>(type: "text", nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedUser = table.Column<string>(maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IC_UserFaceMaster", x => new { x.EmployeeATID, x.CompanyIndex });
                });

            migrationBuilder.CreateTable(
                name: "IC_UserFaceV2Master",
                columns: table => new
                {
                    EmployeeATID = table.Column<string>(type: "varchar(10)", nullable: false),
                    CompanyIndex = table.Column<int>(nullable: false),
                    Content = table.Column<string>(type: "text", nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    Duress = table.Column<int>(nullable: false),
                    Format = table.Column<int>(nullable: false),
                    Index = table.Column<int>(nullable: false),
                    MajorVer = table.Column<int>(nullable: false),
                    MinorVer = table.Column<int>(nullable: false),
                    No = table.Column<int>(nullable: false),
                    Size = table.Column<int>(nullable: false),
                    TemplateBIODATA = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<int>(nullable: false),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedUser = table.Column<string>(maxLength: 50, nullable: true),
                    Valid = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IC_UserFaceV2Master", x => new { x.EmployeeATID, x.CompanyIndex });
                });

            migrationBuilder.CreateTable(
                name: "IC_UserFingerMaster",
                columns: table => new
                {
                    EmployeeATID = table.Column<string>(type: "varchar(10)", nullable: false),
                    CompanyIndex = table.Column<int>(nullable: false),
                    FingerIndex = table.Column<short>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    FingerData = table.Column<string>(type: "varchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedUser = table.Column<string>(maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IC_UserFingerMaster", x => new { x.EmployeeATID, x.CompanyIndex, x.FingerIndex });
                });
        }
    }
}
