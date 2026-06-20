using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TheUnraveller.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMissionsToRealisticTheme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Badges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Icon = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CriteriaType = table.Column<string>(type: "text", nullable: true),
                    RequiredCount = table.Column<int>(type: "integer", nullable: true),
                    SkillAxis = table.Column<int>(type: "integer", nullable: true),
                    MinAverageScore = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Badges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserBadges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    BadgeId = table.Column<int>(type: "integer", nullable: false),
                    EarnedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBadges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserBadges_Badges_BadgeId",
                        column: x => x.BadgeId,
                        principalTable: "Badges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserBadges_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Badges",
                columns: new[] { "Id", "CriteriaType", "Description", "Icon", "MinAverageScore", "Name", "RequiredCount", "SkillAxis" },
                values: new object[,]
                {
                    { 1, "FirstCompletion", "Complete your first mission", "👣", null, "First Steps", null, null },
                    { 2, "MinAverageScore", "Achieve an average score of 70 or higher on any mission", "🎯", 70, "Skillful", null, null },
                    { 3, "MinAverageScore", "Achieve an average score of 90 or higher on any mission", "💎", 90, "Perfectionist", null, null },
                    { 4, "Streak", "Complete 5 missions in a row without failing", "🔥", null, "Streak Master", 5, null },
                    { 5, "TotalXp", "Earn 1000 total XP", "🏆", null, "Writing Coach", 1000, null },
                    { 6, "PerfectTurn", "Receive all scores above 80 in a single turn", "✨", null, "Polished", null, null },
                    { 7, "DomainDiversity", "Complete missions in all three domains: Professional, Academic, Social", "🌍", null, "Linguist", null, null },
                    { 8, "TotalCompletions", "Complete 10 missions total", "📚", null, "Lifetime Learner", 10, null }
                });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 1,
                column: "Description",
                value: "*The warm aroma of freshly ground coffee fills the cozy café. The Barista wipes down the wooden counter and looks up with a friendly smile.* \"Welcome to Coffee Corner! What can I get started for you today? We've got fresh house blends and hot pastries.\"");

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 2,
                column: "Description",
                value: "*The Supervisor reviews their clipboard as you step into the stockroom.* \"Glad you're here. We have a large shipment of office equipment to sort and organize today. Let me know when you're ready for your instructions.\"");

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 3,
                column: "Description",
                value: "*The glass walls of the boardroom overlook the city skyline. The CEO leans forward, folding their hands.* \"Thank you for coming. We need to reach a deal on the technology sharing agreement. If you agree to our terms, we can sign today. What are your thoughts?\"");

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 4,
                column: "Description",
                value: "*You sit opposite the interviewer in a professional corporate office. The HR manager smiles warmly.* \"Welcome. I've reviewed your resume, and it's quite impressive. To begin, could you tell me why you want to work at our company?\"");

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Description", "Goal", "Title" },
                values: new object[] { "*The Director sits at their desk, looking concerned.* \"Welcome, please have a seat. I heard there was an unexpected issue with our client database yesterday. Can you tell me exactly what happened and what steps were taken to resolve it?\"", "Mô tả sự cố xảy ra tại nơi làm việc và đề xuất hướng giải quyết bằng tiếng Anh.", "Báo cáo Sự cố Công việc" });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Description", "Goal", "Title" },
                values: new object[] { "*You stand in the modern office of a wholesale supplier. The sales representative crosses their arms and smiles.* \"Thanks for coming in. I understand you'd like to place a large order for your manufacturing line. We can discuss volume discounts. What terms did you have in mind?\"", "Thương lượng giá cả và các điều khoản giao hàng với nhà cung cấp nguyên liệu.", "Đàm phán với Nhà cung cấp" });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Description", "ImageUrl" },
                values: new object[] { "*Your supervisor hands you a notes sheet.* \"We need to schedule a team meeting about the Q3 project targets. Draft the invitation email to all department heads. Make it professional but urgent.\"", "/scenario_classroom.png" });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Description", "ImageUrl" },
                values: new object[] { "*The conference room is full of investors. The product manager nods at you.* \"We're counting on you to pitch the new software platform. Remember: features, benefits, market potential. Make it compelling.\"", "/scenario_boardroom.png" });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Description", "ImageUrl" },
                values: new object[] { "*Across the polished table, the potential partner taps the draft agreement.* \"We can offer exclusive distribution rights, but your margin demands are steep. Let's find a middle ground that works for both companies.\"", "/scenario_boardroom.png" });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "Description", "ImageUrl" },
                values: new object[] { "*You find yourself at a rooftop reception overlooking the evening skyline. A friendly attendee approaches you.* \"Hi there! What brings you to this event? I don't think we've met before.\"", "/scenario_coffee.png" });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "Description", "ImageUrl" },
                values: new object[] { "*The seminar room is filled with researchers. The professor clears their throat.* \"Today's topic: the social impact of automation. I'd like to hear your perspective on the shift in labor markets.\"", "/scenario_classroom.png" });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "Description", "ImageUrl" },
                values: new object[] { "*The customer service portal flashes a notification. An unhappy customer's message appears on the screen:* \"My order was supposed to arrive yesterday! This is unacceptable!\" *You type your response.*", "/scenario_classroom.png" });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "Description", "Goal", "ImageUrl", "Title" },
                values: new object[] { "*The meeting room is set, and your coworker looks skeptical.* \"Obviously, changing our workflow management software will cause too much disruption. Why should we support this change?\" *The team leads turn to look at you.*", "Trình bày lập luận thuyết phục để đồng nghiệp đồng ý với phương án quản lý mới.", "/scenario_boardroom.png", "Thuyết phục Đồng nghiệp" });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "Description", "ImageUrl" },
                values: new object[] { "*You're at the annual business mixer. An investor approaches you.* \"I heard your team is working on a new data security solution. Sounds ambitious. Tell me, what makes your team unique?\"", "/scenario_coffee.png" });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "Description", "ImageUrl" },
                values: new object[] { "*After the keynote on cloud computing, the speaker opens the floor. You raise your hand.* \"Professor Chen, your research mentions cost efficiency. How do you respond to concerns regarding data privacy?\"", "/scenario_classroom.png" });

            migrationBuilder.UpdateData(
                table: "Npcs",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "Personality" },
                values: new object[] { "Một nhân viên pha chế thân thiện trong quán cà phê ấm cúng.", "Lịch sự, chu đáo, giúp khách hàng chọn lựa đồ uống phù hợp." });

            migrationBuilder.UpdateData(
                table: "Npcs",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Description", "Personality" },
                values: new object[] { "Giám sát viên quản lý tiến độ và phân chia công việc.", "Chuyên nghiệp, chú trọng chi tiết, yêu cầu độ chính xác trong giao tiếp." });

            migrationBuilder.UpdateData(
                table: "Npcs",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Description", "Name", "NpcEmoji", "Personality", "Role" },
                values: new object[] { "Giám đốc điều hành sắc sảo và giàu kinh nghiệm.", "Director", "👤", "Sắc sảo, thực tế, đòi hỏi các báo cáo rõ ràng và giải pháp thiết thực.", "Giám đốc" });

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "Emoji", "Name" },
                values: new object[] { "Tiết lộ các gợi ý từ vựng và câu trả lời hữu ích trong hội thoại.", "💡", "Gợi ý thông thái" });

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Description", "Emoji", "Name" },
                values: new object[] { "Giảm bớt sự bối rối hoặc mức nghi ngờ từ đối phương.", "📕", "Từ điển ngoại giao" });

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Description", "Emoji", "Name" },
                values: new object[] { "Vật phẩm lưu niệm vàng danh giá dành cho người học xuất sắc.", "🏅", "Huy hiệu Vàng" });

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
                name: "IX_UserBadges_BadgeId",
                table: "UserBadges",
                column: "BadgeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserBadges_UserId",
                table: "UserBadges",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserBadges_UserId_BadgeId",
                table: "UserBadges",
                columns: new[] { "UserId", "BadgeId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserBadges");

            migrationBuilder.DropTable(
                name: "Badges");

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 1,
                column: "Description",
                value: "*The hum of neon lights fills the cozy cyber-café. The Barista wipes down the metallic counter, looking up with a friendly smile.* \"Welcome to Neon Mug! What can I get started for you today? We've got fresh cyber-brews and synthetic pastries.\"");

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 2,
                column: "Description",
                value: "*The Supervisor taps their digital clipboard impatiently as you step into the assembly bay. The neon screens flicker behind them.* \"You're late. We have a heavy shipment of hover-car battery cores to calibrate today. Let me know when you're ready for your instructions.\"");

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 3,
                column: "Description",
                value: "*The glass walls of the boardroom overlook the sprawling city skyline. The CEO leans forward, folding their hands.* \"Thank you for coming. We need to reach a deal on the technology sharing agreement. If you agree to our terms, we can sign today. What are your thoughts?\"");

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 4,
                column: "Description",
                value: "*You sit opposite the interviewer in a sleek high-tech office. The HR manager smiles warmly.* \"Welcome. I've reviewed your credentials and they look impressive. To begin, could you tell me why you want to work here at CyberTech Industries?\"");

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Description", "Goal", "Title" },
                values: new object[] { "*Rain beats against the dirty precinct window. Chief Detective Henderson tosses a case file containing glowing holograms onto the table.* \"Grab a seat. The cyber-vault at Sector 7 was cracked wide open last night. Tell me exactly what you found at the crime scene.\"", "Mô tả hiện trường vụ án và phá giải các bí ẩn bằng văn bản tiếng Anh.", "Báo cáo Điều tra" });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Description", "Goal", "Title" },
                values: new object[] { "*You stand in the dim undercity market, surrounded by holographic advertisements. A shady merchant whispers from the shadows.* \"Psst... I hear you're looking for the decryption key. I might have it, but it's going to cost you. What did you bring to trade?\"", "Xử lý các tình huống phức tạp có nhiều nhân vật với mục tiêu đa lớp.", "Nhập vai Nâng cao" });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Description", "ImageUrl" },
                values: new object[] { "*Your supervisor hands you a crumpled note.* \"We need to schedule a team meeting about the Q3 projections. Draft the invitation email to all department heads. Make it professional but urgent.\"", "/scenario_email.png" });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Description", "ImageUrl" },
                values: new object[] { "*The conference room is full of investors. The product manager nods at you.* \"We're counting on you to pitch the new neural interface. Remember: features, benefits, market potential. Make it compelling.\"", "/scenario_presentation.png" });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Description", "ImageUrl" },
                values: new object[] { "*Across the polished table, the potential partner taps a holographic contract.* \"We can offer exclusive distribution rights, but your margin demands are steep. Let's find a middle ground that works for both corporations.\"", "/scenario_negotiation.png" });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "Description", "ImageUrl" },
                values: new object[] { "*You find yourself at a rooftop party overlooking the neon cityscape. A friendly stranger offers you a drink.* \"So, what brings you to the upper levels? Don't see many new faces up here.\"", "/scenario_party.png" });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "Description", "ImageUrl" },
                values: new object[] { "*The seminar room is filled with researchers. The professor clears their throat.* \"Today's topic: the ethical implications of AI consciousness. I'd like to hear your perspective on the Turing Test limitations.\"", "/scenario_seminar.png" });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "Description", "ImageUrl" },
                values: new object[] { "*The customer service console flashes red. An irate customer's message scrolls across the screen:* \"My order was supposed to arrive yesterday! This is unacceptable!\" *You type your response.*", "/scenario_customer.png" });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "Description", "Goal", "ImageUrl", "Title" },
                values: new object[] { "*The debate stage is set, cameras rolling. Your opponent smirks.* \"Obviously, the new surveillance law is necessary for security. Anyone who disagrees is naive.\" *The moderator hands you the microphone.*", "Xây dựng lập luận mạnh mẽ với bằng chứng và phản bác quan điểm đối lập.", "/scenario_debate.png", " tranh luận Chính trị" });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "Description", "ImageUrl" },
                values: new object[] { "*You're at the annual Tech Futures mixer. A venture capitalist approaches the bar.* \"I heard your startup is working on quantum encryption. Sounds ambitious. Tell me, what makes your team unique?\"", "/scenario_networking.png" });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "Description", "ImageUrl" },
                values: new object[] { "*After the keynote on neural interfaces, the speaker opens the floor. You raise your hand.* \"Professor Chen, your research mentions ethical concerns. How do you respond to critics who fear mind-reading technology?\"", "/scenario_conference.png" });

            migrationBuilder.UpdateData(
                table: "Npcs",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "Personality" },
                values: new object[] { "Một nhân viên pha chế thân thiện trong quán cà phê cyberpunk neon rực rỡ.", "Lịch sự, chu đáo, nhưng dễ bị bối rối trước các yêu cầu phức tạp hoặc hành vi đáng ngờ." });

            migrationBuilder.UpdateData(
                table: "Npcs",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Description", "Personality" },
                values: new object[] { "Giám sát viên vận hành nghiêm khắc kiểm soát hiệu suất làm việc.", "Nghiêm khắc, chú trọng chi tiết, đòi hỏi sự chính xác tuyệt đối và tiếng Anh chuyên nghiệp cao." });

            migrationBuilder.UpdateData(
                table: "Npcs",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Description", "Name", "NpcEmoji", "Personality", "Role" },
                values: new object[] { "Thanh tra kỳ cựu đang phân tích bằng chứng tội phạm.", "Chief Detective", "🔍", "Sắc sảo, hoài nghi, tính phân tích cao, giao tiếp bằng các thuật ngữ thám tử ngắn gọn, trang trọng.", "Thám tử Trưởng" });

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "Emoji", "Name" },
                values: new object[] { "Tiết lộ các manh mối và gợi ý ẩn trong các đoạn hội thoại.", "🔍", "Kính Lúp Thám Tử" });

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Description", "Emoji", "Name" },
                values: new object[] { "Giảm ngay lập tức 20 điểm nghi ngờ từ phía NPC.", "✨", "Khéo Ăn Khéo Nói" });

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Description", "Emoji", "Name" },
                values: new object[] { "Vật phẩm trang trí hiếm có phù hợp cho một điệp viên xâm nhập bậc thầy.", "🧥", "Áo Choàng Bóng Đêm" });

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
