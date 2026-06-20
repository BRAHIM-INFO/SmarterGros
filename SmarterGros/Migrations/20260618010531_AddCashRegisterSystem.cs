using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmarterGros.Migrations
{
    /// <inheritdoc />
    public partial class AddCashRegisterSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CashRegisters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OpeningBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CurrentBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OpeningBalanceDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResponsibleUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResponsibleUserName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashRegisters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DailyClosures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CashRegisterId = table.Column<int>(type: "int", nullable: false),
                    ClosureDate = table.Column<DateTime>(type: "date", nullable: false),
                    OpeningBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalExpense = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ExpectedBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ActualBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Difference = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Count2000 = table.Column<int>(type: "int", nullable: false),
                    Count1000 = table.Column<int>(type: "int", nullable: false),
                    Count500 = table.Column<int>(type: "int", nullable: false),
                    Count200 = table.Column<int>(type: "int", nullable: false),
                    Count100 = table.Column<int>(type: "int", nullable: false),
                    Count50 = table.Column<int>(type: "int", nullable: false),
                    Count20 = table.Column<int>(type: "int", nullable: false),
                    Count10 = table.Column<int>(type: "int", nullable: false),
                    Count5 = table.Column<int>(type: "int", nullable: false),
                    CoinsAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TransactionsCount = table.Column<int>(type: "int", nullable: false),
                    IncomeCount = table.Column<int>(type: "int", nullable: false),
                    ExpenseCount = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DifferenceReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsClosed = table.Column<bool>(type: "bit", nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClosedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClosedByName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyClosures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyClosures_CashRegisters_CashRegisterId",
                        column: x => x.CashRegisterId,
                        principalTable: "CashRegisters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CashTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransactionNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CashRegisterId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    PaymentMethod = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BalanceBefore = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BalanceAfter = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ReferenceType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ReferenceId = table.Column<int>(type: "int", nullable: true),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SupplierId = table.Column<int>(type: "int", nullable: true),
                    CustomerId = table.Column<int>(type: "int", nullable: true),
                    CheckNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BankName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CheckDueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedByName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsCancelled = table.Column<bool>(type: "bit", nullable: false),
                    CancellationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CancelledByName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DailyClosureId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CashTransactions_CashRegisters_CashRegisterId",
                        column: x => x.CashRegisterId,
                        principalTable: "CashRegisters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CashTransactions_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CashTransactions_DailyClosures_DailyClosureId",
                        column: x => x.DailyClosureId,
                        principalTable: "DailyClosures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CashTransactions_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CashRegisters_IsActive",
                table: "CashRegisters",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_CashRegisters_IsDefault",
                table: "CashRegisters",
                column: "IsDefault");

            migrationBuilder.CreateIndex(
                name: "IX_CashRegisters_Name",
                table: "CashRegisters",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_CashTransactions_CashRegisterId",
                table: "CashTransactions",
                column: "CashRegisterId");

            migrationBuilder.CreateIndex(
                name: "IX_CashTransactions_CashRegisterId_TransactionDate",
                table: "CashTransactions",
                columns: new[] { "CashRegisterId", "TransactionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_CashTransactions_Category",
                table: "CashTransactions",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_CashTransactions_Category_TransactionDate",
                table: "CashTransactions",
                columns: new[] { "Category", "TransactionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_CashTransactions_CustomerId",
                table: "CashTransactions",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CashTransactions_DailyClosureId",
                table: "CashTransactions",
                column: "DailyClosureId");

            migrationBuilder.CreateIndex(
                name: "IX_CashTransactions_IsCancelled",
                table: "CashTransactions",
                column: "IsCancelled");

            migrationBuilder.CreateIndex(
                name: "IX_CashTransactions_ReferenceType",
                table: "CashTransactions",
                column: "ReferenceType");

            migrationBuilder.CreateIndex(
                name: "IX_CashTransactions_SupplierId",
                table: "CashTransactions",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_CashTransactions_TransactionDate",
                table: "CashTransactions",
                column: "TransactionDate");

            migrationBuilder.CreateIndex(
                name: "IX_CashTransactions_TransactionNumber",
                table: "CashTransactions",
                column: "TransactionNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CashTransactions_Type",
                table: "CashTransactions",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_CashTransactions_Type_TransactionDate",
                table: "CashTransactions",
                columns: new[] { "Type", "TransactionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_DailyClosures_CashRegisterId",
                table: "DailyClosures",
                column: "CashRegisterId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyClosures_CashRegisterId_ClosureDate",
                table: "DailyClosures",
                columns: new[] { "CashRegisterId", "ClosureDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DailyClosures_ClosureDate",
                table: "DailyClosures",
                column: "ClosureDate");

            migrationBuilder.CreateIndex(
                name: "IX_DailyClosures_IsClosed",
                table: "DailyClosures",
                column: "IsClosed");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CashTransactions");

            migrationBuilder.DropTable(
                name: "DailyClosures");

            migrationBuilder.DropTable(
                name: "CashRegisters");
        }
    }
}
