using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Data.Migrations
{
    public partial class renameColumnOverwriteData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsOrverwriteData",
                table: "IC_SystemCommand",
                newName: "IsOverwriteData");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsOverwriteData",
                table: "IC_SystemCommand",
                newName: "IsOrverwriteData");
        }
    }
}
