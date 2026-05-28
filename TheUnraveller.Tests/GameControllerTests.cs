using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TheUnraveller.API.Controllers;
using TheUnraveller.Service.DTOs;
using TheUnraveller.Service.Interfaces;
using Xunit;

namespace TheUnraveller.Tests;

public class GameControllerTests
{
    private readonly Mock<IAIEvaluationService> _aiServiceMock;
    private readonly GameController _controller;

    public GameControllerTests()
    {
        _aiServiceMock = new Mock<IAIEvaluationService>();

        // Set up Mock User Claims for Authorized Endpoint
        var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1")
        }, "mock"));

        _controller = new GameController(_aiServiceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            }
        };
    }

    [Fact]
    public async Task SendMessage_ShouldReturnOk_OnSuccess()
    {
        // Arrange
        var request = new DialogueRequestDto(0, 1, "Hello officer");
        var mockResponse = new DialogueResponseDto(
            "Who goes there?",
            "Good spelling.",
            15,
            false,
            false,
            1,
            5
        );

        _aiServiceMock
            .Setup(s => s.EvaluateMessageAsync(1, 1, "Hello officer"))
            .ReturnsAsync(mockResponse);

        // Act
        var result = await _controller.SendMessage(request);

        // Assert
        var actionResult = Assert.IsType<ActionResult<DialogueResponseDto>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnedDto = Assert.IsType<DialogueResponseDto>(okResult.Value);

        Assert.Equal("Who goes there?", returnedDto.NpcResponse);
        Assert.Equal("Good spelling.", returnedDto.Feedback);
        Assert.Equal(15, returnedDto.NewSuspicionLevel);
        Assert.Equal(5, returnedDto.XpEarned);
    }

    [Fact]
    public async Task SendMessage_ShouldReturnBadRequest_WhenServiceThrowsException()
    {
        // Arrange
        var request = new DialogueRequestDto(0, 1, "Hello officer");

        _aiServiceMock
            .Setup(s => s.EvaluateMessageAsync(1, 1, "Hello officer"))
            .ThrowsAsync(new System.Exception("Not enough energy"));

        // Act
        var result = await _controller.SendMessage(request);

        // Assert
        var actionResult = Assert.IsType<ActionResult<DialogueResponseDto>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        
        // Assert error message exists in returned object
        var errorObj = badRequestResult.Value;
        Assert.NotNull(errorObj);
    }
}
