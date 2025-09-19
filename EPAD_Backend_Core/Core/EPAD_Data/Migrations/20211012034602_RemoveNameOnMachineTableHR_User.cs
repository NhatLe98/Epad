using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class RemoveNameOnMachineTableHR_User : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NameOnMachine",
                table: "HR_User");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NameOnMachine",
                table: "HR_User",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }
    }
}
