using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinansSitesi.Migrations
{
    /// <inheritdoc />
    public partial class Addrecursive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AccountId",
                table: "RecurringTransactions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_RecurringTransactions_AccountId",
                table: "RecurringTransactions",
                column: "AccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_RecurringTransactions_Accounts_AccountId",
                table: "RecurringTransactions",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecurringTransactions_Accounts_AccountId",
                table: "RecurringTransactions");

            migrationBuilder.DropIndex(
                name: "IX_RecurringTransactions_AccountId",
                table: "RecurringTransactions");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "RecurringTransactions");
        }
    }
}
