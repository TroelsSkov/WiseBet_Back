using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WiseBet.backend.Migrations
{
    /// <inheritdoc />
    public partial class FixedForeignKeyConstraintsForPaymentHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentHistories_UserAccounts_UserAccountUserID",
                table: "PaymentHistories");

            migrationBuilder.DropIndex(
                name: "IX_PaymentHistories_UserAccountUserID",
                table: "PaymentHistories");

            migrationBuilder.DropColumn(
                name: "UserAccountUserID",
                table: "PaymentHistories");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentHistories_UserID",
                table: "PaymentHistories",
                column: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentHistories_UserAccounts_UserID",
                table: "PaymentHistories",
                column: "UserID",
                principalTable: "UserAccounts",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentHistories_UserAccounts_UserID",
                table: "PaymentHistories");

            migrationBuilder.DropIndex(
                name: "IX_PaymentHistories_UserID",
                table: "PaymentHistories");

            migrationBuilder.AddColumn<Guid>(
                name: "UserAccountUserID",
                table: "PaymentHistories",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_PaymentHistories_UserAccountUserID",
                table: "PaymentHistories",
                column: "UserAccountUserID");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentHistories_UserAccounts_UserAccountUserID",
                table: "PaymentHistories",
                column: "UserAccountUserID",
                principalTable: "UserAccounts",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
