using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WiseBet.backend.Migrations
{
    /// <inheritdoc />
    public partial class UserAccountForeignKeyFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chats_UserAccounts_UserAccountUserID",
                table: "Chats");

            migrationBuilder.DropIndex(
                name: "IX_Chats_UserAccountUserID",
                table: "Chats");

            migrationBuilder.DropColumn(
                name: "UserAccountUserID",
                table: "Chats");

            migrationBuilder.CreateIndex(
                name: "IX_Chats_UserID",
                table: "Chats",
                column: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Chats_UserAccounts_UserID",
                table: "Chats",
                column: "UserID",
                principalTable: "UserAccounts",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chats_UserAccounts_UserID",
                table: "Chats");

            migrationBuilder.DropIndex(
                name: "IX_Chats_UserID",
                table: "Chats");

            migrationBuilder.AddColumn<Guid>(
                name: "UserAccountUserID",
                table: "Chats",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Chats_UserAccountUserID",
                table: "Chats",
                column: "UserAccountUserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Chats_UserAccounts_UserAccountUserID",
                table: "Chats",
                column: "UserAccountUserID",
                principalTable: "UserAccounts",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
