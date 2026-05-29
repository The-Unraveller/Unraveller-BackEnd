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
            new SubscriptionPlan { Id = 1, Name = "Basic", Tier = SubscriptionTier.Free, Price = 0, DurationDays = 0, Description = "Free access to starter missions", Features = new List<string> { "Starter Missions", "Daily Energy" } },
            new SubscriptionPlan { Id = 2, Name = "Monthly Premium", Tier = SubscriptionTier.MonthlyPremium, Price = 49000, DurationDays = 30, Description = "Unlock all features for 30 days", Features = new List<string> { "All Missions", "Unlimited Energy", "Advanced AI feedback" } },
            new SubscriptionPlan { Id = 3, Name = "Yearly Premium", Tier = SubscriptionTier.YearlyPremium, Price = 450000, DurationDays = 365, Description = "Best value for serious learners", Features = new List<string> { "All Missions", "Unlimited Energy", "Priority Support", "Certificate" } },
            new SubscriptionPlan { Id = 4, Name = "Lifetime Premium", Tier = SubscriptionTier.LifetimePremium, Price = 1200000, DurationDays = 0, Description = "Pay once, access forever", Features = new List<string> { "All Missions", "Unlimited Energy", "Lifetime Updates", "VIP Badge" } }
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

        // Seed default users
        modelBuilder.Entity<User>().HasData(
            new User { Id = 1, Username = "KHOA_PRO", Email = "khoapro@gmail.com", PasswordHash = "AQAAAAIAAYagAAAAECxHpxxx", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), LastEnergyRechargedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), LastActiveDate = null },
            new User { Id = 2, Username = "Minh Khôi", Email = "minhkhoi@gmail.com", PasswordHash = "HASH2", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), LastEnergyRechargedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), LastActiveDate = null },
            new User { Id = 3, Username = "Lan Anh", Email = "lananh@gmail.com", PasswordHash = "HASH3", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), LastEnergyRechargedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), LastActiveDate = null },
            new User { Id = 4, Username = "Tuấn Khoa", Email = "tuankhoa@gmail.com", PasswordHash = "HASH4", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), LastEnergyRechargedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), LastActiveDate = null }
        );

        // Seed NPCs
        modelBuilder.Entity<Npc>().HasData(
            new Npc { Id = 1, Name = "Barista", Role = "Barista", Description = "A friendly coffee shop barista in a cyberpunk neon café.", Personality = "Polite, helpful, but easily confused by complex orders or suspicious behavior.", NpcEmoji = "☕" },
            new Npc { Id = 2, Name = "Supervisor", Role = "Supervisor", Description = "A strict operations supervisor monitoring efficiency.", Personality = "Strict, detail-oriented, expects absolute precision and highly professional English.", NpcEmoji = "📋" },
            new Npc { Id = 3, Name = "Chief Detective", Role = "Chief Detective", Description = "A veteran inspector analyzing crime evidence.", Personality = "Sharp, cynical, highly analytical, speaks in short, formal detective terms.", NpcEmoji = "🔍" }
        );

        // Seed Missions
        modelBuilder.Entity<Mission>().HasData(
            new Mission { Id = 1, Title = "Coffee Shop Conversations", Goal = "Practice ordering, small talk, and social English in a café setting.", Description = "Hello! Welcome to your English learning journey. Don't worry if you're not perfect yet — everyone starts somewhere.", StartSuspicion = 10, MaxSuspicion = 100, Stage = "Stage 1", Difficulty = "Beginner", XpReward = 150, ImageUrl = "/scenario_coffee.png", Locked = false, NpcId = 1 },
            new Mission { Id = 2, Title = "Following Instructions", Goal = "Listen carefully, understand tasks, and execute with precision.", Description = "You've been assigned several tasks today. Listen carefully to each instruction and complete everything with minimal mistakes.", StartSuspicion = 10, MaxSuspicion = 100, Stage = "Stage 2", Difficulty = "Beginner", XpReward = 200, ImageUrl = "/scenario_classroom.png", Locked = false, NpcId = 2 },
            new Mission { Id = 3, Title = "Debate & Negotiation", Goal = "Practice arguing your point and reaching agreements in English.", Description = "Practice arguing your point and reaching agreements in English in a professional context.", StartSuspicion = 10, MaxSuspicion = 100, Stage = "Stage 3", Difficulty = "Intermediate", XpReward = 300, ImageUrl = "", Locked = true, NpcId = 2 },
            new Mission { Id = 4, Title = "Job Interview", Goal = "Ace an English job interview with proper vocabulary and confidence.", Description = "Ace an English job interview with proper vocabulary and confidence in a professional setting.", StartSuspicion = 10, MaxSuspicion = 100, Stage = "Stage 4", Difficulty = "Intermediate", XpReward = 350, ImageUrl = "", Locked = true, NpcId = 2 },
            new Mission { Id = 5, Title = "Detective Writing", Goal = "Describe scenes and solve mysteries in written English.", Description = "A crime has been committed. As the lead detective, you must gather evidence, interview suspects, and file your report.", StartSuspicion = 10, MaxSuspicion = 100, Stage = "Stage 5", Difficulty = "Advanced", XpReward = 500, ImageUrl = "/scenario_detective.png", Locked = false, NpcId = 3 },
            new Mission { Id = 6, Title = "Advanced Roleplay", Goal = "Complex multi-character scenarios with layered objectives.", Description = "Complex multi-character scenarios with layered objectives to test fluency.", StartSuspicion = 10, MaxSuspicion = 100, Stage = "Stage 6", Difficulty = "Advanced", XpReward = 600, ImageUrl = "", Locked = true, NpcId = 3 }
        );

        // Seed user progresses
        modelBuilder.Entity<UserProgress>().HasData(
            new UserProgress { Id = 10, UserId = 2, MissionId = 1, CurrentSuspicion = 15, Status = MissionStatus.Completed, TurnCount = 5, XpEarned = 1000, LastActivity = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new UserProgress { Id = 11, UserId = 2, MissionId = 2, CurrentSuspicion = 20, Status = MissionStatus.Completed, TurnCount = 5, XpEarned = 1200, LastActivity = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new UserProgress { Id = 12, UserId = 2, MissionId = 3, CurrentSuspicion = 25, Status = MissionStatus.Completed, TurnCount = 5, XpEarned = 1300, LastActivity = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new UserProgress { Id = 13, UserId = 2, MissionId = 4, CurrentSuspicion = 30, Status = MissionStatus.Completed, TurnCount = 5, XpEarned = 1300, LastActivity = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new UserProgress { Id = 20, UserId = 3, MissionId = 1, CurrentSuspicion = 10, Status = MissionStatus.Completed, TurnCount = 5, XpEarned = 950, LastActivity = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new UserProgress { Id = 21, UserId = 3, MissionId = 2, CurrentSuspicion = 15, Status = MissionStatus.Completed, TurnCount = 5, XpEarned = 1000, LastActivity = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new UserProgress { Id = 22, UserId = 3, MissionId = 3, CurrentSuspicion = 20, Status = MissionStatus.Completed, TurnCount = 5, XpEarned = 1000, LastActivity = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new UserProgress { Id = 23, UserId = 3, MissionId = 4, CurrentSuspicion = 25, Status = MissionStatus.Completed, TurnCount = 5, XpEarned = 1000, LastActivity = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new UserProgress { Id = 30, UserId = 4, MissionId = 1, CurrentSuspicion = 20, Status = MissionStatus.Completed, TurnCount = 5, XpEarned = 800, LastActivity = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new UserProgress { Id = 31, UserId = 4, MissionId = 2, CurrentSuspicion = 22, Status = MissionStatus.Completed, TurnCount = 5, XpEarned = 800, LastActivity = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new UserProgress { Id = 32, UserId = 4, MissionId = 3, CurrentSuspicion = 25, Status = MissionStatus.Completed, TurnCount = 5, XpEarned = 800, LastActivity = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new UserProgress { Id = 33, UserId = 4, MissionId = 4, CurrentSuspicion = 28, Status = MissionStatus.Completed, TurnCount = 5, XpEarned = 800, LastActivity = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new UserProgress { Id = 40, UserId = 1, MissionId = 1, CurrentSuspicion = 30, Status = MissionStatus.Completed, TurnCount = 5, XpEarned = 600, LastActivity = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new UserProgress { Id = 41, UserId = 1, MissionId = 2, CurrentSuspicion = 35, Status = MissionStatus.Completed, TurnCount = 5, XpEarned = 650, LastActivity = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );

        // Seed ShopItems
        modelBuilder.Entity<ShopItem>().HasData(
            new ShopItem { Id = 1, Name = "Detective Magnifier", Description = "Reveals hidden clues and hints in dialogues.", Type = ItemType.InGameHint, PriceXp = 200, Emoji = "🔍" },
            new ShopItem { Id = 2, Name = "Golden Tongue", Description = "Instantly reduces suspicion by 20 points.", Type = ItemType.BribeNpc, PriceXp = 500, Emoji = "✨" },
            new ShopItem { Id = 3, Name = "Shadow Cape", Description = "A rare cosmetic item that fits a master infiltrator.", Type = ItemType.Cosmetic, PriceXp = 1000, Emoji = "🧥" }
        );
    }
}
