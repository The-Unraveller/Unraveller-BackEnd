using System.Net;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using TheUnraveller.Core.Entities;
using TheUnraveller.Infrastructure.Data;
using TheUnraveller.Service.Implementations;
using Xunit;

namespace TheUnraveller.Tests;

public class AIEvaluationServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _context;
    private readonly Mock<IConfiguration> _configMock;

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
        if (user != null)
        {
            user.Energy = 100;
            user.MaxEnergy = 100;
            user.LastEnergyRechargedAt = DateTime.UtcNow;
            user.XpBalance = 0;
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
            ""XpEarned"": 15
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
        var service = new AIEvaluationService(httpClient, _context, _configMock.Object);

        // Act
        var result = await service.EvaluateMessageAsync(1, 1, "Hello officer");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Identify yourself!", result.NpcResponse);
        Assert.Equal("Good grammar, standard greeting.", result.Feedback);
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
        var service = new AIEvaluationService(httpClient, _context, _configMock.Object);

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
        var service = new AIEvaluationService(httpClient, _context, _configMock.Object);

        // Act
        var result = await service.EvaluateMessageAsync(1, 1, "Hello officer");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("I didn't quite catch that. Can you repeat it?", result.NpcResponse);
        
        Assert.Contains("Không phát hiện lỗi", result.Feedback); 
        
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
            ""XpEarned"": 15
        }";

        string capturedRequestContent = null;
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
        var service = new AIEvaluationService(httpClient, _context, _configMock.Object);

        // Act
        var result = await service.EvaluateMessageAsync(1, 1, "Testing levels");

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(capturedRequestContent);
        Assert.Contains(expectedInstructionSubstring, capturedRequestContent);
        Assert.Contains($"PLAYER ENGLISH LEVEL: {level}", capturedRequestContent);
    }
}