using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EducationCenter.Crm.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGoogleEventId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GoogleEventId",
                table: "ScheduleOccurrences",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleOccurrences_GoogleEventId",
                table: "ScheduleOccurrences",
                column: "GoogleEventId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ScheduleOccurrences_GoogleEventId",
                table: "ScheduleOccurrences");

            migrationBuilder.DropColumn(
                name: "GoogleEventId",
                table: "ScheduleOccurrences");
        }
    }
}
