using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TheUnraveller.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Npcs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Personality = table.Column<string>(type: "text", nullable: false),
                    NpcEmoji = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Npcs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShopItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    PriceXp = table.Column<int>(type: "integer", nullable: false),
                    Emoji = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Username = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Energy = table.Column<int>(type: "integer", nullable: false),
                    MaxEnergy = table.Column<int>(type: "integer", nullable: false),
                    LastEnergyRechargedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StreakCount = table.Column<int>(type: "integer", nullable: false),
                    LastActiveDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    XpBalance = table.Column<int>(type: "integer", nullable: false),
                    IsPremium = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Missions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Goal = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    StartSuspicion = table.Column<int>(type: "integer", nullable: false),
                    MaxSuspicion = table.Column<int>(type: "integer", nullable: false),
                    Stage = table.Column<string>(type: "text", nullable: false),
                    Difficulty = table.Column<string>(type: "text", nullable: false),
                    XpReward = table.Column<int>(type: "integer", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: false),
                    Locked = table.Column<bool>(type: "boolean", nullable: false),
                    NpcId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Missions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Missions_Npcs_NpcId",
                        column: x => x.NpcId,
                        principalTable: "Npcs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    PlanId = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    PaymentUrl = table.Column<string>(type: "text", nullable: true),
                    OrderId = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserInventories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    ItemId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserInventories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserInventories_ShopItems_ItemId",
                        column: x => x.ItemId,
                        principalTable: "ShopItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserInventories_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Dialogues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    NpcId = table.Column<int>(type: "integer", nullable: false),
                    MissionId = table.Column<int>(type: "integer", nullable: false),
                    PlayerMessage = table.Column<string>(type: "text", nullable: false),
                    NpcResponse = table.Column<string>(type: "text", nullable: false),
                    Feedback = table.Column<string>(type: "text", nullable: false),
                    SuspicionChange = table.Column<int>(type: "integer", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dialogues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dialogues_Missions_MissionId",
                        column: x => x.MissionId,
                        principalTable: "Missions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Dialogues_Npcs_NpcId",
                        column: x => x.NpcId,
                        principalTable: "Npcs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Dialogues_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserProgresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    MissionId = table.Column<int>(type: "integer", nullable: false),
                    CurrentSuspicion = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TurnCount = table.Column<int>(type: "integer", nullable: false),
                    XpEarned = table.Column<int>(type: "integer", nullable: false),
                    LastActivity = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProgresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserProgresses_Missions_MissionId",
                        column: x => x.MissionId,
                        principalTable: "Missions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserProgresses_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Npcs",
                columns: new[] { "Id", "Description", "Name", "NpcEmoji", "Personality", "Role" },
                values: new object[,]
                {
                    { 1, "A friendly coffee shop barista in a cyberpunk neon café.", "Barista", "☕", "Polite, helpful, but easily confused by complex orders or suspicious behavior.", "Barista" },
                    { 2, "A strict operations supervisor monitoring efficiency.", "Supervisor", "📋", "Strict, detail-oriented, expects absolute precision and highly professional English.", "Supervisor" },
                    { 3, "A veteran inspector analyzing crime evidence.", "Chief Detective", "🔍", "Sharp, cynical, highly analytical, speaks in short, formal detective terms.", "Chief Detective" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "Energy", "IsPremium", "LastActiveDate", "LastEnergyRechargedAt", "MaxEnergy", "PasswordHash", "StreakCount", "Username", "XpBalance" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "khoapro@gmail.com", 100, false, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 100, "AQAAAAIAAYagAAAAECxHpxxx", 0, "KHOA_PRO", 0 },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "minhkhoi@gmail.com", 100, false, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 100, "HASH2", 0, "Minh Khôi", 0 },
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "lananh@gmail.com", 100, false, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 100, "HASH3", 0, "Lan Anh", 0 },
                    { 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "tuankhoa@gmail.com", 100, false, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 100, "HASH4", 0, "Tuấn Khoa", 0 }
                });

            migrationBuilder.InsertData(
                table: "Missions",
                columns: new[] { "Id", "Description", "Difficulty", "Goal", "ImageUrl", "Locked", "MaxSuspicion", "NpcId", "Stage", "StartSuspicion", "Title", "XpReward" },
                values: new object[,]
                {
                    { 1, "Hello! Welcome to your English learning journey. Don't worry if you're not perfect yet — everyone starts somewhere.", "Beginner", "Practice ordering, small talk, and social English in a café setting.", "/scenario_coffee.png", false, 100, 1, "Stage 1", 10, "Coffee Shop Conversations", 150 },
                    { 2, "You've been assigned several tasks today. Listen carefully to each instruction and complete everything with minimal mistakes.", "Beginner", "Listen carefully, understand tasks, and execute with precision.", "/scenario_classroom.png", false, 100, 2, "Stage 2", 10, "Following Instructions", 200 },
                    { 3, "Practice arguing your point and reaching agreements in English in a professional context.", "Intermediate", "Practice arguing your point and reaching agreements in English.", "", true, 100, 2, "Stage 3", 10, "Debate & Negotiation", 300 },
                    { 4, "Ace an English job interview with proper vocabulary and confidence in a professional setting.", "Intermediate", "Ace an English job interview with proper vocabulary and confidence.", "", true, 100, 2, "Stage 4", 10, "Job Interview", 350 },
                    { 5, "A crime has been committed. As the lead detective, you must gather evidence, interview suspects, and file your report.", "Advanced", "Describe scenes and solve mysteries in written English.", "/scenario_detective.png", false, 100, 3, "Stage 5", 10, "Detective Writing", 500 },
                    { 6, "Complex multi-character scenarios with layered objectives to test fluency.", "Advanced", "Complex multi-character scenarios with layered objectives.", "", true, 100, 3, "Stage 6", 10, "Advanced Roleplay", 600 }
                });

            migrationBuilder.InsertData(
                table: "UserProgresses",
                columns: new[] { "Id", "CurrentSuspicion", "LastActivity", "MissionId", "Status", "TurnCount", "UserId", "XpEarned" },
                values: new object[,]
                {
                    { 10, 15, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 1, 5, 2, 1000 },
                    { 11, 20, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 1, 5, 2, 1200 },
                    { 12, 25, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, 1, 5, 2, 1300 },
                    { 13, 30, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 1, 5, 2, 1300 },
                    { 20, 10, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 1, 5, 3, 950 },
                    { 21, 15, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 1, 5, 3, 1000 },
                    { 22, 20, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, 1, 5, 3, 1000 },
                    { 23, 25, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 1, 5, 3, 1000 },
                    { 30, 20, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 1, 5, 4, 800 },
                    { 31, 22, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 1, 5, 4, 800 },
                    { 32, 25, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, 1, 5, 4, 800 },
                    { 33, 28, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 1, 5, 4, 800 },
                    { 40, 30, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 1, 5, 1, 600 },
                    { 41, 35, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 1, 5, 1, 650 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Dialogues_MissionId",
                table: "Dialogues",
                column: "MissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Dialogues_NpcId",
                table: "Dialogues",
                column: "NpcId");

            migrationBuilder.CreateIndex(
                name: "IX_Dialogues_UserId",
                table: "Dialogues",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Missions_NpcId",
                table: "Missions",
                column: "NpcId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_UserId",
                table: "Payments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserInventories_ItemId",
                table: "UserInventories",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_UserInventories_UserId",
                table: "UserInventories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProgresses_MissionId",
                table: "UserProgresses",
                column: "MissionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProgresses_UserId",
                table: "UserProgresses",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Dialogues");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "UserInventories");

            migrationBuilder.DropTable(
                name: "UserProgresses");

            migrationBuilder.DropTable(
                name: "ShopItems");

            migrationBuilder.DropTable(
                name: "Missions");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Npcs");
        }
    }
}
