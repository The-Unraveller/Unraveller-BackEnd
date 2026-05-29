using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheUnraveller.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMissionApprovals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 1,
                column: "Features",
                value: new List<string> { "Starter Missions", "Daily Energy" });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 2,
                column: "Features",
                value: new List<string> { "All Missions", "Unlimited Energy", "Advanced AI feedback" });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 3,
                column: "Features",
                value: new List<string> { "All Missions", "Unlimited Energy", "Priority Support", "Certificate" });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 4,
                column: "Features",
                value: new List<string> { "All Missions", "Unlimited Energy", "Lifetime Updates", "VIP Badge" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 1,
                column: "Features",
                value: new List<string> { "Starter Missions", "Daily Energy" });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 2,
                column: "Features",
                value: new List<string> { "All Missions", "Unlimited Energy", "Advanced AI feedback" });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 3,
                column: "Features",
                value: new List<string> { "All Missions", "Unlimited Energy", "Priority Support", "Certificate" });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 4,
                column: "Features",
                value: new List<string> { "All Missions", "Unlimited Energy", "Lifetime Updates", "VIP Badge" });
        }
    }
}
