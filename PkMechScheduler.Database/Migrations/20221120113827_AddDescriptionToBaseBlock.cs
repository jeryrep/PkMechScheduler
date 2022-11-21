using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PkMechScheduler.Database.Migrations
{
    public partial class AddDescriptionToBaseBlock : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "StudentBlocks",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "StudentBlocks");
        }
    }
}
