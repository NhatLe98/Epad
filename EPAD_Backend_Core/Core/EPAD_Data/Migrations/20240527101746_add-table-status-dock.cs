using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class addtablestatusdock : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IC_StatusDock",
                columns: table => new
                {
                    Key = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IC_StatusDock", x => x.Key);
                });

            migrationBuilder.InsertData(
                table: "IC_StatusDock",
                columns: new[] { "Key", "Name" },
                values: new object[] { "0001", "Đăng tài" });

            migrationBuilder.InsertData(
                table: "IC_StatusDock",
                columns: new[] { "Key", "Name" },
                values: new object[] { "0002", "Xe vào cổng" });

            migrationBuilder.InsertData(
                table: "IC_StatusDock",
                columns: new[] { "Key", "Name" },
                values: new object[] { "0003", "Xe ra cổng" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IC_StatusDock");
        }
    }
}
