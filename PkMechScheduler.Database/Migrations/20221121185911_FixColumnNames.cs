using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PkMechScheduler.Database.Migrations
{
    public partial class FixColumnNames : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PlacesLink",
                table: "TeacherBlocks",
                newName: "PlaceLink");

            migrationBuilder.RenameColumn(
                name: "CourserLinks",
                table: "TeacherBlocks",
                newName: "CourseLinks");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PlaceLink",
                table: "TeacherBlocks",
                newName: "PlacesLink");

            migrationBuilder.RenameColumn(
                name: "CourseLinks",
                table: "TeacherBlocks",
                newName: "CourserLinks");
        }
    }
}
