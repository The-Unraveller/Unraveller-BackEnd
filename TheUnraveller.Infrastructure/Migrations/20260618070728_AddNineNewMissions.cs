using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TheUnraveller.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNineNewMissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Missions",
                columns: new[] { "Id", "ApprovalStatus", "CefrLevel", "CreatedByUserId", "Description", "Difficulty", "Domain", "Goal", "GrammarTarget", "ImageUrl", "Locked", "MaxSuspicion", "MinAverageScore", "MinTurnsToComplete", "NpcId", "RejectionReason", "Stage", "StartSuspicion", "Title", "WritingObjective", "XpReward" },
                values: new object[,]
                {
                    { 7, 0, 2, null, "*Your supervisor hands you a crumpled note.* \"We need to schedule a team meeting about the Q3 projections. Draft the invitation email to all department heads. Make it professional but urgent.\"", "Intermediate", 1, "Tạo email chuyên nghiệp với cấu trúc rõ ràng và ngôn ngữ phù hợp.", "Sử dụng câu bị động (Passive Voice) và từ nối (Furthermore, However) trong văn bản hành chính.", "/scenario_email.png", true, 100, 65, 5, 2, null, "Stage 7", 10, "Viết Email Công việc", "Sử dụng câu bị động (Passive Voice) và từ nối (Furthermore, However) trong văn bản hành chính.", 280 },
                    { 8, 0, 3, null, "*The conference room is full of investors. The product manager nods at you.* \"We're counting on you to pitch the new neural interface. Remember: features, benefits, market potential. Make it compelling.\"", "Advanced", 0, "Giới thiệu sản phẩm mới bằng tiếng Anh với cấu trúc logic và từ vựng phong phú.", "Sử dụng thì tương lai đơn (Future Simple) và câu so sánh hơn (Comparative forms) để mô tả ưu điểm.", "/scenario_presentation.png", true, 100, 70, 6, 2, null, "Stage 8", 10, "Thuyết trình Sản phẩm", "Sử dụng thì tương lai đơn (Future Simple) và câu so sánh hơn (Comparative forms) để mô tả ưu điểm.", 400 },
                    { 9, 0, 4, null, "*Across the polished table, the potential partner taps a holographic contract.* \"We can offer exclusive distribution rights, but your margin demands are steep. Let's find a middle ground that works for both corporations.\"", "Advanced", 0, "Thương lượng các điều khoản hợp đồng và tìm điểm chung.", "Sử dụng câu điều kiện loại 2 (If I were...) và từ trung lập (Compromise, Concession) trong đàm phán.", "/scenario_negotiation.png", true, 100, 75, 6, 2, null, "Stage 9", 10, "Đàm phán Hợp đồng", "Sử dụng câu điều kiện loại 2 (If I were...) và từ trung lập (Compromise, Concession) trong đàm phán.", 450 },
                    { 10, 0, 1, null, "*You find yourself at a rooftop party overlooking the neon cityscape. A friendly stranger offers you a drink.* \"So, what brings you to the upper levels? Don't see many new faces up here.\"", "Beginner", 2, "Tham gia cuộc trò chuyện thân thiện với từ ngữ tự nhiên và phong cách lịch sự.", "Sử dụng thì quá khứ đơn (Past Simple) để kể chuyện và câu hỏi mở (What about you?).", "/scenario_party.png", true, 100, 55, 5, 3, null, "Stage 10", 10, "Trò chuyện Xã giao", "Sử dụng thì quá khứ đơn (Past Simple) để kể chuyện và câu hỏi mở (What about you?).", 180 },
                    { 11, 0, 4, null, "*The seminar room is filled with researchers. The professor clears their throat.* \"Today's topic: the ethical implications of AI consciousness. I'd like to hear your perspective on the Turing Test limitations.\"", "Advanced", 1, "Tham gia thảo luận học thuật với lập luận có cấu trúc và từ nối học thuật.", "Sử dụng mệnh đề tương quan (Relative Clauses) và từ nối học thuật (Therefore, Consequently).", "/scenario_seminar.png", true, 100, 75, 6, 2, null, "Stage 11", 10, "Thảo luận Học thuật", "Sử dụng mệnh đề tương quan (Relative Clauses) và từ nối học thuật (Therefore, Consequently).", 420 },
                    { 12, 0, 2, null, "*The customer service console flashes red. An irate customer's message scrolls across the screen:* \"My order was supposed to arrive yesterday! This is unacceptable!\" *You type your response.*", "Intermediate", 0, "Giải quyết phàn nàn của khách hàng với thái độ tích cực và giải pháp hiệu quả.", "Sử dụng câu bị động (Passive Voice) và lời xin lỗi lịch sự (I apologize for...).", "/scenario_customer.png", true, 100, 65, 5, 1, null, "Stage 12", 10, "Dịch vụ Khách hàng", "Sử dụng câu bị động (Passive Voice) và lời xin lỗi lịch sự (I apologize for...).", 260 },
                    { 13, 0, 4, null, "*The debate stage is set, cameras rolling. Your opponent smirks.* \"Obviously, the new surveillance law is necessary for security. Anyone who disagrees is naive.\" *The moderator hands you the microphone.*", "Advanced", 1, "Xây dựng lập luận mạnh mẽ với bằng chứng và phản bác quan điểm đối lập.", "Sử dụng câu điều kiện loại 3 (If we had...) và từ nối phản lập (However, On the other hand).", "/scenario_debate.png", true, 100, 78, 7, 3, null, "Stage 13", 10, " tranh luận Chính trị", "Sử dụng câu điều kiện loại 3 (If we had...) và từ nối phản lập (However, On the other hand).", 480 },
                    { 14, 0, 2, null, "*You're at the annual Tech Futures mixer. A venture capitalist approaches the bar.* \"I heard your startup is working on quantum encryption. Sounds ambitious. Tell me, what makes your team unique?\"", "Intermediate", 2, "Tạo ấn tượng tốt với các chuyên gia qua cuộc trò chuyện ngắn ngủi.", "Sử dụng thì hiện tại hoàn thành (Present Perfect) để mô tả thành tích và câu hỏi Follow-up questions.", "/scenario_networking.png", true, 100, 68, 5, 2, null, "Stage 14", 10, "Sự kiện Kết nối", "Sử dụng thì hiện tại hoàn thành (Present Perfect) để mô tả thành tích và câu hỏi Follow-up questions.", 320 },
                    { 15, 0, 4, null, "*After the keynote on neural interfaces, the speaker opens the floor. You raise your hand.* \"Professor Chen, your research mentions ethical concerns. How do you respond to critics who fear mind-reading technology?\"", "Advanced", 1, "Đặt câu hỏi học thuật và nhận xét về bài thuyết trình.", "Sử dụng câu gián tiếp (Indirect Questions) và từ nối học thuật (While, Whereas).", "/scenario_conference.png", true, 100, 72, 6, 2, null, "Stage 15", 10, "Hỏi & Đáp Hội nghị", "Sử dụng câu gián tiếp (Indirect Questions) và từ nối học thuật (While, Whereas).", 380 }
                });

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
            migrationBuilder.DeleteData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 15);

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
