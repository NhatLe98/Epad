using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class updatecolumnIsGuestDefaultGroupGC_AccessedGroup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDriverDefaultGroup",
                table: "GC_AccessedGroup",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsGuestDefaultGroup",
                table: "GC_AccessedGroup",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDriverDefaultGroup",
                table: "GC_AccessedGroup");

            migrationBuilder.DropColumn(
                name: "IsGuestDefaultGroup",
                table: "GC_AccessedGroup");
        }
    }
}
