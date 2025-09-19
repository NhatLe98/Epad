using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class updatetableactimezone : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AC_TimeZone",
                columns: table => new
                {
                    UID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SunStart1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SunEnd1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SunStart2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SunEnd2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SunStart3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SunEnd3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MonStart1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MonEnd1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MonStart2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MonEnd2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MonStart3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MonEnd3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TuesStart1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TuesEnd1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TuesStart2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TuesEnd2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TuesStart3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TuesEnd3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WedStart1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WedEnd1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WedStart2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WedEnd2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WedStart3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WedEnd3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThurStart1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThursEnd1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThurStart2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThursEnd2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThurStart3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThursEnd3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FriStart1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FriEnd1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FriStart2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FriEnd2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FriStart3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FriEnd3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SatStart1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SatEnd1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SatStart2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SatEnd2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SatStart3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SatEnd3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false),
                    UpdatedUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    UIDIndex = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AC_TimeZone", x => x.UID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AC_TimeZone");
        }
    }
}
