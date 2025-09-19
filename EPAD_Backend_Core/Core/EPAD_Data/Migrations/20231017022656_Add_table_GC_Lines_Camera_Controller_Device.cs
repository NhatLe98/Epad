using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class Add_table_GC_Lines_Camera_Controller_Device : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GC_Lines_CheckInCamera",
                columns: table => new
                {
                    LineIndex = table.Column<int>(type: "int", nullable: false),
                    CameraIndex = table.Column<int>(type: "int", nullable: false),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedUser = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GC_Lines_CheckInCamera", x => new { x.LineIndex, x.CameraIndex, x.CompanyIndex });
                });

            migrationBuilder.CreateTable(
                name: "GC_Lines_CheckInDevice",
                columns: table => new
                {
                    LineIndex = table.Column<int>(type: "int", nullable: false),
                    CheckInDeviceSerial = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedUser = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GC_Lines_CheckInDevice", x => new { x.LineIndex, x.CompanyIndex, x.CheckInDeviceSerial });
                });

            migrationBuilder.CreateTable(
                name: "GC_Lines_CheckInRelayController",
                columns: table => new
                {
                    LineIndex = table.Column<int>(type: "int", nullable: false),
                    RelayControllerIndex = table.Column<int>(type: "int", nullable: false),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false),
                    OpenDoorChannelIndex = table.Column<short>(type: "smallint", nullable: false),
                    FailAlarmChannelIndex = table.Column<short>(type: "smallint", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedUser = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GC_Lines_CheckInRelayController", x => new { x.LineIndex, x.RelayControllerIndex, x.CompanyIndex });
                });

            migrationBuilder.CreateTable(
                name: "GC_Lines_CheckOutCamera",
                columns: table => new
                {
                    LineIndex = table.Column<int>(type: "int", nullable: false),
                    CameraIndex = table.Column<int>(type: "int", nullable: false),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedUser = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GC_Lines_CheckOutCamera", x => new { x.LineIndex, x.CameraIndex, x.CompanyIndex });
                });

            migrationBuilder.CreateTable(
                name: "GC_Lines_CheckOutDevice",
                columns: table => new
                {
                    LineIndex = table.Column<int>(type: "int", nullable: false),
                    CheckOutDeviceSerial = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedUser = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GC_Lines_CheckOutDevice", x => new { x.LineIndex, x.CompanyIndex, x.CheckOutDeviceSerial });
                });

            migrationBuilder.CreateTable(
                name: "GC_Lines_CheckOutRelayController",
                columns: table => new
                {
                    LineIndex = table.Column<int>(type: "int", nullable: false),
                    RelayControllerIndex = table.Column<int>(type: "int", nullable: false),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false),
                    OpenDoorChannelIndex = table.Column<short>(type: "smallint", nullable: false),
                    FailAlarmChannelIndex = table.Column<short>(type: "smallint", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedUser = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GC_Lines_CheckOutRelayController", x => new { x.LineIndex, x.RelayControllerIndex, x.CompanyIndex });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GC_Lines_CheckInCamera");

            migrationBuilder.DropTable(
                name: "GC_Lines_CheckInDevice");

            migrationBuilder.DropTable(
                name: "GC_Lines_CheckInRelayController");

            migrationBuilder.DropTable(
                name: "GC_Lines_CheckOutCamera");

            migrationBuilder.DropTable(
                name: "GC_Lines_CheckOutDevice");

            migrationBuilder.DropTable(
                name: "GC_Lines_CheckOutRelayController");
        }
    }
}
