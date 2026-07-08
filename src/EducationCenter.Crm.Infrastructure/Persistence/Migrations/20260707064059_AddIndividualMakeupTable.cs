using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EducationCenter.Crm.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIndividualMakeupTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IndividualMakeups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AbsentOccurrenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MakeupOccurrenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndividualMakeups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndividualMakeups_ScheduleOccurrences_AbsentOccurrenceId",
                        column: x => x.AbsentOccurrenceId,
                        principalTable: "ScheduleOccurrences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IndividualMakeups_ScheduleOccurrences_MakeupOccurrenceId",
                        column: x => x.MakeupOccurrenceId,
                        principalTable: "ScheduleOccurrences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IndividualMakeups_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IndividualMakeups_AbsentOccurrenceId",
                table: "IndividualMakeups",
                column: "AbsentOccurrenceId");

            migrationBuilder.CreateIndex(
                name: "IX_IndividualMakeups_MakeupOccurrenceId",
                table: "IndividualMakeups",
                column: "MakeupOccurrenceId");

            migrationBuilder.CreateIndex(
                name: "IX_IndividualMakeups_StudentId_AbsentOccurrenceId",
                table: "IndividualMakeups",
                columns: new[] { "StudentId", "AbsentOccurrenceId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IndividualMakeups");
        }
    }
}
