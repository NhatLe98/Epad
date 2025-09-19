using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Data.Migrations
{
    public partial class alterUserMaster : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthenType",
                table: "IC_UserInfo");

            migrationBuilder.AddColumn<string>(
                name: "FaceV2_Content",
                table: "IC_UserMaster",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FaceV2_Duress",
                table: "IC_UserMaster",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FaceV2_Format",
                table: "IC_UserMaster",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FaceV2_Index",
                table: "IC_UserMaster",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FaceV2_MajorVer",
                table: "IC_UserMaster",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FaceV2_MinorVer",
                table: "IC_UserMaster",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FaceV2_No",
                table: "IC_UserMaster",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FaceV2_Size",
                table: "IC_UserMaster",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FaceV2_TemplateBIODATA",
                table: "IC_UserMaster",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FaceV2_Type",
                table: "IC_UserMaster",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FaceV2_Valid",
                table: "IC_UserMaster",
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "Privilege",
                table: "IC_UserMaster",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuthenMode",
                table: "IC_UserInfo",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FaceV2_Content",
                table: "IC_UserMaster");

            migrationBuilder.DropColumn(
                name: "FaceV2_Duress",
                table: "IC_UserMaster");

            migrationBuilder.DropColumn(
                name: "FaceV2_Format",
                table: "IC_UserMaster");

            migrationBuilder.DropColumn(
                name: "FaceV2_Index",
                table: "IC_UserMaster");

            migrationBuilder.DropColumn(
                name: "FaceV2_MajorVer",
                table: "IC_UserMaster");

            migrationBuilder.DropColumn(
                name: "FaceV2_MinorVer",
                table: "IC_UserMaster");

            migrationBuilder.DropColumn(
                name: "FaceV2_No",
                table: "IC_UserMaster");

            migrationBuilder.DropColumn(
                name: "FaceV2_Size",
                table: "IC_UserMaster");

            migrationBuilder.DropColumn(
                name: "FaceV2_TemplateBIODATA",
                table: "IC_UserMaster");

            migrationBuilder.DropColumn(
                name: "FaceV2_Type",
                table: "IC_UserMaster");

            migrationBuilder.DropColumn(
                name: "FaceV2_Valid",
                table: "IC_UserMaster");

            migrationBuilder.DropColumn(
                name: "Privilege",
                table: "IC_UserMaster");

            migrationBuilder.DropColumn(
                name: "AuthenMode",
                table: "IC_UserInfo");

            migrationBuilder.AddColumn<int>(
                name: "AuthenType",
                table: "IC_UserInfo",
                nullable: true);
        }
    }
}
