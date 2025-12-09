using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComputerStore.Migrations
{
    /// <inheritdoc />
    public partial class AddUserToSupply : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Supplies",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Supplies_UserId",
                table: "Supplies",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Supplies_Users_UserId",
                table: "Supplies",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Supplies_Users_UserId",
                table: "Supplies");

            migrationBuilder.DropIndex(
                name: "IX_Supplies_UserId",
                table: "Supplies");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Supplies");
        }
    }
}
