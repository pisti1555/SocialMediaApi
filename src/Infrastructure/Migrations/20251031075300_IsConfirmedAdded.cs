using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class IsConfirmedAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Friendships_Users_User1Id",
                table: "Friendships");

            migrationBuilder.DropForeignKey(
                name: "FK_Friendships_Users_User2Id",
                table: "Friendships");

            migrationBuilder.RenameColumn(
                name: "User2Id",
                table: "Friendships",
                newName: "ResponderId");

            migrationBuilder.RenameColumn(
                name: "User1Id",
                table: "Friendships",
                newName: "RequesterId");

            migrationBuilder.RenameIndex(
                name: "IX_Friendships_User2Id",
                table: "Friendships",
                newName: "IX_Friendships_ResponderId");

            migrationBuilder.RenameIndex(
                name: "IX_Friendships_User1Id_User2Id",
                table: "Friendships",
                newName: "IX_Friendships_RequesterId_ResponderId");

            migrationBuilder.RenameIndex(
                name: "IX_Friendships_User1Id",
                table: "Friendships",
                newName: "IX_Friendships_RequesterId");

            migrationBuilder.AddColumn<bool>(
                name: "IsConfirmed",
                table: "Friendships",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_Friendships_Users_RequesterId",
                table: "Friendships",
                column: "RequesterId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Friendships_Users_ResponderId",
                table: "Friendships",
                column: "ResponderId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Friendships_Users_RequesterId",
                table: "Friendships");

            migrationBuilder.DropForeignKey(
                name: "FK_Friendships_Users_ResponderId",
                table: "Friendships");

            migrationBuilder.DropColumn(
                name: "IsConfirmed",
                table: "Friendships");

            migrationBuilder.RenameColumn(
                name: "ResponderId",
                table: "Friendships",
                newName: "User2Id");

            migrationBuilder.RenameColumn(
                name: "RequesterId",
                table: "Friendships",
                newName: "User1Id");

            migrationBuilder.RenameIndex(
                name: "IX_Friendships_ResponderId",
                table: "Friendships",
                newName: "IX_Friendships_User2Id");

            migrationBuilder.RenameIndex(
                name: "IX_Friendships_RequesterId_ResponderId",
                table: "Friendships",
                newName: "IX_Friendships_User1Id_User2Id");

            migrationBuilder.RenameIndex(
                name: "IX_Friendships_RequesterId",
                table: "Friendships",
                newName: "IX_Friendships_User1Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Friendships_Users_User1Id",
                table: "Friendships",
                column: "User1Id",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Friendships_Users_User2Id",
                table: "Friendships",
                column: "User2Id",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
