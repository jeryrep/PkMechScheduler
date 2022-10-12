using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PkMechScheduler.Database.Migrations
{
    /// <inheritdoc />
    public partial class TeacherBlocks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Blocks",
                table: "StudentBlocks");

            migrationBuilder.RenameTable(
                name: "StudentBlocks",
                newName: "BlockModel");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BlockModel",
                table: "BlockModel",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_BlockModel",
                table: "BlockModel");

            migrationBuilder.RenameTable(
                name: "BlockModel",
                newName: "StudentBlocks");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Blocks",
                table: "StudentBlocks",
                column: "Id");
        }
    }
}
