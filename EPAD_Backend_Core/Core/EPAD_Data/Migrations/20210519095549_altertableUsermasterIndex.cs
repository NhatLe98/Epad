using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Data.Migrations
{
    public partial class altertableUsermasterIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FaceIndex",
                table: "IC_UserMaster",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FaceIndex",
                table: "IC_UserMaster");
        }
    }
}
