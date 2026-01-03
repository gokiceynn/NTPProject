using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ListingMonitor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailScheduling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EmailIntervalHours",
                table: "AlertRules",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EnableScheduledEmail",
                table: "AlertRules",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastEmailSentAt",
                table: "AlertRules",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextEmailSendAt",
                table: "AlertRules",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailIntervalHours",
                table: "AlertRules");

            migrationBuilder.DropColumn(
                name: "EnableScheduledEmail",
                table: "AlertRules");

            migrationBuilder.DropColumn(
                name: "LastEmailSentAt",
                table: "AlertRules");

            migrationBuilder.DropColumn(
                name: "NextEmailSendAt",
                table: "AlertRules");
        }
    }
}
