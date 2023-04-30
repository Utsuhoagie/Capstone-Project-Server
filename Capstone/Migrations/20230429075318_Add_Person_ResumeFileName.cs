using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capstone.Migrations
{
    public partial class Add_Person_ResumeFileName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ResumeFileName",
                table: "People",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResumeFileName",
                table: "People");
        }
    }
}
