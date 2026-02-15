using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using RaindropServer.Common;
using RaindropServer.Tests.Common;
using Xunit;
using NSubstitute;

namespace RaindropServer.Tests;

public class AuthTests
{
    [Fact]
    public void HttpContextTokenProvider_ReturnsToken_ForBearerScheme()
    {
        // Arrange
        var contextAccessor = Substitute.For<IHttpContextAccessor>();
        var context = new DefaultHttpContext();
        context.Request.Headers["Authorization"] = "Bearer test-token";
        contextAccessor.HttpContext.Returns(context);

        var provider = new HttpContextTokenProvider(contextAccessor);

        // Act
        var token = provider.GetToken();

        // Assert
        Assert.Equal("test-token", token);
    }

    [Fact]
    public void HttpContextTokenProvider_ReturnsNull_ForNonBearerScheme()
    {
        // Arrange
        var contextAccessor = Substitute.For<IHttpContextAccessor>();
        var context = new DefaultHttpContext();
        context.Request.Headers["Authorization"] = "Basic some-token";
        contextAccessor.HttpContext.Returns(context);

        var provider = new HttpContextTokenProvider(contextAccessor);

        // Act
        var token = provider.GetToken();

        // Assert
        Assert.Null(token);
    }

    [Fact]
    public async Task PassThroughAuthenticationHandler_ReturnsSuccess_WhenHeaderPresentAndValidGuid()
    {
        // Arrange
        var optionsMonitor = Substitute.For<IOptionsMonitor<AuthenticationSchemeOptions>>();
        optionsMonitor.Get(Arg.Any<string>()).Returns(new AuthenticationSchemeOptions());
        var logger = NullLoggerFactory.Instance;
        var encoder = UrlEncoder.Default;

        var handler = new PassThroughAuthenticationHandler(optionsMonitor, logger, encoder);

        var context = new DefaultHttpContext();
        var validGuid = Guid.NewGuid().ToString();
        context.Request.Headers["Authorization"] = $"Bearer {validGuid}";

        await handler.InitializeAsync(new AuthenticationScheme("PassThrough", "PassThrough", typeof(PassThroughAuthenticationHandler)), context);

        // Act
        var result = await handler.AuthenticateAsync();

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Principal);
        Assert.Equal("RaindropUser", result.Principal?.Identity?.Name);
    }

    [Fact]
    public async Task PassThroughAuthenticationHandler_ReturnsFail_WhenHeaderMissing()
    {
        // Arrange
        var optionsMonitor = Substitute.For<IOptionsMonitor<AuthenticationSchemeOptions>>();
        optionsMonitor.Get(Arg.Any<string>()).Returns(new AuthenticationSchemeOptions());
        var logger = NullLoggerFactory.Instance;
        var encoder = UrlEncoder.Default;

        var handler = new PassThroughAuthenticationHandler(optionsMonitor, logger, encoder);

        var context = new DefaultHttpContext();
        // No header

        await handler.InitializeAsync(new AuthenticationScheme("PassThrough", "PassThrough", typeof(PassThroughAuthenticationHandler)), context);

        // Act
        var result = await handler.AuthenticateAsync();

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("Missing Authorization Header", result.Failure?.Message);
    }

    [Fact]
    public async Task PassThroughAuthenticationHandler_ReturnsFail_WhenTokenIsNotGuid()
    {
        // Arrange
        var optionsMonitor = Substitute.For<IOptionsMonitor<AuthenticationSchemeOptions>>();
        optionsMonitor.Get(Arg.Any<string>()).Returns(new AuthenticationSchemeOptions());
        var logger = NullLoggerFactory.Instance;
        var encoder = UrlEncoder.Default;

        var handler = new PassThroughAuthenticationHandler(optionsMonitor, logger, encoder);

        var context = new DefaultHttpContext();
        context.Request.Headers["Authorization"] = "Bearer not-a-guid";

        await handler.InitializeAsync(new AuthenticationScheme("PassThrough", "PassThrough", typeof(PassThroughAuthenticationHandler)), context);

        // Act
        var result = await handler.AuthenticateAsync();

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("Invalid Token Format", result.Failure?.Message);
    }

    [Fact]
    public async Task AuthHeaderHandler_AddsToken_WhenProviderHasToken()
    {
        // Arrange
        var tokenProvider = new StaticTokenProvider("test-token");
        var innerHandler = new TestHandler();
        var handler = new AuthHeaderHandler(tokenProvider)
        {
            InnerHandler = innerHandler
        };
        var invoker = new HttpMessageInvoker(handler);

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost");
        await invoker.SendAsync(request, CancellationToken.None);

        // Assert
        var sentRequest = innerHandler.LastRequest;
        Assert.NotNull(sentRequest);
        Assert.NotNull(sentRequest.Headers.Authorization);
        Assert.Equal("Bearer", sentRequest.Headers.Authorization.Scheme);
        Assert.Equal("test-token", sentRequest.Headers.Authorization.Parameter);
    }

    [Fact]
    public async Task AuthHeaderHandler_DoesNotAddToken_WhenProviderHasNoToken()
    {
        // Arrange
        var tokenProvider = new StaticTokenProvider(null);
        var innerHandler = new TestHandler();
        var handler = new AuthHeaderHandler(tokenProvider)
        {
            InnerHandler = innerHandler
        };
        var invoker = new HttpMessageInvoker(handler);

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost");
        await invoker.SendAsync(request, CancellationToken.None);

        // Assert
        var sentRequest = innerHandler.LastRequest;
        Assert.NotNull(sentRequest);
        Assert.Null(sentRequest.Headers.Authorization);
    }

    private class TestHandler : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
        }
    }
}
