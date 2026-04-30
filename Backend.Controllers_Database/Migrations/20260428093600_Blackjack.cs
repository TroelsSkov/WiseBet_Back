using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WiseBet.backend.Migrations
{
    /// <inheritdoc />
    public partial class Blackjack : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
        name: "Password",
        table: "UserAccounts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
                migrationBuilder.AddColumn<string>(
        name: "Password",
        table: "UserAccounts",
        type: "nvarchar(max)",
        nullable: false,
        defaultValue: "");
        }
    }
}
