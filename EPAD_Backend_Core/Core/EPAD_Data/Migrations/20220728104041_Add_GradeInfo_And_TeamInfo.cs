using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class Add_GradeInfo_And_TeamInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TeamIndex",
                table: "IC_WorkingInfo",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GradeIndex",
                table: "HR_ClassInfo",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "HR_GradeInfo",
                columns: table => new
                {
                    Index = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HR_GradeInfo", x => x.Index);
                });

            migrationBuilder.CreateTable(
                name: "HR_TeamInfo",
                columns: table => new
                {
                    Index = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HR_TeamInfo", x => x.Index);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HR_GradeInfo");

            migrationBuilder.DropTable(
                name: "HR_TeamInfo");

            migrationBuilder.DropColumn(
                name: "TeamIndex",
                table: "IC_WorkingInfo");

            migrationBuilder.DropColumn(
                name: "GradeIndex",
                table: "HR_ClassInfo");
        }
    }
}
