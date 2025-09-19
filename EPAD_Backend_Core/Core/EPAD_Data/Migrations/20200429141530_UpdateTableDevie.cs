using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Data.Migrations
{
    public partial class UpdateTableDevie : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AdminCount",
                table: "IC_Device",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FaceCount",
                table: "IC_Device",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminCount",
                table: "IC_Device");

            migrationBuilder.DropColumn(
                name: "FaceCount",
                table: "IC_Device");
        }
    }
}
