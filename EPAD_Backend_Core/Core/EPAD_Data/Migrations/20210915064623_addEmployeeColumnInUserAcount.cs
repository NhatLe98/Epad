using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class addEmployeeColumnInUserAcount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmployeeATID",
                table: "IC_UserAccount",
                type: "varchar(100)",
                nullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "OldDepartment",
                table: "IC_EmployeeTransfer",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmployeeATID",
                table: "IC_UserAccount");

            migrationBuilder.AlterColumn<int>(
                name: "OldDepartment",
                table: "IC_EmployeeTransfer",
                type: "int",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);
        }
    }
}
