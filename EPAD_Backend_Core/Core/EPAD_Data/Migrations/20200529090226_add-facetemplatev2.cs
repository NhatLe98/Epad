using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Data.Migrations
{
    public partial class addfacetemplatev2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IC_UserFaceTemplate_v2",
                columns: table => new
                {
                    EmployeeATID = table.Column<string>(type: "varchar(10)", nullable: false),
                    SerialNumber = table.Column<string>(type: "varchar(50)", nullable: false),
                    CompanyIndex = table.Column<int>(nullable: false),
                    No = table.Column<int>(nullable: false),
                    Index = table.Column<int>(nullable: false),
                    Valid = table.Column<int>(nullable: false),
                    Duress = table.Column<int>(nullable: false),
                    Type = table.Column<int>(nullable: false),
                    MajorVer = table.Column<int>(nullable: false),
                    MinorVer = table.Column<int>(nullable: false),
                    Format = table.Column<int>(nullable: false),
                    TemplateBIODATA = table.Column<string>(type: "text", nullable: true),
                    Size = table.Column<int>(nullable: false),
                    Content = table.Column<string>(type: "text", nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedUser = table.Column<string>(maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IC_UserFaceTemplate_v2", x => new { x.EmployeeATID, x.CompanyIndex, x.SerialNumber });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IC_UserFaceTemplate_v2");
        }
    }
}
