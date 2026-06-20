using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheUnraveller.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMissionChoices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<string>>(
                name: "InitialChoices",
                table: "Missions",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.AddColumn<string>(
                name: "SyntaxPuzzlesJson",
                table: "Missions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "InitialChoices", "SyntaxPuzzlesJson" },
                values: new object[] { new List<string> { "I would like to order a cup of coffee, please.", "Could I see the menu, please?", "Do you recommend any house blends today?", "What kind of hot pastries do you have?" }, "[]" });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "InitialChoices", "SyntaxPuzzlesJson" },
                values: new object[] { new List<string> { "Understood. What is the first task I need to do?", "I'm ready. Please give me the instructions.", "Can you guide me on what to do first?", "I will do my best to follow the guidelines." }, "[]" });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "InitialChoices", "SyntaxPuzzlesJson" },
                values: new object[] { new List<string> { "I'm ready. Let's start the negotiation.", "Can you explain the main points of the agreement?", "I'd like to discuss the terms of this deal.", "Let's look at the topic from both sides." }, "[]" });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "InitialChoices", "SyntaxPuzzlesJson" },
                values: new object[] { new List<string> { "Good morning. Thank you for having me today.", "I'm excited to share my experience with you.", "I'm ready for the interview questions.", "Thank you. I'm glad to have this opportunity." }, "[]" });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "InitialChoices", "SyntaxPuzzlesJson" },
                values: new object[] { new List<string> { "I'm on the case. What details do we have?", "Let's start by examining the evidence.", "Where was the victim last seen?", "I'll solve this. What's our first clue." }, "[]" });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "InitialChoices", "SyntaxPuzzlesJson" },
                values: new object[] { new List<string> { "Hello!", "Can you help me?", "I have a question.", "Let's start." }, "[]" });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "InitialChoices", "SyntaxPuzzlesJson" },
                values: new object[] { new List<string> { "Hello!", "Can you help me?", "I have a question.", "Let's start." }, "[]" });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "InitialChoices", "SyntaxPuzzlesJson" },
                values: new object[] { new List<string> { "Hello!", "Can you help me?", "I have a question.", "Let's start." }, "[]" });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "InitialChoices", "SyntaxPuzzlesJson" },
                values: new object[] { new List<string> { "Hello!", "Can you help me?", "I have a question.", "Let's start." }, "[]" });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "InitialChoices", "SyntaxPuzzlesJson" },
                values: new object[] { new List<string> { "Hello!", "Can you help me?", "I have a question.", "Let's start." }, "[]" });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "InitialChoices", "SyntaxPuzzlesJson" },
                values: new object[] { new List<string> { "Hello!", "Can you help me?", "I have a question.", "Let's start." }, "[]" });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "InitialChoices", "SyntaxPuzzlesJson" },
                values: new object[] { new List<string> { "Hello!", "Can you help me?", "I have a question.", "Let's start." }, "[]" });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "InitialChoices", "SyntaxPuzzlesJson" },
                values: new object[] { new List<string> { "Hello!", "Can you help me?", "I have a question.", "Let's start." }, "[]" });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "InitialChoices", "SyntaxPuzzlesJson" },
                values: new object[] { new List<string> { "Hello!", "Can you help me?", "I have a question.", "Let's start." }, "[]" });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "InitialChoices", "SyntaxPuzzlesJson" },
                values: new object[] { new List<string> { "Hello!", "Can you help me?", "I have a question.", "Let's start." }, "[]" });

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InitialChoices",
                table: "Missions");

            migrationBuilder.DropColumn(
                name: "SyntaxPuzzlesJson",
                table: "Missions");

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
