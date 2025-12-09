using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComputerStore.Migrations
{
    /// <inheritdoc />
    public partial class AddArticleCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Supplies_Users_UserId",
                table: "Supplies");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Supplies",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Products",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_Supplies_Users_UserId",
                table: "Supplies",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Supplies_Users_UserId",
                table: "Supplies");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Products");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Supplies",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Supplies_Users_UserId",
                table: "Supplies",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
