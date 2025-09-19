using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class Update_table_TA_Rules_Shift_InOut : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompanyIndex",
                table: "TA_Rules_Shift_InOut",
                type: "int",
                nullable: false,
                defaultValueSql: "((2))");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TA_Rules_Shift_InOut",
                table: "TA_Rules_Shift_InOut",
                columns: new[] { "RuleShiftIndex", "CompanyIndex", "FromTime", "ToTime" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TA_Rules_Shift_InOut",
                table: "TA_Rules_Shift_InOut");

            migrationBuilder.DropColumn(
                name: "CompanyIndex",
                table: "TA_Rules_Shift_InOut");
        }
    }
}
