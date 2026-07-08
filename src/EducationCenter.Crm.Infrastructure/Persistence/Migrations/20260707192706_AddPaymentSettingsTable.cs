using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EducationCenter.Crm.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentSettingsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "FinalAmount",
                table: "TuitionInvoices",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsVietQrOutdated",
                table: "TuitionInvoices",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PaymentContent",
                table: "TuitionInvoices",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentStatus",
                table: "TuitionInvoices",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VietQrImageUrl",
                table: "TuitionInvoices",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PaymentSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BankId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BankName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AccountNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AccountName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    VietQrTemplate = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "compact2"),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentSettings", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "PaymentSettings",
                columns: new[] { "Id", "AccountName", "AccountNo", "BankId", "BankName", "CreatedAtUtc", "CreatedByUserId", "IsActive", "IsDefault", "UpdatedAtUtc", "UpdatedByUserId", "VietQrTemplate" },
                values: new object[] { new Guid("99999999-9999-9999-9999-999999999999"), "LE DOAN GIA HUNG", "1021965186", "vietcombank", "Vietcombank", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, true, true, null, null, "compact2" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentSettings");

            migrationBuilder.DropColumn(
                name: "FinalAmount",
                table: "TuitionInvoices");

            migrationBuilder.DropColumn(
                name: "IsVietQrOutdated",
                table: "TuitionInvoices");

            migrationBuilder.DropColumn(
                name: "PaymentContent",
                table: "TuitionInvoices");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "TuitionInvoices");

            migrationBuilder.DropColumn(
                name: "VietQrImageUrl",
                table: "TuitionInvoices");
        }
    }
}
