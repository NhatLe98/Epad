using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class updatetableactimezonethursday : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ThurStart3",
                table: "AC_TimeZone",
                newName: "ThursStart3");

            migrationBuilder.RenameColumn(
                name: "ThurStart2",
                table: "AC_TimeZone",
                newName: "ThursStart2");

            migrationBuilder.RenameColumn(
                name: "ThurStart1",
                table: "AC_TimeZone",
                newName: "ThursStart1");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ThursStart3",
                table: "AC_TimeZone",
                newName: "ThurStart3");

            migrationBuilder.RenameColumn(
                name: "ThursStart2",
                table: "AC_TimeZone",
                newName: "ThurStart2");

            migrationBuilder.RenameColumn(
                name: "ThursStart1",
                table: "AC_TimeZone",
                newName: "ThurStart1");
        }
    }
}
