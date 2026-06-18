using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TheUnraveller.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWritingCoachSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserProgresses",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "UserProgresses",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "UserProgresses",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "UserProgresses",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "UserProgresses",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "UserProgresses",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "UserProgresses",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "UserProgresses",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "UserProgresses",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "UserProgresses",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "UserProgresses",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "UserProgresses",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "UserProgresses",
                keyColumn: "Id",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "UserProgresses",
                keyColumn: "Id",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.AddColumn<int>(
                name: "CefrLevel",
                table: "Missions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Domain",
                table: "Missions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinAverageScore",
                table: "Missions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinTurnsToComplete",
                table: "Missions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "WritingObjective",
                table: "Missions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ClarityScore",
                table: "Dialogues",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GrammarScore",
                table: "Dialogues",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NaturalnessScore",
                table: "Dialogues",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StructureScore",
                table: "Dialogues",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ToneScore",
                table: "Dialogues",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VocabularyScore",
                table: "Dialogues",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Corrections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DialogueId = table.Column<int>(type: "integer", nullable: false),
                    Axis = table.Column<int>(type: "integer", nullable: false),
                    OriginalText = table.Column<string>(type: "text", nullable: false),
                    CorrectedText = table.Column<string>(type: "text", nullable: false),
                    Explanation = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Corrections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Corrections_Dialogues_DialogueId",
                        column: x => x.DialogueId,
                        principalTable: "Dialogues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WritingSkillSnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    MissionId = table.Column<int>(type: "integer", nullable: false),
                    AverageScore = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    GrammarScore = table.Column<int>(type: "integer", nullable: false),
                    VocabularyScore = table.Column<int>(type: "integer", nullable: false),
                    ToneScore = table.Column<int>(type: "integer", nullable: false),
                    NaturalnessScore = table.Column<int>(type: "integer", nullable: false),
                    ClarityScore = table.Column<int>(type: "integer", nullable: false),
                    StructureScore = table.Column<int>(type: "integer", nullable: false),
                    TurnsCount = table.Column<int>(type: "integer", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    BestSentence = table.Column<string>(type: "text", nullable: true),
                    AiRewriteSuggestion = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WritingSkillSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WritingSkillSnapshots_Missions_MissionId",
                        column: x => x.MissionId,
                        principalTable: "Missions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WritingSkillSnapshots_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CefrLevel", "Domain", "MinAverageScore", "MinTurnsToComplete", "WritingObjective" },
                values: new object[] { 1, 0, 60, 5, "Sử dụng câu nói lịch sự với 'Would like' hoặc động từ khuyết thiếu 'Could/May'." });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CefrLevel", "Domain", "MinAverageScore", "MinTurnsToComplete", "WritingObjective" },
                values: new object[] { 2, 0, 65, 5, "Sử dụng câu mệnh lệnh (Imperatives) hoặc thể bị động (Passive voice) để xác nhận nhiệm vụ." });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CefrLevel", "Domain", "MinAverageScore", "MinTurnsToComplete", "WritingObjective" },
                values: new object[] { 3, 0, 70, 5, "Sử dụng câu điều kiện loại 1 (If... will...) hoặc loại 2 (If... would...) để đàm phán." });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CefrLevel", "Domain", "MinAverageScore", "MinTurnsToComplete", "WritingObjective" },
                values: new object[] { 3, 0, 75, 6, "Sử dụng câu phức chứa mệnh đề quan hệ (Relative Clauses) hoặc liên từ (Because, Although)." });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CefrLevel", "Domain", "MinAverageScore", "MinTurnsToComplete", "WritingObjective" },
                values: new object[] { 4, 0, 80, 6, "Sử dụng trạng từ mô tả (Descriptive Adverbs) và thì Quá khứ đơn (Past Simple) để báo cáo chứng cứ." });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CefrLevel", "Domain", "MinAverageScore", "MinTurnsToComplete", "WritingObjective" },
                values: new object[] { 5, 2, 85, 7, "Sử dụng câu giả định (Subjunctive Mood) hoặc lối nói gián tiếp (Reported Speech) ở trình độ cao." });

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 1,
                column: "DiscountPriceXp",
                value: 160);

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
                name: "IX_Corrections_Axis",
                table: "Corrections",
                column: "Axis");

            migrationBuilder.CreateIndex(
                name: "IX_Corrections_DialogueId",
                table: "Corrections",
                column: "DialogueId");

            migrationBuilder.CreateIndex(
                name: "IX_WritingSkillSnapshots_MissionId",
                table: "WritingSkillSnapshots",
                column: "MissionId");

            migrationBuilder.CreateIndex(
                name: "IX_WritingSkillSnapshots_UserId",
                table: "WritingSkillSnapshots",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WritingSkillSnapshots_UserId_CompletedAt",
                table: "WritingSkillSnapshots",
                columns: new[] { "UserId", "CompletedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Corrections");

            migrationBuilder.DropTable(
                name: "WritingSkillSnapshots");

            migrationBuilder.DropColumn(
                name: "CefrLevel",
                table: "Missions");

            migrationBuilder.DropColumn(
                name: "Domain",
                table: "Missions");

            migrationBuilder.DropColumn(
                name: "MinAverageScore",
                table: "Missions");

            migrationBuilder.DropColumn(
                name: "MinTurnsToComplete",
                table: "Missions");

            migrationBuilder.DropColumn(
                name: "WritingObjective",
                table: "Missions");

            migrationBuilder.DropColumn(
                name: "ClarityScore",
                table: "Dialogues");

            migrationBuilder.DropColumn(
                name: "GrammarScore",
                table: "Dialogues");

            migrationBuilder.DropColumn(
                name: "NaturalnessScore",
                table: "Dialogues");

            migrationBuilder.DropColumn(
                name: "StructureScore",
                table: "Dialogues");

            migrationBuilder.DropColumn(
                name: "ToneScore",
                table: "Dialogues");

            migrationBuilder.DropColumn(
                name: "VocabularyScore",
                table: "Dialogues");

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 1,
                column: "DiscountPriceXp",
                value: 0);

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
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "Energy", "EnglishLevel", "IsPremium", "LastActiveDate", "LastEnergyRechargedAt", "MaxEnergy", "PasswordHash", "Role", "StreakCount", "Username", "XpBalance" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "khoapro@gmail.com", 100, "B1", false, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 100, "AQAAAAIAAYagAAAAENK5j34f8aH1J11qK7bV5P9mH0Vn0E9G5tWp2e/o9v8u9p8n8=", 0, 0, "KHOA_PRO", 0 },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "minhkhoi@gmail.com", 100, "B1", false, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 100, "AQAAAAIAAYagAAAAENK5j34f8aH1J11qK7bV5P9mH0Vn0E9G5tWp2e/o9v8u9p8n8=", 0, 0, "Minh Khôi", 0 },
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "lananh@gmail.com", 100, "B1", false, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 100, "AQAAAAIAAYagAAAAENK5j34f8aH1J11qK7bV5P9mH0Vn0E9G5tWp2e/o9v8u9p8n8=", 0, 0, "Lan Anh", 0 },
                    { 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "tuankhoa@gmail.com", 100, "B1", false, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 100, "AQAAAAIAAYagAAAAENK5j34f8aH1J11qK7bV5P9mH0Vn0E9G5tWp2e/o9v8u9p8n8=", 0, 0, "Tuấn Khoa", 0 }
                });

            migrationBuilder.InsertData(
                table: "UserProgresses",
                columns: new[] { "Id", "CompletedAt", "CompletionToken", "CurrentSuspicion", "LastActivity", "MissionId", "Status", "TurnCount", "UserId", "XpEarned" },
                values: new object[,]
                {
                    { 10, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "UNRV-SEED-2-1", 15, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 1, 5, 2, 1000 },
                    { 11, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "UNRV-SEED-2-2", 20, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 1, 5, 2, 1200 },
                    { 12, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "UNRV-SEED-2-3", 25, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, 1, 5, 2, 1300 },
                    { 13, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "UNRV-SEED-2-4", 30, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 1, 5, 2, 1300 },
                    { 20, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "UNRV-SEED-3-1", 10, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 1, 5, 3, 950 },
                    { 21, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "UNRV-SEED-3-2", 15, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 1, 5, 3, 1000 },
                    { 22, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "UNRV-SEED-3-3", 20, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, 1, 5, 3, 1000 },
                    { 23, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "UNRV-SEED-3-4", 25, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 1, 5, 3, 1000 },
                    { 30, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "UNRV-SEED-4-1", 20, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 1, 5, 4, 800 },
                    { 31, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "UNRV-SEED-4-2", 22, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 1, 5, 4, 800 },
                    { 32, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "UNRV-SEED-4-3", 25, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, 1, 5, 4, 800 },
                    { 33, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "UNRV-SEED-4-4", 28, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 1, 5, 4, 800 },
                    { 40, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "UNRV-SEED-1-1", 30, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 1, 5, 1, 600 },
                    { 41, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "UNRV-SEED-1-2", 35, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 1, 5, 1, 650 }
                });
        }
    }
}
