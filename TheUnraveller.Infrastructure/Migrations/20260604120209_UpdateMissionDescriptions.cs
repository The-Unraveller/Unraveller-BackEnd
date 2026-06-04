using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheUnraveller.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMissionDescriptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "GrammarTarget" },
                values: new object[] { "*The hum of neon lights fills the cozy cyber-café. The Barista wipes down the metallic counter, looking up with a friendly smile.* \"Welcome to Neon Mug! What can I get started for you today? We've got fresh cyber-brews and synthetic pastries.\"", "Sử dụng câu nói lịch sự với 'Would like' hoặc động từ khuyết thiếu 'Could/May'." });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Description", "GrammarTarget" },
                values: new object[] { "*The Supervisor taps their digital clipboard impatiently as you step into the assembly bay. The neon screens flicker behind them.* \"You're late. We have a heavy shipment of hover-car battery cores to calibrate today. Let me know when you're ready for your instructions.\"", "Sử dụng câu mệnh lệnh (Imperatives) hoặc thể bị động (Passive voice) để xác nhận nhiệm vụ." });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Description", "GrammarTarget" },
                values: new object[] { "*The glass walls of the boardroom overlook the sprawling city skyline. The CEO leans forward, folding their hands.* \"Thank you for coming. We need to reach a deal on the technology sharing agreement. If you agree to our terms, we can sign today. What are your thoughts?\"", "Sử dụng câu điều kiện loại 1 (If... will...) hoặc loại 2 (If... would...) để đàm phán." });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Description", "GrammarTarget" },
                values: new object[] { "*You sit opposite the interviewer in a sleek high-tech office. The HR manager smiles warmly.* \"Welcome. I've reviewed your credentials and they look impressive. To begin, could you tell me why you want to work here at CyberTech Industries?\"", "Sử dụng câu phức chứa mệnh đề quan hệ (Relative Clauses) hoặc liên từ (Because, Although)." });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Description", "GrammarTarget" },
                values: new object[] { "*Rain beats against the dirty precinct window. Chief Detective Henderson tosses a case file containing glowing holograms onto the table.* \"Grab a seat. The cyber-vault at Sector 7 was cracked wide open last night. Tell me exactly what you found at the crime scene.\"", "Sử dụng trạng từ mô tả (Descriptive Adverbs) và thì Quá khứ đơn (Past Simple) để báo cáo chứng cứ." });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Description", "GrammarTarget" },
                values: new object[] { "*You stand in the dim undercity market, surrounded by holographic advertisements. A shady merchant whispers from the shadows.* \"Psst... I hear you're looking for the decryption key. I might have it, but it's going to cost you. What did you bring to trade?\"", "Sử dụng câu giả định (Subjunctive Mood) hoặc lối nói gián tiếp (Reported Speech) ở trình độ cao." });

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
                table: "Missions",
                keyColumn: "Id",
                keyValue: 1,
                column: "Description",
                value: "Hello! Welcome to your English learning journey. Don't worry if you're not perfect yet — everyone starts somewhere.");

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 2,
                column: "Description",
                value: "You've been assigned several tasks today. Listen carefully to each instruction and complete everything with minimal mistakes.");

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 3,
                column: "Description",
                value: "Practice arguing your point and reaching agreements in English in a professional context.");

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 4,
                column: "Description",
                value: "Ace an English job interview with proper vocabulary and confidence in a professional setting.");

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 5,
                column: "Description",
                value: "A crime has been committed. As the lead detective, you must gather evidence, interview suspects, and file your report.");

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 6,
                column: "Description",
                value: "Complex multi-character scenarios with layered objectives to test fluency.");

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
