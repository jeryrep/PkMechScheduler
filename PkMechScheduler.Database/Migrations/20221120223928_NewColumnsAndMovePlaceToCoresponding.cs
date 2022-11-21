using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PkMechScheduler.Database.Migrations
{
    public partial class NewColumnsAndMovePlaceToCoresponding : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CourserLinks",
                table: "TeacherBlocks",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlacesLink",
                table: "TeacherBlocks",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InitialsLink",
                table: "StudentBlocks",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlaceLink",
                table: "StudentBlocks",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CourserLinks",
                table: "TeacherBlocks");

            migrationBuilder.DropColumn(
                name: "PlacesLink",
                table: "TeacherBlocks");

            migrationBuilder.DropColumn(
                name: "InitialsLink",
                table: "StudentBlocks");

            migrationBuilder.DropColumn(
                name: "PlaceLink",
                table: "StudentBlocks");
        }
    }
}
