using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class CM_UserContactInfo_Update : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_HR_UserContactInfo",
                table: "HR_UserContactInfo");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "HR_UserContactInfo",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "UserIndex",
                table: "HR_UserContactInfo",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HR_UserContactInfo",
                table: "HR_UserContactInfo",
                columns: new[] { "Index", "CompanyIndex" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_HR_UserContactInfo",
                table: "HR_UserContactInfo");

            migrationBuilder.AlterColumn<string>(
                name: "UserIndex",
                table: "HR_UserContactInfo",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "HR_UserContactInfo",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_HR_UserContactInfo",
                table: "HR_UserContactInfo",
                columns: new[] { "UserIndex", "CompanyIndex", "Name" });
        }
    }
}
