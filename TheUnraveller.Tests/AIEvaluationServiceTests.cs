using System.Net;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using TheUnraveller.Core.Entities;
using TheUnraveller.Infrastructure.Data;
using TheUnraveller.Service.DTOs;
using TheUnraveller.Service.Implementations;
using TheUnraveller.Service.Interfaces;
using Xunit;
using TheUnraveller.Core.Exceptions;

namespace TheUnraveller.Tests;

public class AIEvaluationServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _context;
    private readonly Mock<IConfiguration> _configMock;
    private readonly Mock<IBadgeService> _badgeServiceMock;

    public AIEvaluationServiceTests()
    {
        // 1. Establish SQLite In-Memory Database connection
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();

        // 2. Mock Configuration
        _configMock = new Mock<IConfiguration>();
        _configMock.Setup(c => c["LlmApi:ApiKey"]).Returns("test-api-key");

        // 3. Mock Badge Service
        _badgeServiceMock = new Mock<IBadgeService>();
        _badgeServiceMock.Setup(s => s.AwardBadgesForMissionAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
                         .Returns(Task.CompletedTask);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Close();
    }

    private void SeedDatabase()
    {
        // Ensure default seeded User 1 has clean energy and XP values for testing
        var user = _context.Users.Find(1);
        if (user == null)
        {
            user = new User
            {
                Id = 1,
                Username = "KHOA_PRO",
                Email = "khoapro@gmail.com",
                PasswordHash = "AQAAAAIAAYagAAAAENK5j34f8aH1J11qK7bV5P9mH0Vn0E9G5tWp2e/o9v8u9p8n8=",
                Role = UserRole.User,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Energy = 100,
                MaxEnergy = 100,
                LastEnergyRechargedAt = DateTime.UtcNow,
                StreakCount = 0,
                LastActiveDate = null,
                XpBalance = 0,
                IsPremium = false,
                EnglishLevel = "B1"
            };
            _context.Users.Add(user);
        }
        else
        {
            user.Energy = 100;
            user.MaxEnergy = 100;
            user.LastEnergyRechargedAt = DateTime.UtcNow;
            user.XpBalance = 0;
            user.EnglishLevel = "B1";
            _context.Users.Update(user);
        }

        // Clean up any existing progresses or dialogues to avoid pollution
        var progresses = _context.UserProgresses.ToList();
        _context.UserProgresses.RemoveRange(progresses);

        var dialogues = _context.Dialogues.ToList();
        _context.Dialogues.RemoveRange(dialogues);

        _context.SaveChanges();
    }

    [Fact]
    public async Task EvaluateMessageAsync_ShouldDeductEnergyAndUpdateDatabaseOnSuccess()
    {
        // Arrange
        SeedDatabase();

        // Đã bọc lót tất cả các trường hợp Case Sensitivity (camelCase & PascalCase)
        var geminiResponseText = @"{
            ""npcResponse"": ""Identify yourself!"",
            ""NpcResponse"": ""Identify yourself!"",
            ""feedback"": ""Good grammar, standard greeting."",
            ""Feedback"": ""Good grammar, standard greeting."",
            ""suspicionChange"": 5,
            ""SuspicionChange"": 5,
            ""xpEarned"": 15,
            ""XpEarned"": 15,
            ""writingFeedback"": {
                ""scores"": {
                    ""grammar"": 80,
                    ""vocabulary"": 80,
                    ""tone"": 80,
                    ""naturalness"": 80,
                    ""clarity"": 80,
                    ""structure"": 80
                },
                ""corrections"": [],
                ""rewriteSuggestion"": null,
                ""summary"": ""Good grammar, standard greeting.""
            }
        }";

        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(
                    "event: message_start\n" +
                    "data: {\"type\":\"message_start\"}\n\n" +
                    "event: content_block_start\n" +
                    "data: {\"type\":\"content_block_start\",\"index\":0,\"content_block\":{\"type\":\"text\",\"text\":\"\"}}\n\n" +
                    "event: content_block_delta\n" +
                    "data: {\"type\":\"content_block_delta\",\"index\":0,\"delta\":{\"type\":\"text_delta\",\"text\":\"" + 
                    geminiResponseText.Replace("\"", "\\\"").Replace("\r", "").Replace("\n", " ") + 
                    "\"}}\n\n" +
                    "event: message_stop\n" +
                    "data: {\"type\":\"message_stop\"}\n\n"
                )
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var service = new AIEvaluationService(httpClient, _context, _configMock.Object, _badgeServiceMock.Object);

        // Act
        var result = await service.EvaluateMessageAsync(1, 1, "Hello officer");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Identify yourself!", result.NpcResponse);
        Assert.Equal("* Good grammar, standard greeting.", result.WritingFeedback.Summary);
        Assert.Equal(15, result.NewSuspicionLevel); // 10 (start) + 5 (change)
        Assert.Equal(15, result.XpEarned);

        // Verify database updates
        var updatedUser = await _context.Users.FindAsync(1);
        Assert.NotNull(updatedUser);
        Assert.Equal(95, updatedUser!.Energy); // 100 - 5
        Assert.Equal(15, updatedUser.XpBalance);

        var progress = await _context.UserProgresses.FirstOrDefaultAsync(p => p.UserId == 1 && p.MissionId == 1);
        Assert.NotNull(progress);
        Assert.Equal(15, progress.CurrentSuspicion);
        Assert.Equal(1, progress.TurnCount);
        Assert.Equal(15, progress.XpEarned);

        var dialogue = await _context.Dialogues.FirstOrDefaultAsync(d => d.UserId == 1 && d.MissionId == 1);
        Assert.NotNull(dialogue);
        Assert.Equal("Hello officer", dialogue.PlayerMessage);
        Assert.Equal("Identify yourself!", dialogue.NpcResponse);
    }

    [Fact]
    public async Task EvaluateMessageAsync_ShouldThrowException_WhenUserHasInsufficientEnergy()
    {
        // Arrange
        SeedDatabase();
        var user = await _context.Users.FindAsync(1);
        Assert.NotNull(user);
        user!.Energy = 4; // Insufficient (requires 5)
        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        var handlerMock = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(handlerMock.Object);
        var service = new AIEvaluationService(httpClient, _context, _configMock.Object, _badgeServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            service.EvaluateMessageAsync(1, 1, "Hello"));
        Assert.Contains("Not enough energy", exception.Message);
    }

    [Fact]
    public async Task EvaluateMessageAsync_ShouldUseFallbackResponse_WhenLLMApiFails()
    {
        // Arrange
        SeedDatabase();

        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var service = new AIEvaluationService(httpClient, _context, _configMock.Object, _badgeServiceMock.Object);

        // Act
        var result = await service.EvaluateMessageAsync(1, 1, "Hello officer");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("I didn't quite catch that. Can you repeat it?", result.NpcResponse);

        Assert.Contains("Không thể đánh giá do lỗi hệ thống.", result.WritingFeedback.Summary);

        Assert.Equal(10, result.NewSuspicionLevel); // 10 (start) + 0 (fallback suspicion change)
        Assert.Equal(0, result.XpEarned);

        // Verify energy was still deducted
        var updatedUser = await _context.Users.FindAsync(1);
        Assert.NotNull(updatedUser);
        Assert.Equal(95, updatedUser!.Energy);
    }

    [Theory]
    [InlineData("A1", "Use very simple English vocabulary (A1-A2 level). Write short, direct, simple sentences.")]
    [InlineData("C1", "Use advanced, nuanced, professional, and highly idiomatic English (C1-C2 level).")]
    public async Task EvaluateMessageAsync_ShouldIncludeCefrInstructions_BasedOnUserEnglishLevel(string level, string expectedInstructionSubstring)
    {
        // Arrange
        SeedDatabase();
        var user = await _context.Users.FindAsync(1);
        Assert.NotNull(user);
        user!.EnglishLevel = level;
        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        // Đã bọc lót tất cả các trường hợp Case Sensitivity
        var geminiResponseText = @"{
            ""npcResponse"": ""Roger that."",
            ""NpcResponse"": ""Roger that."",
            ""feedback"": ""Good response."",
            ""Feedback"": ""Good response."",
            ""suspicionChange"": -5,
            ""SuspicionChange"": -5,
            ""xpEarned"": 15,
            ""XpEarned"": 15,
            ""writingFeedback"": {
                ""scores"": {
                    ""grammar"": 85,
                    ""vocabulary"": 85,
                    ""tone"": 85,
                    ""naturalness"": 85,
                    ""clarity"": 85,
                    ""structure"": 85
                },
                ""corrections"": [],
                ""rewriteSuggestion"": null,
                ""summary"": ""Good response.""
            }
        }";

        string? capturedRequestContent = null;
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .Callback<HttpRequestMessage, CancellationToken>(async (req, token) =>
            {
                capturedRequestContent = await req.Content!.ReadAsStringAsync(token);
            })
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(
                    "event: message_start\n" +
                    "data: {\"type\":\"message_start\"}\n\n" +
                    "event: content_block_start\n" +
                    "data: {\"type\":\"content_block_start\",\"index\":0,\"content_block\":{\"type\":\"text\",\"text\":\"\"}}\n\n" +
                    "event: content_block_delta\n" +
                    "data: {\"type\":\"content_block_delta\",\"index\":0,\"delta\":{\"type\":\"text_delta\",\"text\":\"" + 
                    geminiResponseText.Replace("\"", "\\\"").Replace("\r", "").Replace("\n", " ") + 
                    "\"}}\n\n" +
                    "event: message_stop\n" +
                    "data: {\"type\":\"message_stop\"}\n\n"
                )
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var service = new AIEvaluationService(httpClient, _context, _configMock.Object, _badgeServiceMock.Object);

        // Act
        var result = await service.EvaluateMessageAsync(1, 1, "Testing levels");

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(capturedRequestContent);
        Assert.Contains(expectedInstructionSubstring, capturedRequestContent);
        Assert.Contains($"Trình độ tiếng Anh: {level}", capturedRequestContent);
    }

    [Fact]
    public async Task EvaluateMessageAsync_ShouldRecordLoseCondition_WhenSuspicionReachesMaxSuspicion()
    {
        // Arrange
        SeedDatabase();

        var geminiResponseText = @"{
            ""npcResponse"": ""You are caught!"",
            ""NpcResponse"": ""You are caught!"",
            ""feedback"": ""Bad reply."",
            ""Feedback"": ""Bad reply."",
            ""suspicionChange"": 100,
            ""SuspicionChange"": 100,
            ""xpEarned"": 0,
            ""XpEarned"": 0,
            ""writingFeedback"": {
                ""scores"": {
                    ""grammar"": 20,
                    ""vocabulary"": 20,
                    ""tone"": 20,
                    ""naturalness"": 20,
                    ""clarity"": 20,
                    ""structure"": 20
                },
                ""corrections"": [],
                ""rewriteSuggestion"": null,
                ""summary"": ""Failing performance.""
            }
        }";

        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(
                    "event: message_start\n" +
                    "data: {\"type\":\"message_start\"}\n\n" +
                    "event: content_block_start\n" +
                    "data: {\"type\":\"content_block_start\",\"index\":0,\"content_block\":{\"type\":\"text\",\"text\":\"\"}}\n\n" +
                    "event: content_block_delta\n" +
                    "data: {\"type\":\"content_block_delta\",\"index\":0,\"delta\":{\"type\":\"text_delta\",\"text\":\"" + 
                    geminiResponseText.Replace("\"", "\\\"").Replace("\r", "").Replace("\n", " ") + 
                    "\"}}\n\n" +
                    "event: message_stop\n" +
                    "data: {\"type\":\"message_stop\"}\n\n"
                )
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var service = new AIEvaluationService(httpClient, _context, _configMock.Object, _badgeServiceMock.Object);

        // Act
        var result = await service.EvaluateMessageAsync(1, 1, "suspicious message");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsLose);
        Assert.False(result.IsWin);
        Assert.Equal(100, result.NewSuspicionLevel);

        var progress = await _context.UserProgresses.FirstOrDefaultAsync(p => p.UserId == 1 && p.MissionId == 1);
        Assert.NotNull(progress);
        Assert.Equal(MissionStatus.Failed, progress!.Status);
        Assert.Equal(100, progress.CurrentSuspicion);
    }

    [Fact]
    public async Task EvaluateMessageAsync_ShouldRecordWinConditionAndCreateSnapshotAndAwardBadges_WhenCriteriaMet()
    {
        // Arrange
        SeedDatabase();

        // 1. Add 4 existing dialogue turns for this mission to meet the MinTurnsToComplete = 5 requirement
        var now = DateTime.UtcNow;
        for (int i = 1; i <= 4; i++)
        {
            var dialogue = new Dialogue
            {
                UserId = 1,
                MissionId = 1,
                NpcId = 1,
                PlayerMessage = $"Message {i}",
                NpcResponse = $"Response {i}",
                Feedback = "Good",
                Timestamp = now.AddMinutes(-10 + i),
                GrammarScore = 80,
                VocabularyScore = 80,
                ToneScore = 80,
                NaturalnessScore = 80,
                ClarityScore = 80,
                StructureScore = 80
            };
            await _context.Dialogues.AddAsync(dialogue);
        }

        // 2. Set user progress to TurnCount = 4 and Status = InProgress
        var progress = new UserProgress
        {
            UserId = 1,
            MissionId = 1,
            CurrentSuspicion = 20,
            Status = MissionStatus.InProgress,
            TurnCount = 4,
            XpEarned = 50
        };
        await _context.UserProgresses.AddAsync(progress);
        await _context.SaveChangesAsync();

        // 3. Mock the 5th message reply (overallAvg will be 80, turns will be 5, satisfying win)
        var geminiResponseText = @"{
            ""npcResponse"": ""Thank you very much."",
            ""NpcResponse"": ""Thank you very much."",
            ""feedback"": ""Excellent grammar!"",
            ""Feedback"": ""Excellent grammar!"",
            ""suspicionChange"": -5,
            ""SuspicionChange"": -5,
            ""xpEarned"": 15,
            ""XpEarned"": 15,
            ""writingFeedback"": {
                ""scores"": {
                    ""grammar"": 80,
                    ""vocabulary"": 80,
                    ""tone"": 80,
                    ""naturalness"": 80,
                    ""clarity"": 80,
                    ""structure"": 80
                },
                ""corrections"": [],
                ""rewriteSuggestion"": ""Perfect response."",
                ""summary"": ""Great job!""
            }
        }";

        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(
                    "event: message_start\n" +
                    "data: {\"type\":\"message_start\"}\n\n" +
                    "event: content_block_start\n" +
                    "data: {\"type\":\"content_block_start\",\"index\":0,\"content_block\":{\"type\":\"text\",\"text\":\"\"}}\n\n" +
                    "event: content_block_delta\n" +
                    "data: {\"type\":\"content_block_delta\",\"index\":0,\"delta\":{\"type\":\"text_delta\",\"text\":\"" + 
                    geminiResponseText.Replace("\"", "\\\"").Replace("\r", "").Replace("\n", " ") + 
                    "\"}}\n\n" +
                    "event: message_stop\n" +
                    "data: {\"type\":\"message_stop\"}\n\n"
                )
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var service = new AIEvaluationService(httpClient, _context, _configMock.Object, _badgeServiceMock.Object);

        // Act
        var result = await service.EvaluateMessageAsync(1, 1, "I would like a cup of coffee, please.");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsWin);
        Assert.False(result.IsLose);
        Assert.Equal(5, result.TurnCount);
        Assert.NotNull(result.CompletionToken);

        // Verify user progress is completed
        var updatedProgress = await _context.UserProgresses.FirstOrDefaultAsync(p => p.UserId == 1 && p.MissionId == 1);
        Assert.NotNull(updatedProgress);
        Assert.Equal(MissionStatus.Completed, updatedProgress!.Status);
        Assert.Equal(result.CompletionToken, updatedProgress.CompletionToken);

        // Verify WritingSkillSnapshot was created
        var snapshot = await _context.WritingSkillSnapshots.FirstOrDefaultAsync(s => s.UserId == 1 && s.MissionId == 1);
        Assert.NotNull(snapshot);
        Assert.Equal(80, snapshot!.GrammarScore);
        Assert.Equal(80, snapshot.VocabularyScore);
        Assert.Equal(80, snapshot.AverageScore);
        Assert.Equal("Perfect response.", snapshot.AiRewriteSuggestion);

        // Verify BadgeService was called on win
        _badgeServiceMock.Verify(s => s.AwardBadgesForMissionAsync(1, 1, It.Is<decimal>(v => v == 80m), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EvaluateMessageAsync_ShouldCreateCorrections_WhenResponseContainsCorrections()
    {
        // Arrange
        SeedDatabase();

        var geminiResponseText = @"{
            ""npcResponse"": ""Sorry?"",
            ""NpcResponse"": ""Sorry?"",
            ""feedback"": ""Correction needed."",
            ""Feedback"": ""Correction needed."",
            ""suspicionChange"": 10,
            ""SuspicionChange"": 10,
            ""xpEarned"": 5,
            ""XpEarned"": 5,
            ""writingFeedback"": {
                ""scores"": {
                    ""grammar"": 50,
                    ""vocabulary"": 60,
                    ""tone"": 70,
                    ""naturalness"": 60,
                    ""clarity"": 50,
                    ""structure"": 60
                },
                ""corrections"": [
                    {
                        ""axis"": 0,
                        ""original"": ""I goes to coffee shop"",
                        ""corrected"": ""I go to the coffee shop"",
                        ""explanation"": ""Subject-verb agreement and missing article.""
                    }
                ],
                ""rewriteSuggestion"": ""I would like to go to the coffee shop."",
                ""summary"": ""Fix grammar issues.""
            }
        }";

        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(
                    "event: message_start\n" +
                    "data: {\"type\":\"message_start\"}\n\n" +
                    "event: content_block_start\n" +
                    "data: {\"type\":\"content_block_start\",\"index\":0,\"content_block\":{\"type\":\"text\",\"text\":\"\"}}\n\n" +
                    "event: content_block_delta\n" +
                    "data: {\"type\":\"content_block_delta\",\"index\":0,\"delta\":{\"type\":\"text_delta\",\"text\":\"" + 
                    geminiResponseText.Replace("\"", "\\\"").Replace("\r", "").Replace("\n", " ") + 
                    "\"}}\n\n" +
                    "event: message_stop\n" +
                    "data: {\"type\":\"message_stop\"}\n\n"
                )
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var service = new AIEvaluationService(httpClient, _context, _configMock.Object, _badgeServiceMock.Object);

        // Act
        var result = await service.EvaluateMessageAsync(1, 1, "I goes to coffee shop");

        // Assert
        Assert.NotNull(result);

        // Verify correction was created in DB
        var correction = await _context.Corrections.Include(c => c.Dialogue).FirstOrDefaultAsync(c => c.Dialogue.UserId == 1);
        Assert.NotNull(correction);
        Assert.Equal("I goes to coffee shop", correction!.OriginalText);
        Assert.Equal("I go to the coffee shop", correction.CorrectedText);
        Assert.Equal("Subject-verb agreement and missing article.", correction.Explanation);
        Assert.Equal(TheUnraveller.Core.Entities.SkillAxis.Grammar, correction.Axis);
    }

    [Fact]
    public async Task EvaluateMessageAsync_ShouldReturnFallback_WhenResponseIsNotValidJson()
    {
        // Arrange
        SeedDatabase();

        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(
                    "event: message_start\n" +
                    "data: {\"type\":\"message_start\"}\n\n" +
                    "event: content_block_delta\n" +
                    "data: {\"type\":\"content_block_delta\",\"index\":0,\"delta\":{\"type\":\"text_delta\",\"text\":\"This is not valid JSON [[[\"}}\n\n" +
                    "event: message_stop\n" +
                    "data: {\"type\":\"message_stop\"}\n\n"
                )
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var service = new AIEvaluationService(httpClient, _context, _configMock.Object, _badgeServiceMock.Object);

        // Act
        var result = await service.EvaluateMessageAsync(1, 1, "Hello");

        // Assert - should return fallback with valid structure
        Assert.NotNull(result);
        Assert.NotNull(result.WritingFeedback);
        Assert.NotNull(result.WritingFeedback.Scores);
        // Fallback NPC response
        Assert.Contains("think", result.NpcResponse, StringComparison.OrdinalIgnoreCase);
        // Energy still deducted
        var updatedUser = await _context.Users.FindAsync(1);
        Assert.NotNull(updatedUser);
        Assert.Equal(95, updatedUser!.Energy);
    }

    [Fact]
    public async Task EvaluateMessageAsync_ShouldReturnFallback_WhenMissingRequiredFields()
    {
        // Arrange
        SeedDatabase();

        // JSON with missing fields
        var incompleteJson = @"{
            ""npcResponse"": """",
            ""suspicionChange"": 10,
            ""xpEarned"": 5
        }";

        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(
                    "event: message_start\n" +
                    "data: {\"type\":\"message_start\"}\n\n" +
                    "event: content_block_delta\n" +
                    "data: {\"type\":\"content_block_delta\",\"index\":0,\"delta\":{\"type\":\"text_delta\",\"text\":\"" +
                    incompleteJson.Replace("\"", "\\\"").Replace("\r", "").Replace("\n", " ") +
                    "\"}}\n\n" +
                    "event: message_stop\n" +
                    "data: {\"type\":\"message_stop\"}\n\n"
                )
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var service = new AIEvaluationService(httpClient, _context, _configMock.Object, _badgeServiceMock.Object);

        // Act
        var result = await service.EvaluateMessageAsync(1, 1, "Hello");

        // Assert - validation should fix missing fields
        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.NpcResponse)); // Should have a fallback NPC response
        Assert.NotNull(result.WritingFeedback);
        Assert.NotNull(result.WritingFeedback.Scores);
        // Summary should be Vietnamese
        Assert.False(string.IsNullOrWhiteSpace(result.WritingFeedback.Summary));
    }

    [Fact]
    public async Task EvaluateMessageAsync_ShouldAlwaysReturnVietnameseSummary()
    {
        // Arrange
        SeedDatabase();

        var geminiResponseText = @"{
            ""npcResponse"": ""Hello!"",
            ""NpcResponse"": ""Hello!"",
            ""feedback"": ""Good."",
            ""Feedback"": ""Good."",
            ""suspicionChange"": -5,
            ""SuspicionChange"": -5,
            ""xpEarned"": 10,
            ""XpEarned"": 10,
            ""writingFeedback"": {
                ""scores"": {
                    ""grammar"": 80,
                    ""vocabulary"": 80,
                    ""tone"": 80,
                    ""naturalness"": 80,
                    ""clarity"": 80,
                    ""structure"": 80
                },
                ""corrections"": [],
                ""rewriteSuggestion"": null,
                ""summary"": ""Great job! No errors.""
            }
        }";

        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(
                    "event: message_start\n" +
                    "data: {\"type\":\"message_start\"}\n\n" +
                    "event: content_block_start\n" +
                    "data: {\"type\":\"content_block_start\",\"index\":0,\"content_block\":{\"type\":\"text\",\"text\":\"\"}}\n\n" +
                    "event: content_block_delta\n" +
                    "data: {\"type\":\"content_block_delta\",\"index\":0,\"delta\":{\"type\":\"text_delta\",\"text\":\"" +
                    geminiResponseText.Replace("\"", "\\\"").Replace("\r", "").Replace("\n", " ") +
                    "\"}}\n\n" +
                    "event: message_stop\n" +
                    "data: {\"type\":\"message_stop\"}\n\n"
                )
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var service = new AIEvaluationService(httpClient, _context, _configMock.Object, _badgeServiceMock.Object);

        // Act
        var result = await service.EvaluateMessageAsync(1, 1, "Hello");

        // Assert - summary should be Vietnamese (contains Vietnamese characters or starts with bullet)
        Assert.NotNull(result.WritingFeedback.Summary);
        // Check that summary starts with bullet or contains Vietnamese diacritics
        bool hasVietnameseOrBullet = result.WritingFeedback.Summary.StartsWith("*") ||
                                     result.WritingFeedback.Summary.StartsWith("•") ||
                                     result.WritingFeedback.Summary.Any(c => "àáảãạâấầẩẫậăắằẳẵặèéẻẽẹêếềểễệìíỉĩịòóỏõọôốồổỗộơớờởỡợùúủũụưứừửữựỳýỷỹỵđĐÀÁẢÃẠÂẤẦẨẪẬĂẮẰẲẴẶÈÉẺẼẸÊẾỀỂỄỆÌÍỈĨỊÒÓỎÕỌÔỐỒỔỖỘƠỚỜỞỠỢÙÚỦŨỤƯỨỪỬỮỰỲÝỶỸỴ".Contains(c));
        Assert.True(hasVietnameseOrBullet, $"Summary should be in Vietnamese or have bullet points: {result.WritingFeedback.Summary}");
    }

    [Fact]
    public async Task GetWritingSkillMapAsync_ShouldReturnEmptyMap_WhenNoSnapshots()
    {
        // Arrange
        SeedDatabase();
        // Ensure no snapshots exist
        var existingSnapshots = await _context.WritingSkillSnapshots.ToListAsync();
        _context.WritingSkillSnapshots.RemoveRange(existingSnapshots);
        await _context.SaveChangesAsync();

        var service = new AIEvaluationService(
            new HttpClient(), _context, _configMock.Object, _badgeServiceMock.Object);

        // Act
        var result = await service.GetWritingSkillMapAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.CurrentAverage.Grammar);
        Assert.Equal(0, result.CurrentAverage.Vocabulary);
        Assert.Empty(result.HistoricalTrend);
    }

    [Fact]
    public async Task GetWritingSkillMapAsync_ShouldCalculateAverages_WhenSnapshotsExist()
    {
        // Arrange
        SeedDatabase();

        // Add snapshots with different scores
        var snapshots = new List<WritingSkillSnapshot>
        {
            new WritingSkillSnapshot
            {
                UserId = 1,
                MissionId = 1,
                CompletedAt = new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc),
                GrammarScore = 80,
                VocabularyScore = 75,
                ToneScore = 70,
                NaturalnessScore = 85,
                ClarityScore = 90,
                StructureScore = 95,
                AverageScore = 82,
                TurnsCount = 5
            },
            new WritingSkillSnapshot
            {
                UserId = 1,
                MissionId = 2,
                CompletedAt = new DateTime(2026, 2, 10, 14, 0, 0, DateTimeKind.Utc),
                GrammarScore = 85,
                VocabularyScore = 80,
                ToneScore = 75,
                NaturalnessScore = 90,
                ClarityScore = 95,
                StructureScore = 100,
                AverageScore = 88,
                TurnsCount = 6
            },
            new WritingSkillSnapshot
            {
                UserId = 1,
                MissionId = 3,
                CompletedAt = new DateTime(2026, 2, 20, 16, 0, 0, DateTimeKind.Utc),
                GrammarScore = 90,
                VocabularyScore = 85,
                ToneScore = 80,
                NaturalnessScore = 95,
                ClarityScore = 100,
                StructureScore = 100,
                AverageScore = 92,
                TurnsCount = 7
            }
        };

        await _context.WritingSkillSnapshots.AddRangeAsync(snapshots);
        await _context.SaveChangesAsync();

        var service = new AIEvaluationService(
            new HttpClient(), _context, _configMock.Object, _badgeServiceMock.Object);

        // Act
        var result = await service.GetWritingSkillMapAsync(1);

        // Assert
        Assert.NotNull(result);
        // Verify averages: (80+85+90)/3 = 85, (75+80+85)/3 = 80, etc.
        Assert.Equal(85, result.CurrentAverage.Grammar);
        Assert.Equal(80, result.CurrentAverage.Vocabulary);
        Assert.Equal(75, result.CurrentAverage.Tone);
        Assert.Equal(90, result.CurrentAverage.Naturalness);
        Assert.Equal(95, result.CurrentAverage.Clarity);
        Assert.Equal(98, result.CurrentAverage.Structure); // (95+100+100)/3 = 98.33 -> 98

        // Verify historical trend has 2 months (Jan and Feb)
        Assert.Equal(2, result.HistoricalTrend.Count);
        Assert.True(result.HistoricalTrend.ContainsKey("2026-01"));
        Assert.Equal(82m, result.HistoricalTrend["2026-01"]);
        Assert.True(result.HistoricalTrend.ContainsKey("2026-02"));
        // Feb average = (88+92)/2 = 90
        Assert.Equal(90m, result.HistoricalTrend["2026-02"]);
    }

    [Fact]
    public async Task GetWritingSkillMapAsync_ShouldThrowException_WhenUserNotFound()
    {
        // Arrange - use non-existent user ID
        SeedDatabase();
        // Delete user 1 to ensure not found
        var user = await _context.Users.FindAsync(1);
        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        var service = new AIEvaluationService(
            new HttpClient(), _context, _configMock.Object, _badgeServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() => service.GetWritingSkillMapAsync(1));
        Assert.Contains("User not found", exception.Message);
    }
}