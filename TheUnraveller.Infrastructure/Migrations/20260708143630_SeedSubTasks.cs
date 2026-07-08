using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheUnraveller.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedSubTasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 1,
                column: "InitialChoices",
                value: new List<string> { "I would like to order a cup of coffee, please.", "Could I see the menu, please?", "Do you recommend any house blends today?", "What kind of hot pastries do you have?" });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 2,
                column: "InitialChoices",
                value: new List<string> { "Understood. What is the first task I need to do?", "I'm ready. Please give me the instructions.", "Can you guide me on what to do first?", "I will do my best to follow the guidelines." });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 3,
                column: "InitialChoices",
                value: new List<string> { "I'm ready. Let's start the negotiation.", "Can you explain the main points of the agreement?", "I'd like to discuss the terms of this deal.", "Let's look at the topic from both sides." });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 4,
                column: "InitialChoices",
                value: new List<string> { "Good morning. Thank you for having me today.", "I'm excited to share my experience with you.", "I'm ready for the interview questions.", "Thank you. I'm glad to have this opportunity." });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 5,
                column: "InitialChoices",
                value: new List<string> { "I'm on the case. What details do we have?", "Let's start by examining the evidence.", "Where was the victim last seen?", "I'll solve this. What's our first clue." });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 6,
                column: "InitialChoices",
                value: new List<string> { "Hello!", "Can you help me?", "I have a question.", "Let's start." });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 7,
                column: "InitialChoices",
                value: new List<string> { "Hello!", "Can you help me?", "I have a question.", "Let's start." });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 8,
                column: "InitialChoices",
                value: new List<string> { "Hello!", "Can you help me?", "I have a question.", "Let's start." });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 9,
                column: "InitialChoices",
                value: new List<string> { "Hello!", "Can you help me?", "I have a question.", "Let's start." });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 10,
                column: "InitialChoices",
                value: new List<string> { "Hello!", "Can you help me?", "I have a question.", "Let's start." });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 11,
                column: "InitialChoices",
                value: new List<string> { "Hello!", "Can you help me?", "I have a question.", "Let's start." });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 12,
                column: "InitialChoices",
                value: new List<string> { "Hello!", "Can you help me?", "I have a question.", "Let's start." });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 13,
                column: "InitialChoices",
                value: new List<string> { "Hello!", "Can you help me?", "I have a question.", "Let's start." });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 14,
                column: "InitialChoices",
                value: new List<string> { "Hello!", "Can you help me?", "I have a question.", "Let's start." });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 15,
                column: "InitialChoices",
                value: new List<string> { "Hello!", "Can you help me?", "I have a question.", "Let's start." });

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

            migrationBuilder.InsertData(
                table: "MissionSubTasks",
                columns: new[] { "Id", "MissionId", "OrderIndex", "Label", "LabelEn", "HintPhrase", "TriggerKeywords", "IsOptional", "XpBonus" },
                values: new object[,]
                {
                    { 1, 1, 1, "Chào hỏi và hỏi xem quán hôm nay có món gì đặc biệt", "Greet the barista and ask for recommendations", "Hi! What would you recommend for today?", new List<string> { "recommend", "special", "today", "menu" }, false, 10 },
                    { 2, 1, 2, "Gọi một tách cà phê và hỏi giá tiền", "Order a coffee and ask for the price", "Can I have a coffee, please? How much is it?", new List<string> { "coffee", "much", "price", "cost", "have a" }, false, 15 },
                    { 3, 1, 3, "Hỏi mật khẩu WiFi của quán", "Ask for the WiFi password", "By the way, what is the WiFi password?", new List<string> { "wifi", "wi-fi", "password", "passcode" }, true, 20 },
                    { 4, 2, 1, "Tự giới thiệu bản thân và hỏi vai trò của họ", "Introduce yourself and ask for their role", "Nice to meet you, I'm... What is your role here?", new List<string> { "my name", "meet you", "your role", "what do you do" }, false, 10 },
                    { 5, 2, 2, "Hỏi về các dự án hiện tại của công ty họ", "Ask about their company's current projects", "What projects is your team working on lately?", new List<string> { "project", "working on", "team", "current", "lately" }, false, 15 },
                    { 6, 2, 3, "Đề xuất trao đổi thông tin liên lạc (Email/LinkedIn)", "Suggest exchanging contact details", "Could we exchange emails or connect on LinkedIn?", new List<string> { "contact", "email", "linkedin", "exchange", "connect" }, true, 20 },
                    { 7, 3, 1, "Trình bày vấn đề chính một cách lịch sự", "Politely present the main issue", "I wanted to discuss an issue regarding our schedule.", new List<string> { "discuss", "issue", "problem", "schedule", "timeline" }, false, 10 },
                    { 8, 3, 2, "Đưa ra giải pháp đề xuất", "Offer a proposed solution", "I suggest we reallocate resources to keep on track.", new List<string> { "suggest", "solution", "propose", "reallocate", "resource" }, false, 15 }
                });

            migrationBuilder.Sql("SELECT setval(pg_get_serial_sequence('\"MissionSubTasks\"', 'Id'), COALESCE(MAX(\"Id\"), 1)) FROM \"MissionSubTasks\";");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM \"MissionSubTasks\" WHERE \"Id\" IN (1, 2, 3, 4, 5, 6, 7, 8);");
            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 1,
                column: "InitialChoices",
                value: new List<string> { "I would like to order a cup of coffee, please.", "Could I see the menu, please?", "Do you recommend any house blends today?", "What kind of hot pastries do you have?" });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 2,
                column: "InitialChoices",
                value: new List<string> { "Understood. What is the first task I need to do?", "I'm ready. Please give me the instructions.", "Can you guide me on what to do first?", "I will do my best to follow the guidelines." });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 3,
                column: "InitialChoices",
                value: new List<string> { "I'm ready. Let's start the negotiation.", "Can you explain the main points of the agreement?", "I'd like to discuss the terms of this deal.", "Let's look at the topic from both sides." });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 4,
                column: "InitialChoices",
                value: new List<string> { "Good morning. Thank you for having me today.", "I'm excited to share my experience with you.", "I'm ready for the interview questions.", "Thank you. I'm glad to have this opportunity." });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 5,
                column: "InitialChoices",
                value: new List<string> { "I'm on the case. What details do we have?", "Let's start by examining the evidence.", "Where was the victim last seen?", "I'll solve this. What's our first clue." });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 6,
                column: "InitialChoices",
                value: new List<string> { "Hello!", "Can you help me?", "I have a question.", "Let's start." });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 7,
                column: "InitialChoices",
                value: new List<string> { "Hello!", "Can you help me?", "I have a question.", "Let's start." });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 8,
                column: "InitialChoices",
                value: new List<string> { "Hello!", "Can you help me?", "I have a question.", "Let's start." });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 9,
                column: "InitialChoices",
                value: new List<string> { "Hello!", "Can you help me?", "I have a question.", "Let's start." });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 10,
                column: "InitialChoices",
                value: new List<string> { "Hello!", "Can you help me?", "I have a question.", "Let's start." });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 11,
                column: "InitialChoices",
                value: new List<string> { "Hello!", "Can you help me?", "I have a question.", "Let's start." });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 12,
                column: "InitialChoices",
                value: new List<string> { "Hello!", "Can you help me?", "I have a question.", "Let's start." });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 13,
                column: "InitialChoices",
                value: new List<string> { "Hello!", "Can you help me?", "I have a question.", "Let's start." });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 14,
                column: "InitialChoices",
                value: new List<string> { "Hello!", "Can you help me?", "I have a question.", "Let's start." });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 15,
                column: "InitialChoices",
                value: new List<string> { "Hello!", "Can you help me?", "I have a question.", "Let's start." });

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
