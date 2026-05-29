using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheUnraveller.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserEnglishLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EnglishLevel",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

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

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "EnglishLevel",
                value: "B1");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "EnglishLevel",
                value: "B1");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "EnglishLevel",
                value: "B1");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4,
                column: "EnglishLevel",
                value: "B1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnglishLevel",
                table: "Users");

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
