using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Moq;
using TheUnraveller.API.Middleware;
using Xunit;

namespace TheUnraveller.Tests;

public class ModeratorMiddlewareTests
{
    private readonly Mock<RequestDelegate> _nextMock;

    public ModeratorMiddlewareTests()
    {
        _nextMock = new Mock<RequestDelegate>();
    }

    [Fact]
    public async Task InvokeAsync_StandardUserAccessingModeratorRoute_ShouldReturn403Forbidden()
    {
        // Arrange
        var middleware = new ModeratorMiddleware(_nextMock.Object);
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/Moderator/missions";

        // Standard user (unauthenticated or lacking Moderator role claim)
        context.User = new ClaimsPrincipal(new ClaimsIdentity());

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
        _nextMock.Verify(n => n(It.IsAny<HttpContext>()), Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_ModeratorAccessingModeratorRoute_ShouldCallNextDelegate()
    {
        // Arrange
        var middleware = new ModeratorMiddleware(_nextMock.Object);
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/Moderator/missions";

        // Moderator user
        var claims = new[] { new Claim(ClaimTypes.Role, "Moderator") };
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "mock_auth"));

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(n => n(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_AdminAccessingModeratorRoute_ShouldCallNextDelegate()
    {
        // Arrange
        var middleware = new ModeratorMiddleware(_nextMock.Object);
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/Moderator/missions";

        // Admin user
        var claims = new[] { new Claim(ClaimTypes.Role, "Admin") };
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "mock_auth"));

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(n => n(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_StandardUserAccessingOtherRoute_ShouldCallNextDelegate()
    {
        // Arrange
        var middleware = new ModeratorMiddleware(_nextMock.Object);
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/Mission";

        // Standard user
        context.User = new ClaimsPrincipal(new ClaimsIdentity());

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(n => n(context), Times.Once);
    }
}
