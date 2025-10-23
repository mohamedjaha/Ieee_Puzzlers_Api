using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IEEE_Application.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatorIdToPuzzels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatorId",
                table: "Puzzles",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Puzzles_CreatorId",
                table: "Puzzles",
                column: "CreatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Puzzles_AspNetUsers_CreatorId",
                table: "Puzzles",
                column: "CreatorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Puzzles_AspNetUsers_CreatorId",
                table: "Puzzles");

            migrationBuilder.DropIndex(
                name: "IX_Puzzles_CreatorId",
                table: "Puzzles");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "Puzzles");
        }
    }
}
