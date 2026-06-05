using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TheUnraveller.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TranslateSeedItemsAndPlans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Goal", "Title" },
                values: new object[] { "Luyện tập gọi món, trò chuyện ngắn và tiếng Anh giao tiếp trong quán cà phê.", "Giao tiếp tại Quán Cà phê" });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Goal", "Locked", "Title" },
                values: new object[] { "Lắng nghe cẩn thận, hiểu nhiệm vụ và thực hiện với độ chính xác cao.", true, "Làm theo Chỉ dẫn" });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Goal", "Title" },
                values: new object[] { "Luyện tập bảo vệ quan điểm và đạt được thỏa thuận bằng tiếng Anh.", "Tranh luận & Đàm phán" });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Goal", "Title" },
                values: new object[] { "Vượt qua buổi phỏng vấn xin việc bằng tiếng Anh với vốn từ vựng chuyên nghiệp và tự tin.", "Phỏng vấn Xin việc" });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Goal", "Locked", "Title" },
                values: new object[] { "Mô tả hiện trường vụ án và phá giải các bí ẩn bằng văn bản tiếng Anh.", true, "Báo cáo Điều tra" });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Goal", "Title" },
                values: new object[] { "Xử lý các tình huống phức tạp có nhiều nhân vật với mục tiêu đa lớp.", "Nhập vai Nâng cao" });

            migrationBuilder.UpdateData(
                table: "Npcs",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "Personality", "Role" },
                values: new object[] { "Một nhân viên pha chế thân thiện trong quán cà phê cyberpunk neon rực rỡ.", "Lịch sự, chu đáo, nhưng dễ bị bối rối trước các yêu cầu phức tạp hoặc hành vi đáng ngờ.", "Pha chế" });

            migrationBuilder.UpdateData(
                table: "Npcs",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Description", "Personality", "Role" },
                values: new object[] { "Giám sát viên vận hành nghiêm khắc kiểm soát hiệu suất làm việc.", "Nghiêm khắc, chú trọng chi tiết, đòi hỏi sự chính xác tuyệt đối và tiếng Anh chuyên nghiệp cao.", "Giám sát viên" });

            migrationBuilder.UpdateData(
                table: "Npcs",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Description", "Personality", "Role" },
                values: new object[] { "Thanh tra kỳ cựu đang phân tích bằng chứng tội phạm.", "Sắc sảo, hoài nghi, tính phân tích cao, giao tiếp bằng các thuật ngữ thám tử ngắn gọn, trang trọng.", "Thám tử Trưởng" });

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Tiết lộ các manh mối và gợi ý ẩn trong các đoạn hội thoại.", "Kính Lúp Thám Tử" });

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Giảm ngay lập tức 20 điểm nghi ngờ từ phía NPC.", "Khéo Ăn Khéo Nói" });

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Vật phẩm trang trí hiếm có phù hợp cho một điệp viên xâm nhập bậc thầy.", "Áo Choàng Bóng Đêm" });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "Features", "Name" },
                values: new object[] { "Quyền truy cập miễn phí giới hạn vào các kịch bản bắt đầu", new List<string> { "Kịch bản khởi đầu", "Năng lượng mỗi ngày" }, "Gói Miễn Phí" });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Description", "Features", "Name", "Price" },
                values: new object[] { "Mở khóa toàn bộ tính năng và kịch bản cao cấp", new List<string> { "Toàn bộ Kịch bản", "Năng lượng vô cực", "Phản hồi AI nâng cao" }, "Premium VIP", 199000m });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Goal", "Title" },
                values: new object[] { "Practice ordering, small talk, and social English in a café setting.", "Coffee Shop Conversations" });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Goal", "Locked", "Title" },
                values: new object[] { "Listen carefully, understand tasks, and execute with precision.", false, "Following Instructions" });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Goal", "Title" },
                values: new object[] { "Practice arguing your point and reaching agreements in English.", "Debate & Negotiation" });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Goal", "Title" },
                values: new object[] { "Ace an English job interview with proper vocabulary and confidence.", "Job Interview" });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Goal", "Locked", "Title" },
                values: new object[] { "Describe scenes and solve mysteries in written English.", false, "Detective Writing" });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Goal", "Title" },
                values: new object[] { "Complex multi-character scenarios with layered objectives.", "Advanced Roleplay" });

            migrationBuilder.UpdateData(
                table: "Npcs",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "Personality", "Role" },
                values: new object[] { "A friendly coffee shop barista in a cyberpunk neon café.", "Polite, helpful, but easily confused by complex orders or suspicious behavior.", "Barista" });

            migrationBuilder.UpdateData(
                table: "Npcs",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Description", "Personality", "Role" },
                values: new object[] { "A strict operations supervisor monitoring efficiency.", "Strict, detail-oriented, expects absolute precision and highly professional English.", "Supervisor" });

            migrationBuilder.UpdateData(
                table: "Npcs",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Description", "Personality", "Role" },
                values: new object[] { "A veteran inspector analyzing crime evidence.", "Sharp, cynical, highly analytical, speaks in short, formal detective terms.", "Chief Detective" });

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Reveals hidden clues and hints in dialogues.", "Detective Magnifier" });

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Instantly reduces suspicion by 20 points.", "Golden Tongue" });

            migrationBuilder.UpdateData(
                table: "ShopItems",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Description", "Name" },
                values: new object[] { "A rare cosmetic item that fits a master infiltrator.", "Shadow Cape" });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "Features", "Name" },
                values: new object[] { "Free access to starter missions", new List<string> { "Starter Missions", "Daily Energy" }, "Basic" });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Description", "Features", "Name", "Price" },
                values: new object[] { "Unlock all features for 30 days", new List<string> { "All Missions", "Unlimited Energy", "Advanced AI feedback" }, "Monthly Premium", 49000m });

            migrationBuilder.InsertData(
                table: "SubscriptionPlans",
                columns: new[] { "Id", "Description", "DurationDays", "Features", "Name", "Price", "Tier" },
                values: new object[,]
                {
                    { 3, "Best value for serious learners", 365, new List<string> { "All Missions", "Unlimited Energy", "Priority Support", "Certificate" }, "Yearly Premium", 450000m, 2 },
                    { 4, "Pay once, access forever", 0, new List<string> { "All Missions", "Unlimited Energy", "Lifetime Updates", "VIP Badge" }, "Lifetime Premium", 1200000m, 3 }
                });
        }
    }
}
