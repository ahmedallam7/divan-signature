using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UUNATRK.Application.Migrations
{
    /// <inheritdoc />
    public partial class AddPenUsageTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "DrawingDistanceMm",
                table: "RequestLogs",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "DrawingDuration",
                table: "RequestLogs",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PenUsageLogId",
                table: "RequestLogs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StrokeCount",
                table: "RequestLogs",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PenUsageLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PenNumber = table.Column<int>(type: "int", nullable: false),
                    InstalledAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalDistanceMm = table.Column<double>(type: "float", nullable: false),
                    TotalPrintJobs = table.Column<int>(type: "int", nullable: false),
                    TotalStrokes = table.Column<int>(type: "int", nullable: false),
                    TotalDrawingTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    WarningThresholdReached = table.Column<bool>(type: "bit", nullable: false),
                    CriticalThresholdReached = table.Column<bool>(type: "bit", nullable: false),
                    ReplacementThresholdReached = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PenUsageLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RequestLogs_PenUsageLogId",
                table: "RequestLogs",
                column: "PenUsageLogId");

            migrationBuilder.CreateIndex(
                name: "IX_PenUsageLogs_InstalledAt",
                table: "PenUsageLogs",
                column: "InstalledAt");

            migrationBuilder.CreateIndex(
                name: "IX_PenUsageLogs_IsActive",
                table: "PenUsageLogs",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_PenUsageLogs_PenNumber",
                table: "PenUsageLogs",
                column: "PenNumber",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RequestLogs_PenUsageLogs_PenUsageLogId",
                table: "RequestLogs",
                column: "PenUsageLogId",
                principalTable: "PenUsageLogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RequestLogs_PenUsageLogs_PenUsageLogId",
                table: "RequestLogs");

            migrationBuilder.DropTable(
                name: "PenUsageLogs");

            migrationBuilder.DropIndex(
                name: "IX_RequestLogs_PenUsageLogId",
                table: "RequestLogs");

            migrationBuilder.DropColumn(
                name: "DrawingDistanceMm",
                table: "RequestLogs");

            migrationBuilder.DropColumn(
                name: "DrawingDuration",
                table: "RequestLogs");

            migrationBuilder.DropColumn(
                name: "PenUsageLogId",
                table: "RequestLogs");

            migrationBuilder.DropColumn(
                name: "StrokeCount",
                table: "RequestLogs");
        }
    }
}
