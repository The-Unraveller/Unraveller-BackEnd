using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using TheUnraveller.API.Controllers;
using TheUnraveller.Service.DTOs;
using TheUnraveller.Service.Interfaces;
using TheUnraveller.Core.Interfaces;
using Xunit;

namespace TheUnraveller.Tests;

public class GameControllerTests
{
	private readonly Mock<IAIEvaluationService> _aiServiceMock;
	private readonly Mock<IShopRepository> _shopRepoMock;
	private readonly Mock<IUserProgressRepository> _progressRepoMock;
	private readonly GameController _controller;

	public GameControllerTests()
	{
		_aiServiceMock = new Mock<IAIEvaluationService>();
		_shopRepoMock = new Mock<IShopRepository>();
		_progressRepoMock = new Mock<IUserProgressRepository>();

		// Set up Mock User Claims for Authorized Endpoint
		var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
		{
			new Claim(ClaimTypes.NameIdentifier, "1")
		}, "mock"));

		var myConfiguration = new System.Collections.Generic.Dictionary<string, string>
		{
			{ "GameRules:BribeNpcSuspicionReduction", "20" }
		};
		var configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(myConfiguration)
			.Build();

		_controller = new GameController(_aiServiceMock.Object, _shopRepoMock.Object, _progressRepoMock.Object, configuration)
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
		var mockResponse = new DialogueResponseWithScoresDto(
			"Who goes there?",
			new WritingFeedbackDto(
				new WritingScoreDto(80, 85, 90, 75, 88, 82),
				new List<CorrectionDto>(),
				null,
				"Good spelling."
			),
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
		var actionResult = Assert.IsType<ActionResult<DialogueResponseWithScoresDto>>(result);
		var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
		var returnedDto = Assert.IsType<DialogueResponseWithScoresDto>(okResult.Value);

		Assert.Equal("Who goes there?", returnedDto.NpcResponse);
		Assert.Equal("Good spelling.", returnedDto.WritingFeedback.Summary);
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
		var actionResult = Assert.IsType<ActionResult<DialogueResponseWithScoresDto>>(result);
		var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);

		// Assert error message exists in returned object
		var errorObj = badRequestResult.Value;
		Assert.NotNull(errorObj);
	}
}
