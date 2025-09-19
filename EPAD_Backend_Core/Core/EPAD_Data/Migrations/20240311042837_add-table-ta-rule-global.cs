using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class addtabletaruleglobal : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TA_Rules_Global",
                columns: table => new
                {
                    Index = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false),
                    LockAttendanceTime = table.Column<int>(type: "int", nullable: false),
                    OverTimeNormalDay = table.Column<int>(type: "int", nullable: false),
                    NightOverTimeNormalDay = table.Column<int>(type: "int", nullable: false),
                    OverTimeLeaveDay = table.Column<int>(type: "int", nullable: false),
                    NightOverTimeLeaveDay = table.Column<int>(type: "int", nullable: false),
                    OverTimeHoliday = table.Column<int>(type: "int", nullable: false),
                    NightOverTimeHoliday = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TA_Rules_Global", x => x.Index);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TA_Rules_Global");
        }
    }
}
