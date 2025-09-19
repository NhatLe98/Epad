using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Data.Migrations
{
    public partial class alterUserInforColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FaceTemplate",
                table: "IC_UserInfo",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FaceTemplateV2",
                table: "IC_UserInfo",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FingerData0",
                table: "IC_UserInfo",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FingerData1",
                table: "IC_UserInfo",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FingerData2",
                table: "IC_UserInfo",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FingerData3",
                table: "IC_UserInfo",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FingerData4",
                table: "IC_UserInfo",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FingerData5",
                table: "IC_UserInfo",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FingerData6",
                table: "IC_UserInfo",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FingerData7",
                table: "IC_UserInfo",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FingerData8",
                table: "IC_UserInfo",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FingerData9",
                table: "IC_UserInfo",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VeinsData1",
                table: "IC_UserInfo",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VeinsData2",
                table: "IC_UserInfo",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VeinsData3",
                table: "IC_UserInfo",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FaceTemplate",
                table: "IC_UserInfo");

            migrationBuilder.DropColumn(
                name: "FaceTemplateV2",
                table: "IC_UserInfo");

            migrationBuilder.DropColumn(
                name: "FingerData0",
                table: "IC_UserInfo");

            migrationBuilder.DropColumn(
                name: "FingerData1",
                table: "IC_UserInfo");

            migrationBuilder.DropColumn(
                name: "FingerData2",
                table: "IC_UserInfo");

            migrationBuilder.DropColumn(
                name: "FingerData3",
                table: "IC_UserInfo");

            migrationBuilder.DropColumn(
                name: "FingerData4",
                table: "IC_UserInfo");

            migrationBuilder.DropColumn(
                name: "FingerData5",
                table: "IC_UserInfo");

            migrationBuilder.DropColumn(
                name: "FingerData6",
                table: "IC_UserInfo");

            migrationBuilder.DropColumn(
                name: "FingerData7",
                table: "IC_UserInfo");

            migrationBuilder.DropColumn(
                name: "FingerData8",
                table: "IC_UserInfo");

            migrationBuilder.DropColumn(
                name: "FingerData9",
                table: "IC_UserInfo");

            migrationBuilder.DropColumn(
                name: "VeinsData1",
                table: "IC_UserInfo");

            migrationBuilder.DropColumn(
                name: "VeinsData2",
                table: "IC_UserInfo");

            migrationBuilder.DropColumn(
                name: "VeinsData3",
                table: "IC_UserInfo");
        }
    }
}
