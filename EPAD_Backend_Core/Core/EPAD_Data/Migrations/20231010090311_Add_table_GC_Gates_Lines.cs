using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class Add_table_GC_Gates_Lines : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GC_Gates_Lines",
                columns: table => new
                {
                    GateIndex = table.Column<int>(type: "int", nullable: false),
                    LineIndex = table.Column<int>(type: "int", nullable: false),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedUser = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GC_Gates_Lines", x => new { x.GateIndex, x.LineIndex, x.CompanyIndex });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GC_Gates_Lines");
        }
    }
}
