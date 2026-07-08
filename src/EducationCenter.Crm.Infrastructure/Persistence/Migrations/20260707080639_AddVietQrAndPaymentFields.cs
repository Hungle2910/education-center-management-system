using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EducationCenter.Crm.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVietQrAndPaymentFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PaidAmount",
                table: "TuitionInvoices",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentNote",
                table: "TuitionInvoices",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VietQrGeneratedAt",
                table: "TuitionInvoices",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "VietQrOutdated",
                table: "TuitionInvoices",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaidAmount",
                table: "TuitionInvoices");

            migrationBuilder.DropColumn(
                name: "PaymentNote",
                table: "TuitionInvoices");

            migrationBuilder.DropColumn(
                name: "VietQrGeneratedAt",
                table: "TuitionInvoices");

            migrationBuilder.DropColumn(
                name: "VietQrOutdated",
                table: "TuitionInvoices");
        }
    }
}
