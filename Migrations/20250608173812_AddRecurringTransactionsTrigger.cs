using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinansSitesi.Migrations
{
    /// <inheritdoc />
    public partial class AddRecurringTransactionsTrigger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"
        CREATE TRIGGER trg_UpdateAccountBalanceOnRecurringTransaction
        ON RecurringTransactions
        INSTEAD OF INSERT, UPDATE
        AS
        BEGIN
            SET NOCOUNT ON;

            -- Güncelleme yapılmışsa önce eski bakiyeyi geri al (balance restore)
            IF EXISTS (SELECT 1 FROM DELETED)
            BEGIN
                UPDATE a
                SET a.Balance = a.Balance + 
                                CASE 
                                    WHEN d.Type = 'Income' THEN -d.Amount  -- Eski gelir ise azalt
                                    WHEN d.Type = 'Expense' THEN d.Amount   -- Eski gider ise artır
                                    ELSE 0
                                END
                FROM Accounts a
                INNER JOIN DELETED d ON a.Id = d.AccountId
            END

            -- Asıl insert/update işlemini yap (manuel)
            MERGE RecurringTransactions AS target
            USING INSERTED AS source
            ON target.Id = source.Id
            WHEN MATCHED THEN
                UPDATE SET 
                    Amount = source.Amount,
                    Type = source.Type,
                    CategoryId = source.CategoryId,
                    Frequency = source.Frequency,
                    StartDate = source.StartDate,
                    EndDate = source.EndDate,
                    Description = source.Description,
                    AccountId = source.AccountId,
                    UserId = source.UserId
            WHEN NOT MATCHED THEN
                INSERT (UserId, Amount, Type, CategoryId, Frequency, StartDate, EndDate, Description, AccountId)
                VALUES (source.UserId, source.Amount, source.Type, source.CategoryId, source.Frequency, source.StartDate, source.EndDate, source.Description, source.AccountId);

            -- Yeni değeri uygula (INSERT veya UPDATE)
            UPDATE a
            SET a.Balance = a.Balance + 
                            CASE 
                                WHEN i.Type = 'Income' THEN i.Amount
                                WHEN i.Type = 'Expense' THEN -i.Amount
                                ELSE 0
                            END
            FROM Accounts a
            INNER JOIN INSERTED i ON a.Id = i.AccountId
        END
        ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"DROP TRIGGER IF EXISTS trg_UpdateAccountBalanceOnRecurringTransaction;");
        }

    }
}
