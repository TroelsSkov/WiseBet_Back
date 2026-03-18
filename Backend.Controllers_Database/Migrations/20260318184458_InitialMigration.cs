using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WiseBet.backend.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Outcomes",
                columns: table => new
                {
                    OutcomeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OutcomeDescription = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Outcomes", x => x.OutcomeId);
                });

            migrationBuilder.CreateTable(
                name: "Results",
                columns: table => new
                {
                    RoundResultID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Result = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Results", x => x.RoundResultID);
                });

            migrationBuilder.CreateTable(
                name: "UserAccounts",
                columns: table => new
                {
                    UserID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Saldo = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAccounts", x => x.UserID);
                });

            migrationBuilder.CreateTable(
                name: "Rounds",
                columns: table => new
                {
                    RoundID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoundDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OutcomeId = table.Column<int>(type: "int", nullable: true),
                    TotalAmount = table.Column<int>(type: "int", nullable: false),
                    Payout = table.Column<int>(type: "int", nullable: false),
                    Made = table.Column<int>(type: "int", nullable: false),
                    RoundResultID = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rounds", x => x.RoundID);
                    table.ForeignKey(
                        name: "FK_Rounds_Outcomes_OutcomeId",
                        column: x => x.OutcomeId,
                        principalTable: "Outcomes",
                        principalColumn: "OutcomeId");
                    table.ForeignKey(
                        name: "FK_Rounds_Results_RoundResultID",
                        column: x => x.RoundResultID,
                        principalTable: "Results",
                        principalColumn: "RoundResultID");
                });

            migrationBuilder.CreateTable(
                name: "Chats",
                columns: table => new
                {
                    ChatID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    chat = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TimeOfChat = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chats", x => x.ChatID);
                    table.ForeignKey(
                        name: "FK_Chats_UserAccounts_UserID",
                        column: x => x.UserID,
                        principalTable: "UserAccounts",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentHistories",
                columns: table => new
                {
                    PaymentID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TimeOfPayment = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaymentAmount = table.Column<int>(type: "int", nullable: false),
                    PrePaymentBalance = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentHistories", x => x.PaymentID);
                    table.ForeignKey(
                        name: "FK_PaymentHistories_UserAccounts_UserID",
                        column: x => x.UserID,
                        principalTable: "UserAccounts",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BetHistories",
                columns: table => new
                {
                    BetHistoryID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserAccountUserID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OutcomeId = table.Column<int>(type: "int", nullable: true),
                    RoundID = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BetHistories", x => x.BetHistoryID);
                    table.ForeignKey(
                        name: "FK_BetHistories_Outcomes_OutcomeId",
                        column: x => x.OutcomeId,
                        principalTable: "Outcomes",
                        principalColumn: "OutcomeId");
                    table.ForeignKey(
                        name: "FK_BetHistories_Rounds_RoundID",
                        column: x => x.RoundID,
                        principalTable: "Rounds",
                        principalColumn: "RoundID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BetHistories_UserAccounts_UserAccountUserID",
                        column: x => x.UserAccountUserID,
                        principalTable: "UserAccounts",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BetHistories_OutcomeId",
                table: "BetHistories",
                column: "OutcomeId");

            migrationBuilder.CreateIndex(
                name: "IX_BetHistories_RoundID",
                table: "BetHistories",
                column: "RoundID");

            migrationBuilder.CreateIndex(
                name: "IX_BetHistories_UserAccountUserID",
                table: "BetHistories",
                column: "UserAccountUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Chats_UserID",
                table: "Chats",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentHistories_UserID",
                table: "PaymentHistories",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Rounds_OutcomeId",
                table: "Rounds",
                column: "OutcomeId");

            migrationBuilder.CreateIndex(
                name: "IX_Rounds_RoundResultID",
                table: "Rounds",
                column: "RoundResultID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BetHistories");

            migrationBuilder.DropTable(
                name: "Chats");

            migrationBuilder.DropTable(
                name: "PaymentHistories");

            migrationBuilder.DropTable(
                name: "Rounds");

            migrationBuilder.DropTable(
                name: "UserAccounts");

            migrationBuilder.DropTable(
                name: "Outcomes");

            migrationBuilder.DropTable(
                name: "Results");
        }
    }
}
