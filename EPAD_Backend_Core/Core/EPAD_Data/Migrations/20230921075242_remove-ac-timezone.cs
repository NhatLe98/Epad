using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class removeactimezone : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AC_TimeZone");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AC_TimeZone",
                columns: table => new
                {
                    UID = table.Column<int>(type: "int", nullable: false),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false),
                    FriEnd = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FriStart = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MonEnd = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MonStart = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
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
                    table.PrimaryKey("PK_AC_TimeZone", x => x.UID);
                });
        }
    }
}
