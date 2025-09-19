using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class addAuditDescriptionColum : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "IC_Audit",
                maxLength: 1000,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "IC_Audit");
        }
    }
}
