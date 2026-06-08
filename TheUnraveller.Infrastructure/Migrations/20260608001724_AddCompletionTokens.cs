using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheUnraveller.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCompletionTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 
        FROM information_schema.columns 
        WHERE table_name='UserProgresses' AND column_name='CompletedAt'
    ) THEN
        ALTER TABLE ""UserProgresses"" ADD ""CompletedAt"" timestamp without time zone;
    END IF;

    IF NOT EXISTS (
        SELECT 1 
        FROM information_schema.columns 
        WHERE table_name='UserProgresses' AND column_name='CompletionToken'
    ) THEN
        ALTER TABLE ""UserProgresses"" ADD ""CompletionToken"" text;
    END IF;
END $$;
");

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 1,
                column: "Features",
                value: new List<string> { "Kịch bản khởi đầu", "Năng lượng mỗi ngày" });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 2,
                column: "Features",
                value: new List<string> { "Toàn bộ Kịch bản", "Năng lượng vô cực", "Phản hồi AI nâng cao" });

            migrationBuilder.UpdateData(
                table: "UserProgresses",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "CompletedAt", "CompletionToken" },
                values: new object[] { new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "UNRV-SEED-2-1" });

            migrationBuilder.UpdateData(
                table: "UserProgresses",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "CompletedAt", "CompletionToken" },
                values: new object[] { new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "UNRV-SEED-2-2" });

            migrationBuilder.UpdateData(
                table: "UserProgresses",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "CompletedAt", "CompletionToken" },
                values: new object[] { new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "UNRV-SEED-2-3" });

            migrationBuilder.UpdateData(
                table: "UserProgresses",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "CompletedAt", "CompletionToken" },
                values: new object[] { new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "UNRV-SEED-2-4" });

            migrationBuilder.UpdateData(
                table: "UserProgresses",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "CompletedAt", "CompletionToken" },
                values: new object[] { new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "UNRV-SEED-3-1" });

            migrationBuilder.UpdateData(
                table: "UserProgresses",
                keyColumn: "Id",
                keyValue: 21,
                columns: new[] { "CompletedAt", "CompletionToken" },
                values: new object[] { new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "UNRV-SEED-3-2" });

            migrationBuilder.UpdateData(
                table: "UserProgresses",
                keyColumn: "Id",
                keyValue: 22,
                columns: new[] { "CompletedAt", "CompletionToken" },
                values: new object[] { new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "UNRV-SEED-3-3" });

            migrationBuilder.UpdateData(
                table: "UserProgresses",
                keyColumn: "Id",
                keyValue: 23,
                columns: new[] { "CompletedAt", "CompletionToken" },
                values: new object[] { new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "UNRV-SEED-3-4" });

            migrationBuilder.UpdateData(
                table: "UserProgresses",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "CompletedAt", "CompletionToken" },
                values: new object[] { new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "UNRV-SEED-4-1" });

            migrationBuilder.UpdateData(
                table: "UserProgresses",
                keyColumn: "Id",
                keyValue: 31,
                columns: new[] { "CompletedAt", "CompletionToken" },
                values: new object[] { new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "UNRV-SEED-4-2" });

            migrationBuilder.UpdateData(
                table: "UserProgresses",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "CompletedAt", "CompletionToken" },
                values: new object[] { new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "UNRV-SEED-4-3" });

            migrationBuilder.UpdateData(
                table: "UserProgresses",
                keyColumn: "Id",
                keyValue: 33,
                columns: new[] { "CompletedAt", "CompletionToken" },
                values: new object[] { new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "UNRV-SEED-4-4" });

            migrationBuilder.UpdateData(
                table: "UserProgresses",
                keyColumn: "Id",
                keyValue: 40,
                columns: new[] { "CompletedAt", "CompletionToken" },
                values: new object[] { new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "UNRV-SEED-1-1" });

            migrationBuilder.UpdateData(
                table: "UserProgresses",
                keyColumn: "Id",
                keyValue: 41,
                columns: new[] { "CompletedAt", "CompletionToken" },
                values: new object[] { new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "UNRV-SEED-1-2" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "UserProgresses");

            migrationBuilder.DropColumn(
                name: "CompletionToken",
                table: "UserProgresses");

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 1,
                column: "Features",
                value: new List<string> { "Kịch bản khởi đầu", "Năng lượng mỗi ngày" });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 2,
                column: "Features",
                value: new List<string> { "Toàn bộ Kịch bản", "Năng lượng vô cực", "Phản hồi AI nâng cao" });
        }
    }
}
