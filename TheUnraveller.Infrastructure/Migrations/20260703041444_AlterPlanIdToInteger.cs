using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TheUnraveller.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AlterPlanIdToInteger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Cleanup non-numeric PlanId values in Payments table (e.g. "plus", "premium" -> "2")
            migrationBuilder.Sql("UPDATE \"Payments\" SET \"PlanId\" = '2' WHERE \"PlanId\" IS NULL OR \"PlanId\" = '' OR \"PlanId\" !~ '^[0-9]+$';");

            // Drop pre-existing tables to prevent "relation already exists" errors during migration
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"UserSubTaskProgress\" CASCADE;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"MissionSubTasks\" CASCADE;");

            // Custom SQL to cast columns to integer with USING clause to avoid Postgres 42804 error
            migrationBuilder.Sql("ALTER TABLE \"UserSubscriptions\" ALTER COLUMN \"UserId\" TYPE integer USING \"UserId\"::integer;");
            migrationBuilder.Sql("ALTER TABLE \"Payments\" ALTER COLUMN \"PlanId\" TYPE integer USING \"PlanId\"::integer;");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "UserSubscriptions",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "PlanId",
                table: "Payments",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateTable(
                name: "MissionSubTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MissionId = table.Column<int>(type: "integer", nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    Label = table.Column<string>(type: "text", nullable: false),
                    LabelEn = table.Column<string>(type: "text", nullable: false),
                    HintPhrase = table.Column<string>(type: "text", nullable: false),
                    TriggerKeywords = table.Column<List<string>>(type: "text[]", nullable: false),
                    IsOptional = table.Column<bool>(type: "boolean", nullable: false),
                    XpBonus = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MissionSubTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MissionSubTasks_Missions_MissionId",
                        column: x => x.MissionId,
                        principalTable: "Missions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserSubTaskProgress",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    MissionId = table.Column<int>(type: "integer", nullable: false),
                    SubTaskId = table.Column<int>(type: "integer", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSubTaskProgress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSubTaskProgress_MissionSubTasks_SubTaskId",
                        column: x => x.SubTaskId,
                        principalTable: "MissionSubTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserSubTaskProgress_Missions_MissionId",
                        column: x => x.MissionId,
                        principalTable: "Missions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserSubTaskProgress_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ImageUrl", "InitialChoices" },
                values: new object[] { "/scenario_coffee.webp", new List<string> { "I would like to order a cup of coffee, please.", "Could I see the menu, please?", "Do you recommend any house blends today?", "What kind of hot pastries do you have?" } });

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
                columns: new[] { "ImageUrl", "InitialChoices" },
                values: new object[] { "/scenario_coffee.webp", new List<string> { "Hello!", "Can you help me?", "I have a question.", "Let's start." } });

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
                columns: new[] { "ImageUrl", "InitialChoices" },
                values: new object[] { "/scenario_coffee.webp", new List<string> { "Hello!", "Can you help me?", "I have a question.", "Let's start." } });

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

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptions_PlanId",
                table: "UserSubscriptions",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptions_UserId",
                table: "UserSubscriptions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptions_UserId_IsActive",
                table: "UserSubscriptions",
                columns: new[] { "UserId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_MissionSubTasks_MissionId",
                table: "MissionSubTasks",
                column: "MissionId");

            migrationBuilder.CreateIndex(
                name: "IX_MissionSubTasks_MissionId_OrderIndex",
                table: "MissionSubTasks",
                columns: new[] { "MissionId", "OrderIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSubTaskProgress_MissionId",
                table: "UserSubTaskProgress",
                column: "MissionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubTaskProgress_SubTaskId",
                table: "UserSubTaskProgress",
                column: "SubTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubTaskProgress_UserId_MissionId",
                table: "UserSubTaskProgress",
                columns: new[] { "UserId", "MissionId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserSubTaskProgress_UserId_SubTaskId",
                table: "UserSubTaskProgress",
                columns: new[] { "UserId", "SubTaskId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserSubscriptions_SubscriptionPlans_PlanId",
                table: "UserSubscriptions",
                column: "PlanId",
                principalTable: "SubscriptionPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserSubscriptions_Users_UserId",
                table: "UserSubscriptions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserSubscriptions_SubscriptionPlans_PlanId",
                table: "UserSubscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSubscriptions_Users_UserId",
                table: "UserSubscriptions");

            migrationBuilder.DropTable(
                name: "UserSubTaskProgress");

            migrationBuilder.DropTable(
                name: "MissionSubTasks");

            migrationBuilder.DropIndex(
                name: "IX_UserSubscriptions_PlanId",
                table: "UserSubscriptions");

            migrationBuilder.DropIndex(
                name: "IX_UserSubscriptions_UserId",
                table: "UserSubscriptions");

            migrationBuilder.DropIndex(
                name: "IX_UserSubscriptions_UserId_IsActive",
                table: "UserSubscriptions");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserSubscriptions",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "PlanId",
                table: "Payments",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ImageUrl", "InitialChoices" },
                values: new object[] { "/scenario_coffee.png", new List<string> { "I would like to order a cup of coffee, please.", "Could I see the menu, please?", "Do you recommend any house blends today?", "What kind of hot pastries do you have?" } });

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
                columns: new[] { "ImageUrl", "InitialChoices" },
                values: new object[] { "/scenario_coffee.png", new List<string> { "Hello!", "Can you help me?", "I have a question.", "Let's start." } });

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
                columns: new[] { "ImageUrl", "InitialChoices" },
                values: new object[] { "/scenario_coffee.png", new List<string> { "Hello!", "Can you help me?", "I have a question.", "Let's start." } });

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
