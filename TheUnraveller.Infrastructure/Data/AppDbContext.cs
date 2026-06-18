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
    public DbSet<WritingSkillSnapshot> WritingSkillSnapshots { get; set; } = null!;
    public DbSet<Correction> Corrections { get; set; } = null!;
    public DbSet<Badge> Badges { get; set; } = null!;
    public DbSet<UserBadge> UserBadges { get; set; } = null!;
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

        // WritingSkillSnapshot configuration
        modelBuilder.Entity<WritingSkillSnapshot>(entity =>
        {
            entity.ToTable("WritingSkillSnapshots");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityByDefaultColumn();
            entity.Property(e => e.AverageScore).HasColumnType("decimal(5,2)");
            entity.HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(s => s.Mission)
                .WithMany()
                .HasForeignKey(s => s.MissionId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(s => s.UserId);
            entity.HasIndex(s => s.MissionId);
            entity.HasIndex(s => new { s.UserId, s.CompletedAt });
        });

        // Correction configuration
        modelBuilder.Entity<Correction>(entity =>
        {
            entity.ToTable("Corrections");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityByDefaultColumn();
            entity.HasOne(c => c.Dialogue)
                .WithMany()
                .HasForeignKey(c => c.DialogueId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(c => c.DialogueId);
            entity.HasIndex(c => c.Axis);
        });

        // Badge configuration
        modelBuilder.Entity<Badge>(entity =>
        {
            entity.ToTable("Badges");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityByDefaultColumn();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Icon).HasMaxLength(50);
        });

        // UserBadge configuration
        modelBuilder.Entity<UserBadge>(entity =>
        {
            entity.ToTable("UserBadges");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityByDefaultColumn();
            entity.HasOne(ub => ub.User)
                .WithMany()
                .HasForeignKey(ub => ub.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(ub => ub.Badge)
                .WithMany()
                .HasForeignKey(ub => ub.BadgeId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(ub => new { ub.UserId, ub.BadgeId }).IsUnique();
            entity.HasIndex(ub => ub.UserId);
        });

        // Seed NPCs
        modelBuilder.Entity<Npc>().HasData(
            new Npc { Id = 1, Name = "Barista", Role = "Pha chế", Description = "Một nhân viên pha chế thân thiện trong quán cà phê cyberpunk neon rực rỡ.", Personality = "Lịch sự, chu đáo, nhưng dễ bị bối rối trước các yêu cầu phức tạp hoặc hành vi đáng ngờ.", NpcEmoji = "☕" },
            new Npc { Id = 2, Name = "Supervisor", Role = "Giám sát viên", Description = "Giám sát viên vận hành nghiêm khắc kiểm soát hiệu suất làm việc.", Personality = "Nghiêm khắc, chú trọng chi tiết, đòi hỏi sự chính xác tuyệt đối và tiếng Anh chuyên nghiệp cao.", NpcEmoji = "📋" },
            new Npc { Id = 3, Name = "Chief Detective", Role = "Thám tử Trưởng", Description = "Thanh tra kỳ cựu đang phân tích bằng chứng tội phạm.", Personality = "Sắc sảo, hoài nghi, tính phân tích cao, giao tiếp bằng các thuật ngữ thám tử ngắn gọn, trang trọng.", NpcEmoji = "🔍" }
        );

        modelBuilder.Entity<Mission>().HasData(
            new Mission {
                Id = 1,
                Title = "Giao tiếp tại Quán Cà phê",
                Goal = "Luyện tập gọi món, trò chuyện ngắn và tiếng Anh giao tiếp trong quán cà phê.",
                Description = "*The hum of neon lights fills the cozy cyber-café. The Barista wipes down the metallic counter, looking up with a friendly smile.* \"Welcome to Neon Mug! What can I get started for you today? We've got fresh cyber-brews and synthetic pastries.\"",
                StartSuspicion = 10,
                MaxSuspicion = 100,
                Stage = "Stage 1",
                Difficulty = "Beginner",
                XpReward = 150,
                ImageUrl = "/scenario_coffee.png",
                Locked = false,
                NpcId = 1,
                GrammarTarget = "Sử dụng câu nói lịch sự với 'Would like' hoặc động từ khuyết thiếu 'Could/May'.",
                WritingObjective = "Sử dụng câu nói lịch sự với 'Would like' hoặc động từ khuyết thiếu 'Could/May'.",
                Domain = DomainType.Professional,
                CefrLevel = CefrLevel.A2,
                MinTurnsToComplete = 5,
                MinAverageScore = 60
            },
            new Mission {
                Id = 2,
                Title = "Làm theo Chỉ dẫn",
                Goal = "Lắng nghe cẩn thận, hiểu nhiệm vụ và thực hiện với độ chính xác cao.",
                Description = "*The Supervisor taps their digital clipboard impatiently as you step into the assembly bay. The neon screens flicker behind them.* \"You're late. We have a heavy shipment of hover-car battery cores to calibrate today. Let me know when you're ready for your instructions.\"",
                StartSuspicion = 10,
                MaxSuspicion = 100,
                Stage = "Stage 2",
                Difficulty = "Beginner",
                XpReward = 200,
                ImageUrl = "/scenario_classroom.png",
                Locked = true,
                NpcId = 2,
                GrammarTarget = "Sử dụng câu mệnh lệnh (Imperatives) hoặc thể bị động (Passive voice) để xác nhận nhiệm vụ.",
                WritingObjective = "Sử dụng câu mệnh lệnh (Imperatives) hoặc thể bị động (Passive voice) để xác nhận nhiệm vụ.",
                Domain = DomainType.Professional,
                CefrLevel = CefrLevel.B1,
                MinTurnsToComplete = 5,
                MinAverageScore = 65
            },
            new Mission {
                Id = 3,
                Title = "Tranh luận & Đàm phán",
                Goal = "Luyện tập bảo vệ quan điểm và đạt được thỏa thuận bằng tiếng Anh.",
                Description = "*The glass walls of the boardroom overlook the sprawling city skyline. The CEO leans forward, folding their hands.* \"Thank you for coming. We need to reach a deal on the technology sharing agreement. If you agree to our terms, we can sign today. What are your thoughts?\"",
                StartSuspicion = 10,
                MaxSuspicion = 100,
                Stage = "Stage 3",
                Difficulty = "Intermediate",
                XpReward = 300,
                ImageUrl = "/scenario_boardroom.png",
                Locked = true,
                NpcId = 2,
                GrammarTarget = "Sử dụng câu điều kiện loại 1 (If... will...) hoặc loại 2 (If... would...) để đàm phán.",
                WritingObjective = "Sử dụng câu điều kiện loại 1 (If... will...) hoặc loại 2 (If... would...) để đàm phán.",
                Domain = DomainType.Professional,
                CefrLevel = CefrLevel.B2,
                MinTurnsToComplete = 5,
                MinAverageScore = 70
            },
            new Mission {
                Id = 4,
                Title = "Phỏng vấn Xin việc",
                Goal = "Vượt qua buổi phỏng vấn xin việc bằng tiếng Anh với vốn từ vựng chuyên nghiệp và tự tin.",
                Description = "*You sit opposite the interviewer in a sleek high-tech office. The HR manager smiles warmly.* \"Welcome. I've reviewed your credentials and they look impressive. To begin, could you tell me why you want to work here at CyberTech Industries?\"",
                StartSuspicion = 10,
                MaxSuspicion = 100,
                Stage = "Stage 4",
                Difficulty = "Intermediate",
                XpReward = 350,
                ImageUrl = "/scenario_interview.png",
                Locked = true,
                NpcId = 2,
                GrammarTarget = "Sử dụng câu phức chứa mệnh đề quan hệ (Relative Clauses) hoặc liên từ (Because, Although).",
                WritingObjective = "Sử dụng câu phức chứa mệnh đề quan hệ (Relative Clauses) hoặc liên từ (Because, Although).",
                Domain = DomainType.Professional,
                CefrLevel = CefrLevel.B2,
                MinTurnsToComplete = 6,
                MinAverageScore = 75
            },
            new Mission {
                Id = 5,
                Title = "Báo cáo Điều tra",
                Goal = "Mô tả hiện trường vụ án và phá giải các bí ẩn bằng văn bản tiếng Anh.",
                Description = "*Rain beats against the dirty precinct window. Chief Detective Henderson tosses a case file containing glowing holograms onto the table.* \"Grab a seat. The cyber-vault at Sector 7 was cracked wide open last night. Tell me exactly what you found at the crime scene.\"",
                StartSuspicion = 10,
                MaxSuspicion = 100,
                Stage = "Stage 5",
                Difficulty = "Advanced",
                XpReward = 500,
                ImageUrl = "/scenario_detective.png",
                Locked = true,
                NpcId = 3,
                GrammarTarget = "Sử dụng trạng từ mô tả (Descriptive Adverbs) và thì Quá khứ đơn (Past Simple) để báo cáo chứng cứ.",
                WritingObjective = "Sử dụng trạng từ mô tả (Descriptive Adverbs) và thì Quá khứ đơn (Past Simple) để báo cáo chứng cứ.",
                Domain = DomainType.Professional,
                CefrLevel = CefrLevel.C1,
                MinTurnsToComplete = 6,
                MinAverageScore = 80
            },
            new Mission {
                Id = 6,
                Title = "Nhập vai Nâng cao",
                Goal = "Xử lý các tình huống phức tạp có nhiều nhân vật với mục tiêu đa lớp.",
                Description = "*You stand in the dim undercity market, surrounded by holographic advertisements. A shady merchant whispers from the shadows.* \"Psst... I hear you're looking for the decryption key. I might have it, but it's going to cost you. What did you bring to trade?\"",
                StartSuspicion = 10,
                MaxSuspicion = 100,
                Stage = "Stage 6",
                Difficulty = "Advanced",
                XpReward = 600,
                ImageUrl = "/scenario_undercity.png",
                Locked = true,
                NpcId = 3,
                GrammarTarget = "Sử dụng câu giả định (Subjunctive Mood) hoặc lối nói gián tiếp (Reported Speech) ở trình độ cao.",
                WritingObjective = "Sử dụng câu giả định (Subjunctive Mood) hoặc lối nói gián tiếp (Reported Speech) ở trình độ cao.",
                Domain = DomainType.Social,
                CefrLevel = CefrLevel.C2,
                MinTurnsToComplete = 7,
                MinAverageScore = 85
            },
            // Mission 7: Email Writing
            new Mission {
                Id = 7,
                Title = "Viết Email Công việc",
                Goal = "Tạo email chuyên nghiệp với cấu trúc rõ ràng và ngôn ngữ phù hợp.",
                Description = "*Your supervisor hands you a crumpled note.* \"We need to schedule a team meeting about the Q3 projections. Draft the invitation email to all department heads. Make it professional but urgent.\"",
                StartSuspicion = 10,
                MaxSuspicion = 100,
                Stage = "Stage 7",
                Difficulty = "Intermediate",
                XpReward = 280,
                ImageUrl = "/scenario_classroom.png",
                Locked = true,
                NpcId = 2,
                GrammarTarget = "Sử dụng câu bị động (Passive Voice) và từ nối (Furthermore, However) trong văn bản hành chính.",
                WritingObjective = "Sử dụng câu bị động (Passive Voice) và từ nối (Furthermore, However) trong văn bản hành chính.",
                Domain = DomainType.Academic,
                CefrLevel = CefrLevel.B1,
                MinTurnsToComplete = 5,
                MinAverageScore = 65
            },
            // Mission 8: Presentation Skills
            new Mission {
                Id = 8,
                Title = "Thuyết trình Sản phẩm",
                Goal = "Giới thiệu sản phẩm mới bằng tiếng Anh với cấu trúc logic và từ vựng phong phú.",
                Description = "*The conference room is full of investors. The product manager nods at you.* \"We're counting on you to pitch the new neural interface. Remember: features, benefits, market potential. Make it compelling.\"",
                StartSuspicion = 10,
                MaxSuspicion = 100,
                Stage = "Stage 8",
                Difficulty = "Advanced",
                XpReward = 400,
                ImageUrl = "/scenario_boardroom.png",
                Locked = true,
                NpcId = 2,
                GrammarTarget = "Sử dụng thì tương lai đơn (Future Simple) và câu so sánh hơn (Comparative forms) để mô tả ưu điểm.",
                WritingObjective = "Sử dụng thì tương lai đơn (Future Simple) và câu so sánh hơn (Comparative forms) để mô tả ưu điểm.",
                Domain = DomainType.Professional,
                CefrLevel = CefrLevel.B2,
                MinTurnsToComplete = 6,
                MinAverageScore = 70
            },
            // Mission 9: Negotiation
            new Mission {
                Id = 9,
                Title = "Đàm phán Hợp đồng",
                Goal = "Thương lượng các điều khoản hợp đồng và tìm điểm chung.",
                Description = "*Across the polished table, the potential partner taps a holographic contract.* \"We can offer exclusive distribution rights, but your margin demands are steep. Let's find a middle ground that works for both corporations.\"",
                StartSuspicion = 10,
                MaxSuspicion = 100,
                Stage = "Stage 9",
                Difficulty = "Advanced",
                XpReward = 450,
                ImageUrl = "/scenario_boardroom.png",
                Locked = true,
                NpcId = 2,
                GrammarTarget = "Sử dụng câu điều kiện loại 2 (If I were...) và từ trung lập (Compromise, Concession) trong đàm phán.",
                WritingObjective = "Sử dụng câu điều kiện loại 2 (If I were...) và từ trung lập (Compromise, Concession) trong đàm phán.",
                Domain = DomainType.Professional,
                CefrLevel = CefrLevel.C1,
                MinTurnsToComplete = 6,
                MinAverageScore = 75
            },
            // Mission 10: Social Chat
            new Mission {
                Id = 10,
                Title = "Trò chuyện Xã giao",
                Goal = "Tham gia cuộc trò chuyện thân thiện với từ ngữ tự nhiên và phong cách lịch sự.",
                Description = "*You find yourself at a rooftop party overlooking the neon cityscape. A friendly stranger offers you a drink.* \"So, what brings you to the upper levels? Don't see many new faces up here.\"",
                StartSuspicion = 10,
                MaxSuspicion = 100,
                Stage = "Stage 10",
                Difficulty = "Beginner",
                XpReward = 180,
                ImageUrl = "/scenario_coffee.png",
                Locked = true,
                NpcId = 3,
                GrammarTarget = "Sử dụng thì quá khứ đơn (Past Simple) để kể chuyện và câu hỏi mở (What about you?).",
                WritingObjective = "Sử dụng thì quá khứ đơn (Past Simple) để kể chuyện và câu hỏi mở (What about you?).",
                Domain = DomainType.Social,
                CefrLevel = CefrLevel.A2,
                MinTurnsToComplete = 5,
                MinAverageScore = 55
            },
            // Mission 11: Academic Discussion
            new Mission {
                Id = 11,
                Title = "Thảo luận Học thuật",
                Goal = "Tham gia thảo luận học thuật với lập luận có cấu trúc và từ nối học thuật.",
                Description = "*The seminar room is filled with researchers. The professor clears their throat.* \"Today's topic: the ethical implications of AI consciousness. I'd like to hear your perspective on the Turing Test limitations.\"",
                StartSuspicion = 10,
                MaxSuspicion = 100,
                Stage = "Stage 11",
                Difficulty = "Advanced",
                XpReward = 420,
                ImageUrl = "/scenario_classroom.png",
                Locked = true,
                NpcId = 2,
                GrammarTarget = "Sử dụng mệnh đề tương quan (Relative Clauses) và từ nối học thuật (Therefore, Consequently).",
                WritingObjective = "Sử dụng mệnh đề tương quan (Relative Clauses) và từ nối học thuật (Therefore, Consequently).",
                Domain = DomainType.Academic,
                CefrLevel = CefrLevel.C1,
                MinTurnsToComplete = 6,
                MinAverageScore = 75
            },
            // Mission 12: Customer Service
            new Mission {
                Id = 12,
                Title = "Dịch vụ Khách hàng",
                Goal = "Giải quyết phàn nàn của khách hàng với thái độ tích cực và giải pháp hiệu quả.",
                Description = "*The customer service console flashes red. An irate customer's message scrolls across the screen:* \"My order was supposed to arrive yesterday! This is unacceptable!\" *You type your response.*",
                StartSuspicion = 10,
                MaxSuspicion = 100,
                Stage = "Stage 12",
                Difficulty = "Intermediate",
                XpReward = 260,
                ImageUrl = "/scenario_classroom.png",
                Locked = true,
                NpcId = 1,
                GrammarTarget = "Sử dụng câu bị động (Passive Voice) và lời xin lỗi lịch sự (I apologize for...).",
                WritingObjective = "Sử dụng câu bị động (Passive Voice) và lời xin lỗi lịch sự (I apologize for...).",
                Domain = DomainType.Professional,
                CefrLevel = CefrLevel.B1,
                MinTurnsToComplete = 5,
                MinAverageScore = 65
            },
            // Mission 13: Debate
            new Mission {
                Id = 13,
                Title = " tranh luận Chính trị",
                Goal = "Xây dựng lập luận mạnh mẽ với bằng chứng và phản bác quan điểm đối lập.",
                Description = "*The debate stage is set, cameras rolling. Your opponent smirks.* \"Obviously, the new surveillance law is necessary for security. Anyone who disagrees is naive.\" *The moderator hands you the microphone.*",
                StartSuspicion = 10,
                MaxSuspicion = 100,
                Stage = "Stage 13",
                Difficulty = "Advanced",
                XpReward = 480,
                ImageUrl = "/scenario_boardroom.png",
                Locked = true,
                NpcId = 3,
                GrammarTarget = "Sử dụng câu điều kiện loại 3 (If we had...) và từ nối phản lập (However, On the other hand).",
                WritingObjective = "Sử dụng câu điều kiện loại 3 (If we had...) và từ nối phản lập (However, On the other hand).",
                Domain = DomainType.Academic,
                CefrLevel = CefrLevel.C1,
                MinTurnsToComplete = 7,
                MinAverageScore = 78
            },
            // Mission 14: Networking Event
            new Mission {
                Id = 14,
                Title = "Sự kiện Kết nối",
                Goal = "Tạo ấn tượng tốt với các chuyên gia qua cuộc trò chuyện ngắn ngủi.",
                Description = "*You're at the annual Tech Futures mixer. A venture capitalist approaches the bar.* \"I heard your startup is working on quantum encryption. Sounds ambitious. Tell me, what makes your team unique?\"",
                StartSuspicion = 10,
                MaxSuspicion = 100,
                Stage = "Stage 14",
                Difficulty = "Intermediate",
                XpReward = 320,
                ImageUrl = "/scenario_coffee.png",
                Locked = true,
                NpcId = 2,
                GrammarTarget = "Sử dụng thì hiện tại hoàn thành (Present Perfect) để mô tả thành tích và câu hỏi Follow-up questions.",
                WritingObjective = "Sử dụng thì hiện tại hoàn thành (Present Perfect) để mô tả thành tích và câu hỏi Follow-up questions.",
                Domain = DomainType.Social,
                CefrLevel = CefrLevel.B1,
                MinTurnsToComplete = 5,
                MinAverageScore = 68
            },
            // Mission 15: Conference Q&A
            new Mission {
                Id = 15,
                Title = "Hỏi & Đáp Hội nghị",
                Goal = "Đặt câu hỏi học thuật và nhận xét về bài thuyết trình.",
                Description = "*After the keynote on neural interfaces, the speaker opens the floor. You raise your hand.* \"Professor Chen, your research mentions ethical concerns. How do you respond to critics who fear mind-reading technology?\"",
                StartSuspicion = 10,
                MaxSuspicion = 100,
                Stage = "Stage 15",
                Difficulty = "Advanced",
                XpReward = 380,
                ImageUrl = "/scenario_classroom.png",
                Locked = true,
                NpcId = 2,
                GrammarTarget = "Sử dụng câu gián tiếp (Indirect Questions) và từ nối học thuật (While, Whereas).",
                WritingObjective = "Sử dụng câu gián tiếp (Indirect Questions) và từ nối học thuật (While, Whereas).",
                Domain = DomainType.Academic,
                CefrLevel = CefrLevel.C1,
                MinTurnsToComplete = 6,
                MinAverageScore = 72
            }
        );

        // Seed Badges
        modelBuilder.Entity<Badge>().HasData(
            new Badge { Id = 1, Name = "First Steps", Description = "Complete your first mission", Icon = "👣", CriteriaType = "FirstCompletion" },
            new Badge { Id = 2, Name = "Skillful", Description = "Achieve an average score of 70 or higher on any mission", Icon = "🎯", CriteriaType = "MinAverageScore", MinAverageScore = 70 },
            new Badge { Id = 3, Name = "Perfectionist", Description = "Achieve an average score of 90 or higher on any mission", Icon = "💎", CriteriaType = "MinAverageScore", MinAverageScore = 90 },
            new Badge { Id = 4, Name = "Streak Master", Description = "Complete 5 missions in a row without failing", Icon = "🔥", CriteriaType = "Streak", RequiredCount = 5 },
            new Badge { Id = 5, Name = "Writing Coach", Description = "Earn 1000 total XP", Icon = "🏆", CriteriaType = "TotalXp", RequiredCount = 1000 },
            new Badge { Id = 6, Name = "Polished", Description = "Receive all scores above 80 in a single turn", Icon = "✨", CriteriaType = "PerfectTurn" },
            new Badge { Id = 7, Name = "Linguist", Description = "Complete missions in all three domains: Professional, Academic, Social", Icon = "🌍", CriteriaType = "DomainDiversity" },
            new Badge { Id = 8, Name = "Lifetime Learner", Description = "Complete 10 missions total", Icon = "📚", CriteriaType = "TotalCompletions", RequiredCount = 10 }
        );

        // Seed ShopItems
        modelBuilder.Entity<ShopItem>().HasData(
            new ShopItem { Id = 1, Name = "Kính Lúp Thám Tử", Description = "Tiết lộ các manh mối và gợi ý ẩn trong các đoạn hội thoại.", Type = ItemType.InGameHint, PriceXp = 200, DiscountPriceXp = 160, Emoji = "🔍" },
            new ShopItem { Id = 2, Name = "Khéo Ăn Khéo Nói", Description = "Giảm ngay lập tức 20 điểm nghi ngờ từ phía NPC.", Type = ItemType.BribeNpc, PriceXp = 500, Emoji = "✨" },
            new ShopItem { Id = 3, Name = "Áo Choàng Bóng Đêm", Description = "Vật phẩm trang trí hiếm có phù hợp cho một điệp viên xâm nhập bậc thầy.", Type = ItemType.Cosmetic, PriceXp = 1000, Emoji = "🧥" }
        );
    }
}


