using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class P22107updatetableIC_Audit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DescriptionEn",
                table: "IC_Audit",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IC_SystemCommandIndex",
                table: "IC_Audit",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "IC_Audit",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumFailure",
                table: "IC_Audit",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumSuccess",
                table: "IC_Audit",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PageName",
                table: "IC_Audit",
                type: "varchar(250)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "IC_Audit",
                type: "varchar(100)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_IC_Audit_IC_SystemCommandIndex",
                table: "IC_Audit",
                column: "IC_SystemCommandIndex");

            migrationBuilder.AddForeignKey(
                name: "FK_IC_Audit_IC_SystemCommand_IC_SystemCommandIndex",
                table: "IC_Audit",
                column: "IC_SystemCommandIndex",
                principalTable: "IC_SystemCommand",
                principalColumn: "Index",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IC_Audit_IC_SystemCommand_IC_SystemCommandIndex",
                table: "IC_Audit");

            migrationBuilder.DropIndex(
                name: "IX_IC_Audit_IC_SystemCommandIndex",
                table: "IC_Audit");

            migrationBuilder.DropColumn(
                name: "DescriptionEn",
                table: "IC_Audit");

            migrationBuilder.DropColumn(
                name: "IC_SystemCommandIndex",
                table: "IC_Audit");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "IC_Audit");

            migrationBuilder.DropColumn(
                name: "NumFailure",
                table: "IC_Audit");

            migrationBuilder.DropColumn(
                name: "NumSuccess",
                table: "IC_Audit");

            migrationBuilder.DropColumn(
                name: "PageName",
                table: "IC_Audit");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "IC_Audit");
        }
    }
}
