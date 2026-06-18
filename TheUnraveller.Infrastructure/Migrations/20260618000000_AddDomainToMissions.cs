using Microsoft.EntityFrameworkCore.Migrations;

namespace TheUnraveller.Infrastructure.Migrations;

public partial class AddDomainToMissions : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // 1. Add Domain column (int, not null, default 0)
        migrationBuilder.AddColumn<int>(
            name: "Domain",
            table: "Missions",
            type: "integer",
            nullable: false,
            defaultValue: 0);

        // 2. Update Domain for existing missions based on Stage/Id
        // Domain mapping: 0=Professional, 1=Academic, 2=Social
        migrationBuilder.UpdateData(
            table: "Missions",
            keyColumn: "Id",
            keyValue: 1,
            column: "Domain",
            value: 2); // Social

        migrationBuilder.UpdateData(
            table: "Missions",
            keyColumn: "Id",
            keyValue: 2,
            column: "Domain",
            value: 1); // Academic

        migrationBuilder.UpdateData(
            table: "Missions",
            keyColumn: "Id",
            keyValue: 3,
            column: "Domain",
            value: 1); // Academic

        migrationBuilder.UpdateData(
            table: "Missions",
            keyColumn: "Id",
            keyValue: 4,
            column: "Domain",
            value: 0); // Professional

        migrationBuilder.UpdateData(
            table: "Missions",
            keyColumn: "Id",
            keyValue: 5,
            column: "Domain",
            value: 2); // Social

        migrationBuilder.UpdateData(
            table: "Missions",
            keyColumn: "Id",
            keyValue: 6,
            column: "Domain",
            value: 2); // Social

        migrationBuilder.UpdateData(
            table: "Missions",
            keyColumn: "Id",
            keyValue: 7,
            column: "Domain",
            value: 0); // Professional

        migrationBuilder.UpdateData(
            table: "Missions",
            keyColumn: "Id",
            keyValue: 8,
            column: "Domain",
            value: 0); // Professional

        migrationBuilder.UpdateData(
            table: "Missions",
            keyColumn: "Id",
            keyValue: 9,
            column: "Domain",
            value: 0); // Professional

        migrationBuilder.UpdateData(
            table: "Missions",
            keyColumn: "Id",
            keyValue: 10,
            column: "Domain",
            value: 2); // Social

        migrationBuilder.UpdateData(
            table: "Missions",
            keyColumn: "Id",
            keyValue: 11,
            column: "Domain",
            value: 1); // Academic

        migrationBuilder.UpdateData(
            table: "Missions",
            keyColumn: "Id",
            keyValue: 12,
            column: "Domain",
            value: 0); // Professional

        migrationBuilder.UpdateData(
            table: "Missions",
            keyColumn: "Id",
            keyValue: 13,
            column: "Domain",
            value: 1); // Academic

        migrationBuilder.UpdateData(
            table: "Missions",
            keyColumn: "Id",
            keyValue: 14,
            column: "Domain",
            value: 0); // Professional

        migrationBuilder.UpdateData(
            table: "Missions",
            keyColumn: "Id",
            keyValue: 15,
            column: "Domain",
            value: 1); // Academic
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Domain",
            table: "Missions");
    }
}
