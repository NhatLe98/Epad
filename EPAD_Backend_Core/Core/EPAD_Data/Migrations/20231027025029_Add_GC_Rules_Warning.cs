using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class Add_GC_Rules_Warning : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GC_Rules_Warning",
                columns: table => new
                {
                    Index = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UseSpeaker = table.Column<bool>(type: "bit", nullable: true),
                    UseSpeakerFocus = table.Column<bool>(type: "bit", nullable: true),
                    UseSpeakerInPlace = table.Column<bool>(type: "bit", nullable: true),
                    UseLed = table.Column<bool>(type: "bit", nullable: true),
                    UseLedFocus = table.Column<bool>(type: "bit", nullable: true),
                    UseLedInPlace = table.Column<bool>(type: "bit", nullable: true),
                    UseEmail = table.Column<bool>(type: "bit", nullable: true),
                    Email = table.Column<string>(type: "varchar(100)", nullable: true),
                    EmailSendType = table.Column<short>(type: "smallint", nullable: true),
                    UseComputerSound = table.Column<bool>(type: "bit", nullable: true),
                    ComputerSoundPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UseChangeColor = table.Column<bool>(type: "bit", nullable: true),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false),
                    RulesWarningGroupIndex = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedUser = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GC_Rules_Warning", x => x.Index);
                });

            migrationBuilder.CreateTable(
                name: "GC_Rules_Warning_ControllerChannels",
                columns: table => new
                {
                    Index = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ControllerIndex = table.Column<int>(type: "int", nullable: false),
                    ChannelIndex = table.Column<int>(type: "int", nullable: false),
                    RulesWarningIndex = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    LineIndex = table.Column<int>(type: "int", nullable: true),
                    GateIndex = table.Column<int>(type: "int", nullable: true),
                    SerialNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedUser = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GC_Rules_Warning_ControllerChannels", x => x.Index);
                });

            migrationBuilder.CreateTable(
                name: "GC_Rules_Warning_EmailSchedules",
                columns: table => new
                {
                    Index = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DayOfWeekIndex = table.Column<int>(type: "int", nullable: false),
                    Time = table.Column<TimeSpan>(type: "time", nullable: false),
                    LatestDateSendMail = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false),
                    RulesWarningIndex = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedUser = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GC_Rules_Warning_EmailSchedules", x => x.Index);
                    table.UniqueConstraint("AK_GC_Rules_Warning_EmailSchedules_Time_CompanyIndex_DayOfWeekIndex_RulesWarningIndex", x => new { x.Time, x.CompanyIndex, x.DayOfWeekIndex, x.RulesWarningIndex });
                });

            migrationBuilder.CreateTable(
                name: "GC_Rules_WarningGroup",
                columns: table => new
                {
                    Index = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedUser = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GC_Rules_WarningGroup", x => x.Index);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GC_Rules_Warning");

            migrationBuilder.DropTable(
                name: "GC_Rules_Warning_ControllerChannels");

            migrationBuilder.DropTable(
                name: "GC_Rules_Warning_EmailSchedules");

            migrationBuilder.DropTable(
                name: "GC_Rules_WarningGroup");
        }
    }
}
