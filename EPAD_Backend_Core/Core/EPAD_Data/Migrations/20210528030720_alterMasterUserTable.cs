using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Data.Migrations
{
    public partial class alterMasterUserTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "FaceIndex",
                table: "IC_UserMaster",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FaceTemplate",
                table: "IC_UserMaster",
                type: "text",
                nullable: true);

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

            migrationBuilder.AddColumn<string>(
                name: "FingerData0",
                table: "IC_UserMaster",
                type: "varchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FingerData1",
                table: "IC_UserMaster",
                type: "varchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FingerData2",
                table: "IC_UserMaster",
                type: "varchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FingerData3",
                table: "IC_UserMaster",
                type: "varchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FingerData4",
                table: "IC_UserMaster",
                type: "varchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FingerData5",
                table: "IC_UserMaster",
                type: "varchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FingerData6",
                table: "IC_UserMaster",
                type: "varchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FingerData7",
                table: "IC_UserMaster",
                type: "varchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FingerData8",
                table: "IC_UserMaster",
                type: "varchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FingerData9",
                table: "IC_UserMaster",
                type: "varchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "FingerIndex",
                table: "IC_UserMaster",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FaceIndex",
                table: "IC_UserMaster");

            migrationBuilder.DropColumn(
                name: "FaceTemplate",
                table: "IC_UserMaster");

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
                name: "FingerData0",
                table: "IC_UserMaster");

            migrationBuilder.DropColumn(
                name: "FingerData1",
                table: "IC_UserMaster");

            migrationBuilder.DropColumn(
                name: "FingerData2",
                table: "IC_UserMaster");

            migrationBuilder.DropColumn(
                name: "FingerData3",
                table: "IC_UserMaster");

            migrationBuilder.DropColumn(
                name: "FingerData4",
                table: "IC_UserMaster");

            migrationBuilder.DropColumn(
                name: "FingerData5",
                table: "IC_UserMaster");

            migrationBuilder.DropColumn(
                name: "FingerData6",
                table: "IC_UserMaster");

            migrationBuilder.DropColumn(
                name: "FingerData7",
                table: "IC_UserMaster");

            migrationBuilder.DropColumn(
                name: "FingerData8",
                table: "IC_UserMaster");

            migrationBuilder.DropColumn(
                name: "FingerData9",
                table: "IC_UserMaster");

            migrationBuilder.DropColumn(
                name: "FingerIndex",
                table: "IC_UserMaster");
        }
    }
}
