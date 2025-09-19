using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class IC_HistoryTrackingIntegrate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IC_HistoryTrackingIntegrate",
                columns: table => new
                {
                    Index = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RunTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Success = table.Column<short>(type: "smallint", nullable: false),
                    Failed = table.Column<short>(type: "smallint", nullable: false),
                    DataNew = table.Column<short>(type: "smallint", nullable: false),
                    DataUpdate = table.Column<short>(type: "smallint", nullable: false),
                    DataDelete = table.Column<short>(type: "smallint", nullable: false),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IC_HistoryTrackingIntegrate", x => x.Index);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IC_HistoryTrackingIntegrate");
        }
    }
}
