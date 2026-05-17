using Microsoft.EntityFrameworkCore;
using TheUnraveller.Core.Entities;

namespace TheUnraveller.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Npc> Npcs { get; set; }
    public DbSet<Mission> Missions { get; set; }
    public DbSet<Dialogue> Dialogues { get; set; }
    public DbSet<UserProgress> UserProgresses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure relationships if necessary
        modelBuilder.Entity<Dialogue>()
            .HasOne(d => d.User)
            .WithMany()
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserProgress>()
            .HasOne(up => up.User)
            .WithMany(u => u.Progresses)
            .HasForeignKey(up => up.UserId);

        // Seed default users
        modelBuilder.Entity<User>().HasData(
            new User { Id = 1, Username = "KHOA_PRO", Email = "khoapro@gmail.com", PasswordHash = "AQAAAAIAAYagAAAAECxHpxxx", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new User { Id = 2, Username = "Minh Khôi", Email = "minhkhoi@gmail.com", PasswordHash = "HASH2", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new User { Id = 3, Username = "Lan Anh", Email = "lananh@gmail.com", PasswordHash = "HASH3", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new User { Id = 4, Username = "Tuấn Khoa", Email = "tuankhoa@gmail.com", PasswordHash = "HASH4", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );

        // Seed NPCs
        modelBuilder.Entity<Npc>().HasData(
            new Npc
            {
                Id = 1,
                Name = "Barista",
                Role = "Barista",
                Description = "A friendly coffee shop barista in a cyberpunk neon café.",
                Personality = "Polite, helpful, but easily confused by complex orders or suspicious behavior.",
                NpcEmoji = "☕"
            },
            new Npc
            {
                Id = 2,
                Name = "Supervisor",
                Role = "Supervisor",
                Description = "A strict operations supervisor monitoring efficiency.",
                Personality = "Strict, detail-oriented, expects absolute precision and highly professional English.",
                NpcEmoji = "📋"
            },
            new Npc
            {
                Id = 3,
                Name = "Chief Detective",
                Role = "Chief Detective",
                Description = "A veteran inspector analyzing crime evidence.",
                Personality = "Sharp, cynical, highly analytical, speaks in short, formal detective terms.",
                NpcEmoji = "🔍"
            }
        );

        // Seed Missions
        modelBuilder.Entity<Mission>().HasData(
            new Mission
            {
                Id = 1,
                Title = "Coffee Shop Conversations",
                Goal = "Practice ordering, small talk, and social English in a café setting.",
                Description = "Hello! Welcome to your English learning journey. Don't worry if you're not perfect yet — everyone starts somewhere.",
                StartSuspicion = 10,
                MaxSuspicion = 100,
                Stage = "Stage 1",
                Difficulty = "Beginner",
                XpReward = 150,
                ImageUrl = "/scenario_coffee.png",
                Locked = false,
                NpcId = 1
            },
            new Mission
            {
                Id = 2,
                Title = "Following Instructions",
                Goal = "Listen carefully, understand tasks, and execute with precision.",
                Description = "You've been assigned several tasks today. Listen carefully to each instruction and complete everything with minimal mistakes.",
                StartSuspicion = 10,
                MaxSuspicion = 100,
                Stage = "Stage 2",
                Difficulty = "Beginner",
                XpReward = 200,
                ImageUrl = "/scenario_classroom.png",
                Locked = false,
                NpcId = 2
            },
            new Mission
            {
                Id = 3,
                Title = "Debate & Negotiation",
                Goal = "Practice arguing your point and reaching agreements in English.",
                Description = "Practice arguing your point and reaching agreements in English in a professional context.",
                StartSuspicion = 10,
                MaxSuspicion = 100,
                Stage = "Stage 3",
                Difficulty = "Intermediate",
                XpReward = 300,
                ImageUrl = "",
                Locked = true,
                NpcId = 2
            },
            new Mission
            {
                Id = 4,
                Title = "Job Interview",
                Goal = "Ace an English job interview with proper vocabulary and confidence.",
                Description = "Ace an English job interview with proper vocabulary and confidence in a professional setting.",
                StartSuspicion = 10,
                MaxSuspicion = 100,
                Stage = "Stage 4",
                Difficulty = "Intermediate",
                XpReward = 350,
                ImageUrl = "",
                Locked = true,
                NpcId = 2
            },
            new Mission
            {
                Id = 5,
                Title = "Detective Writing",
                Goal = "Describe scenes and solve mysteries in written English.",
                Description = "A crime has been committed. As the lead detective, you must gather evidence, interview suspects, and file your report.",
                StartSuspicion = 10,
                MaxSuspicion = 100,
                Stage = "Stage 5",
                Difficulty = "Advanced",
                XpReward = 500,
                ImageUrl = "/scenario_detective.png",
                Locked = false,
                NpcId = 3
            },
            new Mission
            {
                Id = 6,
                Title = "Advanced Roleplay",
                Goal = "Complex multi-character scenarios with layered objectives.",
                Description = "Complex multi-character scenarios with layered objectives to test fluency.",
                StartSuspicion = 10,
                MaxSuspicion = 100,
                Stage = "Stage 6",
                Difficulty = "Advanced",
                XpReward = 600,
                ImageUrl = "",
                Locked = true,
                NpcId = 3
            }
        );

        // Seed user progresses with earned XP to populate the leaderboard dynamically
        modelBuilder.Entity<UserProgress>().HasData(
            // Minh Khôi (User 2) - 4800 XP total
            new UserProgress { Id = 10, UserId = 2, MissionId = 1, CurrentSuspicion = 15, Status = MissionStatus.Completed, TurnCount = 5, XpEarned = 1000 },
            new UserProgress { Id = 11, UserId = 2, MissionId = 2, CurrentSuspicion = 20, Status = MissionStatus.Completed, TurnCount = 5, XpEarned = 1200 },
            new UserProgress { Id = 12, UserId = 2, MissionId = 3, CurrentSuspicion = 25, Status = MissionStatus.Completed, TurnCount = 5, XpEarned = 1300 },
            new UserProgress { Id = 13, UserId = 2, MissionId = 4, CurrentSuspicion = 30, Status = MissionStatus.Completed, TurnCount = 5, XpEarned = 1300 },

            // Lan Anh (User 3) - 3950 XP total
            new UserProgress { Id = 20, UserId = 3, MissionId = 1, CurrentSuspicion = 10, Status = MissionStatus.Completed, TurnCount = 5, XpEarned = 950 },
            new UserProgress { Id = 21, UserId = 3, MissionId = 2, CurrentSuspicion = 15, Status = MissionStatus.Completed, TurnCount = 5, XpEarned = 1000 },
            new UserProgress { Id = 22, UserId = 3, MissionId = 3, CurrentSuspicion = 20, Status = MissionStatus.Completed, TurnCount = 5, XpEarned = 1000 },
            new UserProgress { Id = 23, UserId = 3, MissionId = 4, CurrentSuspicion = 25, Status = MissionStatus.Completed, TurnCount = 5, XpEarned = 1000 },

            // Tuấn Khoa (User 4) - 3200 XP total
            new UserProgress { Id = 30, UserId = 4, MissionId = 1, CurrentSuspicion = 20, Status = MissionStatus.Completed, TurnCount = 5, XpEarned = 800 },
            new UserProgress { Id = 31, UserId = 4, MissionId = 2, CurrentSuspicion = 22, Status = MissionStatus.Completed, TurnCount = 5, XpEarned = 800 },
            new UserProgress { Id = 32, UserId = 4, MissionId = 3, CurrentSuspicion = 25, Status = MissionStatus.Completed, TurnCount = 5, XpEarned = 800 },
            new UserProgress { Id = 33, UserId = 4, MissionId = 4, CurrentSuspicion = 28, Status = MissionStatus.Completed, TurnCount = 5, XpEarned = 800 },

            // KHOA_PRO (User 1) - 1250 XP starter progress
            new UserProgress { Id = 40, UserId = 1, MissionId = 1, CurrentSuspicion = 30, Status = MissionStatus.Completed, TurnCount = 5, XpEarned = 600 },
            new UserProgress { Id = 41, UserId = 1, MissionId = 2, CurrentSuspicion = 35, Status = MissionStatus.Completed, TurnCount = 5, XpEarned = 650 }
        );
    }
}
