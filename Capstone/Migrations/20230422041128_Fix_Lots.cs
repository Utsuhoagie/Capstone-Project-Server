using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capstone.Migrations
{
    public partial class Fix_Lots : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_People_EmployeeId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "EndHour",
                table: "People");

            migrationBuilder.DropColumn(
                name: "StartHour",
                table: "People");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_People_EmployeeId",
                table: "AspNetUsers",
                column: "EmployeeId",
                principalTable: "People",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_People_EmployeeId",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<int>(
                name: "EndHour",
                table: "People",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StartHour",
                table: "People",
                type: "int",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_People_EmployeeId",
                table: "AspNetUsers",
                column: "EmployeeId",
                principalTable: "People",
                principalColumn: "Id");
        }
    }
}
