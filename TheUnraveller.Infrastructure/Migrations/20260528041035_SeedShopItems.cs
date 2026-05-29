using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TheUnraveller.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedShopItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ShopItems",
                columns: new[] { "Id", "Description", "Emoji", "Name", "PriceXp", "Type" },
                values: new object[,]
                {
                    { 1, "Reveals hidden clues and hints in dialogues.", "🔍", "Detective Magnifier", 200, 1 },
                    { 2, "Instantly reduces suspicion by 20 points.", "✨", "Golden Tongue", 500, 2 },
                    { 3, "A rare cosmetic item that fits a master infiltrator.", "🧥", "Shadow Cape", 1000, 3 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 3);
        }
    }
}
