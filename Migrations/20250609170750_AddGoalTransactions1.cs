using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinansSitesi.Migrations
{
    /// <inheritdoc />
    public partial class AddGoalTransactions1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GoalTransactions_Transactions_TransactionId",
                table: "GoalTransactions");

            migrationBuilder.DropIndex(
                name: "IX_GoalTransactions_TransactionId",
                table: "GoalTransactions");

            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "GoalTransactions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TransactionId",
                table: "GoalTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GoalTransactions_TransactionId",
                table: "GoalTransactions",
                column: "TransactionId");

            migrationBuilder.AddForeignKey(
                name: "FK_GoalTransactions_Transactions_TransactionId",
                table: "GoalTransactions",
                column: "TransactionId",
                principalTable: "Transactions",
                principalColumn: "Id");
        }
    }
}
