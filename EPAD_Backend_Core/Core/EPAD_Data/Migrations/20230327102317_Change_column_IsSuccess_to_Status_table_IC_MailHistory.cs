using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class Change_column_IsSuccess_to_Status_table_IC_MailHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSuccess",
                table: "IC_MailHistory");

            migrationBuilder.AlterColumn<int>(
                name: "Times",
                table: "IC_MailHistory",
                type: "int",
                nullable: false,
                defaultValueSql: "((0))",
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true,
                oldDefaultValueSql: "((0))");

            migrationBuilder.AddColumn<short>(
                name: "Status",
                table: "IC_MailHistory",
                type: "smallint",
                nullable: false,
                defaultValueSql: "((0))");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "IC_MailHistory");

            migrationBuilder.AlterColumn<int>(
                name: "Times",
                table: "IC_MailHistory",
                type: "int",
                nullable: true,
                defaultValueSql: "((0))",
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValueSql: "((0))");

            migrationBuilder.AddColumn<bool>(
                name: "IsSuccess",
                table: "IC_MailHistory",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
