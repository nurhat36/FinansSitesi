using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinansSitesi.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToTransactions1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_AspNetUsers_ApplicationUserId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_ApplicationUserId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "Transactions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "Transactions",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_ApplicationUserId",
                table: "Transactions",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_AspNetUsers_ApplicationUserId",
                table: "Transactions",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
