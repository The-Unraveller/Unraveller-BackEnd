using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheUnraveller.Infrastructure.Data.Migrations;

public partial class AddPerformanceIndexes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Index on Dialogues for queries filtering by UserId and MissionId
        migrationBuilder.CreateIndex(
            name: "IX_Dialogues_UserId_MissionId",
            table: "Dialogues",
            columns: new[] { "UserId", "MissionId" });

        // Index on UserProgresses for filtering by UserId and Status (completed missions)
        migrationBuilder.CreateIndex(
            name: "IX_UserProgresses_UserId_Status",
            table: "UserProgresses",
            columns: new[] { "UserId", "Status" });

        // Index on Missions for filtering by ApprovalStatus and Domain (recommendation queries)
        migrationBuilder.CreateIndex(
            name: "IX_Missions_ApprovalStatus_Domain",
            table: "Missions",
            columns: new[] { "ApprovalStatus", "Domain" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Dialogues_UserId_MissionId",
            table: "Dialogues");

        migrationBuilder.DropIndex(
            name: "IX_UserProgresses_UserId_Status",
            table: "UserProgresses");

        migrationBuilder.DropIndex(
            name: "IX_Missions_ApprovalStatus_Domain",
            table: "Missions");
    }
}
