using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class Add_table_GC_CustomerVehicle : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GC_CustomerVehicle",
                columns: table => new
                {
                    Index = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false),
                    EmployeeATID = table.Column<string>(type: "nvarchar(30)", nullable: false),
                    Type = table.Column<short>(type: "smallint", nullable: false),
                    Plate = table.Column<string>(type: "nvarchar(30)", nullable: true),
                    Branch = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    Color = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedUser = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GC_CustomerVehicle", x => x.Index);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GC_CustomerVehicle");
        }
    }
}
