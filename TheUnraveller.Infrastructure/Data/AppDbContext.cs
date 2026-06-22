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
    public DbSet<MissionSubTask> MissionSubTasks { get; set; } = null!;
    public DbSet<UserSubTaskProgress> UserSubTaskProgresses { get; set; } = null!;

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
            new Npc { Id = 1, Name = "Barista", Role = "Pha chế", Description = "Một nhân viên pha chế thân thiện trong quán cà phê ấm cúng.", Personality = "Lịch sự, chu đáo, giúp khách hàng chọn lựa đồ uống phù hợp.", NpcEmoji = "☕" },
            new Npc { Id = 2, Name = "Supervisor", Role = "Giám sát viên", Description = "Giám sát viên quản lý tiến độ và phân chia công việc.", Personality = "Chuyên nghiệp, chú trọng chi tiết, yêu cầu độ chính xác trong giao tiếp.", NpcEmoji = "📋" },
            new Npc { Id = 3, Name = "Director", Role = "Giám đốc", Description = "Giám đốc điều hành sắc sảo và giàu kinh nghiệm.", Personality = "Sắc sảo, thực tế, đòi hỏi các báo cáo rõ ràng và giải pháp thiết thực.", NpcEmoji = "👤" }
        );

        modelBuilder.Entity<Mission>().HasData(
            new Mission {
                Id = 1,
                Title = "Giao tiếp tại Quán Cà phê",
                Goal = "Luyện tập gọi món, trò chuyện ngắn và tiếng Anh giao tiếp trong quán cà phê.",
                Description = "*The warm aroma of freshly ground coffee fills the cozy café. The Barista wipes down the wooden counter and looks up with a friendly smile.* \"Welcome to Coffee Corner! What can I get started for you today? We've got fresh house blends and hot pastries.\"",
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
            ,
                InitialChoices = new List<string> { "I would like to order a cup of coffee, please.", "Could I see the menu, please?", "Do you recommend any house blends today?", "What kind of hot pastries do you have?" },
                SyntaxPuzzlesJson = "[]"
            },
            new Mission {
                Id = 2,
                Title = "Làm theo Chỉ dẫn",
                Goal = "Lắng nghe cẩn thận, hiểu nhiệm vụ và thực hiện với độ chính xác cao.",
                Description = "*The Supervisor reviews their clipboard as you step into the stockroom.* \"Glad you're here. We have a large shipment of office equipment to sort and organize today. Let me know when you're ready for your instructions.\"",
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
            ,
                InitialChoices = new List<string> { "Understood. What is the first task I need to do?", "I'm ready. Please give me the instructions.", "Can you guide me on what to do first?", "I will do my best to follow the guidelines." },
                SyntaxPuzzlesJson = "[]"
            },
            new Mission {
                Id = 3,
                Title = "Tranh luận & Đàm phán",
                Goal = "Luyện tập bảo vệ quan điểm và đạt được thỏa thuận bằng tiếng Anh.",
                Description = "*The glass walls of the boardroom overlook the city skyline. The CEO leans forward, folding their hands.* \"Thank you for coming. We need to reach a deal on the technology sharing agreement. If you agree to our terms, we can sign today. What are your thoughts?\"",
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
            ,
                InitialChoices = new List<string> { "I'm ready. Let's start the negotiation.", "Can you explain the main points of the agreement?", "I'd like to discuss the terms of this deal.", "Let's look at the topic from both sides." },
                SyntaxPuzzlesJson = "[]"
            },
            new Mission {
                Id = 4,
                Title = "Phỏng vấn Xin việc",
                Goal = "Vượt qua buổi phỏng vấn xin việc bằng tiếng Anh với vốn từ vựng chuyên nghiệp và tự tin.",
                Description = "*You sit opposite the interviewer in a professional corporate office. The HR manager smiles warmly.* \"Welcome. I've reviewed your resume, and it's quite impressive. To begin, could you tell me why you want to work at our company?\"",
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
            ,
                InitialChoices = new List<string> { "Good morning. Thank you for having me today.", "I'm excited to share my experience with you.", "I'm ready for the interview questions.", "Thank you. I'm glad to have this opportunity." },
                SyntaxPuzzlesJson = "[]"
            },
            new Mission {
                Id = 5,
                Title = "Báo cáo Sự cố Công việc",
                Goal = "Mô tả sự cố xảy ra tại nơi làm việc và đề xuất hướng giải quyết bằng tiếng Anh.",
                Description = "*The Director sits at their desk, looking concerned.* \"Welcome, please have a seat. I heard there was an unexpected issue with our client database yesterday. Can you tell me exactly what happened and what steps were taken to resolve it?\"",
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
            ,
                InitialChoices = new List<string> { "I'm on the case. What details do we have?", "Let's start by examining the evidence.", "Where was the victim last seen?", "I'll solve this. What's our first clue." },
                SyntaxPuzzlesJson = "[]"
            },
            new Mission {
                Id = 6,
                Title = "Đàm phán với Nhà cung cấp",
                Goal = "Thương lượng giá cả và các điều khoản giao hàng với nhà cung cấp nguyên liệu.",
                Description = "*You stand in the modern office of a wholesale supplier. The sales representative crosses their arms and smiles.* \"Thanks for coming in. I understand you'd like to place a large order for your manufacturing line. We can discuss volume discounts. What terms did you have in mind?\"",
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
            ,
                InitialChoices = new List<string> { "Hello!", "Can you help me?", "I have a question.", "Let's start." },
                SyntaxPuzzlesJson = "[]"
            },
            // Mission 7: Email Writing
            new Mission {
                Id = 7,
                Title = "Viết Email Công việc",
                Goal = "Tạo email chuyên nghiệp với cấu trúc rõ ràng và ngôn ngữ phù hợp.",
                Description = "*Your supervisor hands you a notes sheet.* \"We need to schedule a team meeting about the Q3 project targets. Draft the invitation email to all department heads. Make it professional but urgent.\"",
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
            ,
                InitialChoices = new List<string> { "Hello!", "Can you help me?", "I have a question.", "Let's start." },
                SyntaxPuzzlesJson = "[]"
            },
            // Mission 8: Presentation Skills
            new Mission {
                Id = 8,
                Title = "Thuyết trình Sản phẩm",
                Goal = "Giới thiệu sản phẩm mới bằng tiếng Anh với cấu trúc logic và từ vựng phong phú.",
                Description = "*The conference room is full of investors. The product manager nods at you.* \"We're counting on you to pitch the new software platform. Remember: features, benefits, market potential. Make it compelling.\"",
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
            ,
                InitialChoices = new List<string> { "Hello!", "Can you help me?", "I have a question.", "Let's start." },
                SyntaxPuzzlesJson = "[]"
            },
            // Mission 9: Negotiation
            new Mission {
                Id = 9,
                Title = "Đàm phán Hợp đồng",
                Goal = "Thương lượng các điều khoản hợp đồng và tìm điểm chung.",
                Description = "*Across the polished table, the potential partner taps the draft agreement.* \"We can offer exclusive distribution rights, but your margin demands are steep. Let's find a middle ground that works for both companies.\"",
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
            ,
                InitialChoices = new List<string> { "Hello!", "Can you help me?", "I have a question.", "Let's start." },
                SyntaxPuzzlesJson = "[]"
            },
            // Mission 10: Social Chat
            new Mission {
                Id = 10,
                Title = "Trò chuyện Xã giao",
                Goal = "Tham gia cuộc trò chuyện thân thiện với từ ngữ tự nhiên và phong cách lịch sự.",
                Description = "*You find yourself at a rooftop reception overlooking the evening skyline. A friendly attendee approaches you.* \"Hi there! What brings you to this event? I don't think we've met before.\"",
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
            ,
                InitialChoices = new List<string> { "Hello!", "Can you help me?", "I have a question.", "Let's start." },
                SyntaxPuzzlesJson = "[]"
            },
            // Mission 11: Academic Discussion
            new Mission {
                Id = 11,
                Title = "Thảo luận Học thuật",
                Goal = "Tham gia thảo luận học thuật với lập luận có cấu trúc và từ nối học thuật.",
                Description = "*The seminar room is filled with researchers. The professor clears their throat.* \"Today's topic: the social impact of automation. I'd like to hear your perspective on the shift in labor markets.\"",
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
            ,
                InitialChoices = new List<string> { "Hello!", "Can you help me?", "I have a question.", "Let's start." },
                SyntaxPuzzlesJson = "[]"
            },
            // Mission 12: Customer Service
            new Mission {
                Id = 12,
                Title = "Dịch vụ Khách hàng",
                Goal = "Giải quyết phàn nàn của khách hàng với thái độ tích cực và giải pháp hiệu quả.",
                Description = "*The customer service portal flashes a notification. An unhappy customer's message appears on the screen:* \"My order was supposed to arrive yesterday! This is unacceptable!\" *You type your response.*",
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
            ,
                InitialChoices = new List<string> { "Hello!", "Can you help me?", "I have a question.", "Let's start." },
                SyntaxPuzzlesJson = "[]"
            },
            // Mission 13: Persuasive Debate
            new Mission {
                Id = 13,
                Title = "Thuyết phục Đồng nghiệp",
                Goal = "Trình bày lập luận thuyết phục để đồng nghiệp đồng ý với phương án quản lý mới.",
                Description = "*The meeting room is set, and your coworker looks skeptical.* \"Obviously, changing our workflow management software will cause too much disruption. Why should we support this change?\" *The team leads turn to look at you.*",
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
            ,
                InitialChoices = new List<string> { "Hello!", "Can you help me?", "I have a question.", "Let's start." },
                SyntaxPuzzlesJson = "[]"
            },
            // Mission 14: Networking Event
            new Mission {
                Id = 14,
                Title = "Sự kiện Kết nối",
                Goal = "Tạo ấn tượng tốt với các chuyên gia qua cuộc trò chuyện ngắn ngủi.",
                Description = "*You're at the annual business mixer. An investor approaches you.* \"I heard your team is working on a new data security solution. Sounds ambitious. Tell me, what makes your team unique?\"",
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
            ,
                InitialChoices = new List<string> { "Hello!", "Can you help me?", "I have a question.", "Let's start." },
                SyntaxPuzzlesJson = "[]"
            },
            // Mission 15: Conference Q&A
            new Mission {
                Id = 15,
                Title = "Hỏi & Đáp Hội nghị",
                Goal = "Đặt câu hỏi học thuật và nhận xét về bài thuyết trình.",
                Description = "*After the keynote on cloud computing, the speaker opens the floor. You raise your hand.* \"Professor Chen, your research mentions cost efficiency. How do you respond to concerns regarding data privacy?\"",
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
            ,
                InitialChoices = new List<string> { "Hello!", "Can you help me?", "I have a question.", "Let's start." },
                SyntaxPuzzlesJson = "[]"
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
            new ShopItem { Id = 1, Name = "Gợi ý thông thái", Description = "Tiết lộ các gợi ý từ vựng và câu trả lời hữu ích trong hội thoại.", Type = ItemType.InGameHint, PriceXp = 200, DiscountPriceXp = 160, Emoji = "💡" },
            new ShopItem { Id = 2, Name = "Từ điển ngoại giao", Description = "Giảm bớt sự bối rối hoặc mức nghi ngờ từ đối phương.", Type = ItemType.BribeNpc, PriceXp = 500, Emoji = "📕" },
            new ShopItem { Id = 3, Name = "Huy hiệu Vàng", Description = "Vật phẩm lưu niệm vàng danh giá dành cho người học xuất sắc.", Type = ItemType.Cosmetic, PriceXp = 1000, Emoji = "🏅" }
        );

        // MissionSubTask configuration
        modelBuilder.Entity<MissionSubTask>(entity =>
        {
            entity.ToTable("MissionSubTasks");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityByDefaultColumn();
            entity.Property(e => e.TriggerKeywords).HasColumnType("text[]");
            entity.HasOne(e => e.Mission)
                .WithMany(m => m.SubTasks)
                .HasForeignKey(e => e.MissionId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.MissionId, e.OrderIndex }).IsUnique();
            entity.HasIndex(e => e.MissionId);
        });

        // UserSubTaskProgress configuration
        modelBuilder.Entity<UserSubTaskProgress>(entity =>
        {
            entity.ToTable("UserSubTaskProgress");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityByDefaultColumn();
            entity.Property(e => e.CompletedAt).HasColumnType("timestamp with time zone");
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.SubTask)
                .WithMany()
                .HasForeignKey(e => e.SubTaskId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.UserId, e.SubTaskId }).IsUnique();
            entity.HasIndex(e => new { e.UserId, e.MissionId });
        });
    }
}


