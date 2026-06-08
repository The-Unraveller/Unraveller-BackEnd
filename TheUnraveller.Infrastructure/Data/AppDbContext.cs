using Microsoft.EntityFrameworkCore;
using TheUnraveller.Core.Entities;

namespace TheUnraveller.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Npc> Npcs { get; set; } = null!;
    public DbSet<Mission> Missions { get; set; } = null!;
    public DbSet<Dialogue> Dialogues { get; set; } = null!;
    public DbSet<UserProgress> UserProgresses { get; set; } = null!;
    public DbSet<Payment> Payments { get; set; } = null!;
    public DbSet<ShopItem> ShopItems { get; set; } = null!;
    public DbSet<UserInventory> UserInventories { get; set; } = null!;
    public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; } = null!;
    public DbSet<UserSubscription> UserSubscriptions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Explicitly map entities to PostgreSQL tables and identity columns
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityByDefaultColumn();
            entity.Property(e => e.Email).IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone");
            entity.Property(e => e.LastEnergyRechargedAt).HasColumnType("timestamp with time zone");
            entity.Property(e => e.LastActiveDate).HasColumnType("timestamp with time zone");
        });

        modelBuilder.Entity<Npc>(entity =>
        {
            entity.ToTable("Npcs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityByDefaultColumn();
        });

        modelBuilder.Entity<Mission>(entity =>
        {
            entity.ToTable("Missions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityByDefaultColumn();
        });

        modelBuilder.Entity<Dialogue>(entity =>
        {
            entity.ToTable("Dialogues");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityByDefaultColumn();
        });

        modelBuilder.Entity<UserProgress>(entity =>
        {
            entity.ToTable("UserProgresses");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityByDefaultColumn();
            entity.HasIndex(e => new { e.UserId, e.MissionId }).IsUnique();
        });

        modelBuilder.Entity<ShopItem>(entity =>
        {
            entity.ToTable("ShopItems");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityByDefaultColumn();
        });

        modelBuilder.Entity<UserInventory>(entity =>
        {
            entity.ToTable("UserInventories");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityByDefaultColumn();
            entity.HasIndex(e => new { e.UserId, e.ItemId }).IsUnique();
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("Payments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityByDefaultColumn();
        });

        // Seed Subscription Plans
        modelBuilder.Entity<SubscriptionPlan>().HasData(
            new SubscriptionPlan { Id = 1, Name = "Gói Miễn Phí", Tier = SubscriptionTier.Free, Price = 0, DurationDays = 0, Description = "Quyền truy cập miễn phí giới hạn vào các kịch bản bắt đầu", Features = new List<string> { "Kịch bản khởi đầu", "Năng lượng mỗi ngày" } },
            new SubscriptionPlan { Id = 2, Name = "Premium VIP", Tier = SubscriptionTier.MonthlyPremium, Price = 199000, DurationDays = 30, Description = "Mở khóa toàn bộ tính năng và kịch bản cao cấp", Features = new List<string> { "Toàn bộ Kịch bản", "Năng lượng vô cực", "Phản hồi AI nâng cao" } }
        );

        // Configure relationships
        modelBuilder.Entity<Dialogue>()
            .HasOne(d => d.User)
            .WithMany()
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserProgress>()
            .HasOne(up => up.User)
            .WithMany(u => u.Progresses)
            .HasForeignKey(up => up.UserId);

        modelBuilder.Entity<Payment>()
            .HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId);

        modelBuilder.Entity<UserInventory>()
            .HasOne(ui => ui.User)
            .WithMany()
            .HasForeignKey(ui => ui.UserId);

        modelBuilder.Entity<UserInventory>()
            .HasOne(ui => ui.Item)
            .WithMany()
            .HasForeignKey(ui => ui.ItemId);

        modelBuilder.Entity<Mission>()
            .HasOne(m => m.CreatedByUser)
            .WithMany()
            .HasForeignKey(m => m.CreatedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        // Seed NPCs
        modelBuilder.Entity<Npc>().HasData(
            new Npc { Id = 1, Name = "Barista", Role = "Pha chế", Description = "Một nhân viên pha chế thân thiện trong quán cà phê cyberpunk neon rực rỡ.", Personality = "Lịch sự, chu đáo, nhưng dễ bị bối rối trước các yêu cầu phức tạp hoặc hành vi đáng ngờ.", NpcEmoji = "☕" },
            new Npc { Id = 2, Name = "Supervisor", Role = "Giám sát viên", Description = "Giám sát viên vận hành nghiêm khắc kiểm soát hiệu suất làm việc.", Personality = "Nghiêm khắc, chú trọng chi tiết, đòi hỏi sự chính xác tuyệt đối và tiếng Anh chuyên nghiệp cao.", NpcEmoji = "📋" },
            new Npc { Id = 3, Name = "Chief Detective", Role = "Thám tử Trưởng", Description = "Thanh tra kỳ cựu đang phân tích bằng chứng tội phạm.", Personality = "Sắc sảo, hoài nghi, tính phân tích cao, giao tiếp bằng các thuật ngữ thám tử ngắn gọn, trang trọng.", NpcEmoji = "🔍" }
        );

        modelBuilder.Entity<Mission>().HasData(
            new Mission { Id = 1, Title = "Giao tiếp tại Quán Cà phê", Goal = "Luyện tập gọi món, trò chuyện ngắn và tiếng Anh giao tiếp trong quán cà phê.", Description = "*The hum of neon lights fills the cozy cyber-café. The Barista wipes down the metallic counter, looking up with a friendly smile.* \"Welcome to Neon Mug! What can I get started for you today? We've got fresh cyber-brews and synthetic pastries.\"", StartSuspicion = 10, MaxSuspicion = 100, Stage = "Stage 1", Difficulty = "Beginner", XpReward = 150, ImageUrl = "/scenario_coffee.png", Locked = false, NpcId = 1, GrammarTarget = "Sử dụng câu nói lịch sự với 'Would like' hoặc động từ khuyết thiếu 'Could/May'." },
            new Mission { Id = 2, Title = "Làm theo Chỉ dẫn", Goal = "Lắng nghe cẩn thận, hiểu nhiệm vụ và thực hiện với độ chính xác cao.", Description = "*The Supervisor taps their digital clipboard impatiently as you step into the assembly bay. The neon screens flicker behind them.* \"You're late. We have a heavy shipment of hover-car battery cores to calibrate today. Let me know when you're ready for your instructions.\"", StartSuspicion = 10, MaxSuspicion = 100, Stage = "Stage 2", Difficulty = "Beginner", XpReward = 200, ImageUrl = "/scenario_classroom.png", Locked = true, NpcId = 2, GrammarTarget = "Sử dụng câu mệnh lệnh (Imperatives) hoặc thể bị động (Passive voice) để xác nhận nhiệm vụ." },
            new Mission { Id = 3, Title = "Tranh luận & Đàm phán", Goal = "Luyện tập bảo vệ quan điểm và đạt được thỏa thuận bằng tiếng Anh.", Description = "*The glass walls of the boardroom overlook the sprawling city skyline. The CEO leans forward, folding their hands.* \"Thank you for coming. We need to reach a deal on the technology sharing agreement. If you agree to our terms, we can sign today. What are your thoughts?\"", StartSuspicion = 10, MaxSuspicion = 100, Stage = "Stage 3", Difficulty = "Intermediate", XpReward = 300, ImageUrl = "/scenario_boardroom.png", Locked = true, NpcId = 2, GrammarTarget = "Sử dụng câu điều kiện loại 1 (If... will...) hoặc loại 2 (If... would...) để đàm phán." },
            new Mission { Id = 4, Title = "Phỏng vấn Xin việc", Goal = "Vượt qua buổi phỏng vấn xin việc bằng tiếng Anh với vốn từ vựng chuyên nghiệp và tự tin.", Description = "*You sit opposite the interviewer in a sleek high-tech office. The HR manager smiles warmly.* \"Welcome. I've reviewed your credentials and they look impressive. To begin, could you tell me why you want to work here at CyberTech Industries?\"", StartSuspicion = 10, MaxSuspicion = 100, Stage = "Stage 4", Difficulty = "Intermediate", XpReward = 350, ImageUrl = "/scenario_interview.png", Locked = true, NpcId = 2, GrammarTarget = "Sử dụng câu phức chứa mệnh đề quan hệ (Relative Clauses) hoặc liên từ (Because, Although)." },
            new Mission { Id = 5, Title = "Báo cáo Điều tra", Goal = "Mô tả hiện trường vụ án và phá giải các bí ẩn bằng văn bản tiếng Anh.", Description = "*Rain beats against the dirty precinct window. Chief Detective Henderson tosses a case file containing glowing holograms onto the table.* \"Grab a seat. The cyber-vault at Sector 7 was cracked wide open last night. Tell me exactly what you found at the crime scene.\"", StartSuspicion = 10, MaxSuspicion = 100, Stage = "Stage 5", Difficulty = "Advanced", XpReward = 500, ImageUrl = "/scenario_detective.png", Locked = true, NpcId = 3, GrammarTarget = "Sử dụng trạng từ mô tả (Descriptive Adverbs) và thì Quá khứ đơn (Past Simple) để báo cáo chứng cứ." },
            new Mission { Id = 6, Title = "Nhập vai Nâng cao", Goal = "Xử lý các tình huống phức tạp có nhiều nhân vật với mục tiêu đa lớp.", Description = "*You stand in the dim undercity market, surrounded by holographic advertisements. A shady merchant whispers from the shadows.* \"Psst... I hear you're looking for the decryption key. I might have it, but it's going to cost you. What did you bring to trade?\"", StartSuspicion = 10, MaxSuspicion = 100, Stage = "Stage 6", Difficulty = "Advanced", XpReward = 600, ImageUrl = "/scenario_undercity.png", Locked = true, NpcId = 3, GrammarTarget = "Sử dụng câu giả định (Subjunctive Mood) hoặc lối nói gián tiếp (Reported Speech) ở trình độ cao." }
        );

        // Seed ShopItems
        modelBuilder.Entity<ShopItem>().HasData(
            new ShopItem { Id = 1, Name = "Kính Lúp Thám Tử", Description = "Tiết lộ các manh mối và gợi ý ẩn trong các đoạn hội thoại.", Type = ItemType.InGameHint, PriceXp = 200, DiscountPriceXp = 160, Emoji = "🔍" },
            new ShopItem { Id = 2, Name = "Khéo Ăn Khéo Nói", Description = "Giảm ngay lập tức 20 điểm nghi ngờ từ phía NPC.", Type = ItemType.BribeNpc, PriceXp = 500, Emoji = "✨" },
            new ShopItem { Id = 3, Name = "Áo Choàng Bóng Đêm", Description = "Vật phẩm trang trí hiếm có phù hợp cho một điệp viên xâm nhập bậc thầy.", Type = ItemType.Cosmetic, PriceXp = 1000, Emoji = "🧥" }
        );
    }
}


