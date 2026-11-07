using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Give_AID.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentMethodAndSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CardLast4",
                table: "Donations",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(4)",
                oldMaxLength: 4);

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "Donations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReceiptPath",
                table: "Donations",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransactionRef",
                table: "Donations",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PaymentSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EasyPaisaNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EasyPaisaTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    JazzCashNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    JazzCashTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BankName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BankAccountTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BankAccountNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BankIBAN = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BankBranch = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentSettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentSettings");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "Donations");

            migrationBuilder.DropColumn(
                name: "ReceiptPath",
                table: "Donations");

            migrationBuilder.DropColumn(
                name: "TransactionRef",
                table: "Donations");

            migrationBuilder.AlterColumn<string>(
                name: "CardLast4",
                table: "Donations",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(4)",
                oldMaxLength: 4,
                oldNullable: true);
        }
    }
}
