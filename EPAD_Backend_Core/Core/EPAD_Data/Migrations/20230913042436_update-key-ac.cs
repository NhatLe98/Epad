using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class updatekeyac : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AC_AccGroup");

            migrationBuilder.DropTable(
                name: "AC_AccHoliday");

            migrationBuilder.DropTable(
                name: "AC_TimeZone");

            migrationBuilder.CreateTable(
                name: "AC_Area",
                columns: table => new
                {
                    Index = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false),
                    UpdatedUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AC_Area", x => x.Index);
                });

            migrationBuilder.CreateTable(
                name: "AC_AreaAndDoor",
                columns: table => new
                {
                    AreaIndex = table.Column<int>(type: "int", nullable: false),
                    DoorIndex = table.Column<int>(type: "int", nullable: false),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false),
                    UpdatedUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AC_AreaAndDoor", x => new { x.AreaIndex, x.DoorIndex });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AC_Area");

            migrationBuilder.DropTable(
                name: "AC_AreaAndDoor");

            migrationBuilder.CreateTable(
                name: "AC_AccGroup",
                columns: table => new
                {
                    SerialNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UID = table.Column<int>(type: "int", nullable: false),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false),
                    TimeZone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValidHoliday = table.Column<bool>(type: "bit", nullable: false),
                    Verify = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AC_AccGroup", x => new { x.SerialNumber, x.UID });
                });

            migrationBuilder.CreateTable(
                name: "AC_AccHoliday",
                columns: table => new
                {
                    SerialNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UID = table.Column<int>(type: "int", nullable: false),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HolidayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TimeZone = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedUser = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AC_AccHoliday", x => new { x.SerialNumber, x.UID });
                });

            migrationBuilder.CreateTable(
                name: "AC_TimeZone",
                columns: table => new
                {
                    SerialNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UID = table.Column<int>(type: "int", nullable: false),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false),
                    FriEnd = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FriStart = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MonEnd = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MonStart = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SatEnd = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SatStart = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SunEnd = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SunStart = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThurStart = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThursEnd = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TuesEnd = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TuesStart = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WedEnd = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WedStart = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AC_TimeZone", x => new { x.SerialNumber, x.UID });
                });
        }
    }
}
