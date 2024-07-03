using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RapidPay.CardsManagement.Api.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "CardsManagement");

            migrationBuilder.CreateTable(
                name: "Cards",
                schema: "CardsManagement",
                columns: table => new
                {
                    Number = table.Column<string>(type: "nchar(15)", fixedLength: true, maxLength: 15, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cards", x => x.Number);
                });

            migrationBuilder.CreateTable(
                name: "TransactionTypes",
                schema: "CardsManagement",
                columns: table => new
                {
                    SystemCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sign = table.Column<int>(type: "int", nullable: false),
                    GeneratesFee = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionTypes", x => x.SystemCode);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                schema: "CardsManagement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CardNumber = table.Column<string>(type: "nchar(15)", nullable: false),
                    TypeSystemCode = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GetUtcDate()"),
                    TransactionAmount = table.Column<decimal>(type: "decimal(17,4)", precision: 17, scale: 4, nullable: false, defaultValue: 0m),
                    FeeAmount = table.Column<decimal>(type: "decimal(17,4)", precision: 17, scale: 4, nullable: false, defaultValue: 0m),
                    CardBalanceAmount = table.Column<decimal>(type: "decimal(17,4)", precision: 17, scale: 4, nullable: false, defaultValue: 0m)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transactions_Cards_CardNumber",
                        column: x => x.CardNumber,
                        principalSchema: "CardsManagement",
                        principalTable: "Cards",
                        principalColumn: "Number",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Transactions_TransactionTypes_TypeSystemCode",
                        column: x => x.TypeSystemCode,
                        principalSchema: "CardsManagement",
                        principalTable: "TransactionTypes",
                        principalColumn: "SystemCode");
                });

            migrationBuilder.InsertData(
                schema: "CardsManagement",
                table: "TransactionTypes",
                columns: new[] { "SystemCode", "GeneratesFee", "Name", "Sign" },
                values: new object[,]
                {
                    { "Payment", true, "Payment", 1 },
                    { "Purchase", false, "Purchase", -1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CardNumber",
                schema: "CardsManagement",
                table: "Transactions",
                column: "CardNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_TypeSystemCode",
                schema: "CardsManagement",
                table: "Transactions",
                column: "TypeSystemCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Transactions",
                schema: "CardsManagement");

            migrationBuilder.DropTable(
                name: "Cards",
                schema: "CardsManagement");

            migrationBuilder.DropTable(
                name: "TransactionTypes",
                schema: "CardsManagement");
        }
    }
}
