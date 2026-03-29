using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using RaindropServer.Common;
using RaindropServer.Tests.Common;
using RaindropServer.User;
using Xunit;
using NSubstitute;

namespace RaindropServer.Tests;

public class AuthTests
{
    private readonly IMemoryCache _cache;
    private readonly IOptionsMonitor<AuthenticationSchemeOptions> _optionsMonitor;
    private readonly UrlEncoder _encoder;

    public AuthTests()
    {
        _cache = new MemoryCache(new MemoryCacheOptions());
        _optionsMonitor = Substitute.For<IOptionsMonitor<AuthenticationSchemeOptions>>();
        _optionsMonitor.Get(Arg.Any<string>()).Returns(new AuthenticationSchemeOptions());
        _encoder = UrlEncoder.Default;
    }

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
    public async Task PassThroughAuthenticationHandler_ReturnsSuccess_WhenTokenIsValidatedByApi()
    {
        // Arrange
        var userApi = Substitute.For<IUserApi>();
        userApi.GetAsync().Returns(new ItemResponse<UserInfo>(true, new UserInfo()));

        var services = new ServiceCollection();
        services.AddSingleton(userApi);
        var serviceProvider = services.BuildServiceProvider();

        var handler = new PassThroughAuthenticationHandler(_optionsMonitor, NullLoggerFactory.Instance, _encoder, _cache);

        var context = new DefaultHttpContext { RequestServices = serviceProvider };
        var validGuid = Guid.NewGuid().ToString();
        context.Request.Headers["Authorization"] = $"Bearer {validGuid}";

        await handler.InitializeAsync(new AuthenticationScheme("PassThrough", "PassThrough", typeof(PassThroughAuthenticationHandler)), context);

        // Act
        var result = await handler.AuthenticateAsync();

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("RaindropUser", result.Principal?.Identity?.Name);
        await userApi.Received(1).GetAsync();
    }

    [Fact]
    public async Task PassThroughAuthenticationHandler_ReturnsSuccess_WhenTokenIsInCache()
    {
        // Arrange
        var userApi = Substitute.For<IUserApi>();
        var validGuid = Guid.NewGuid().ToString();
        _cache.Set($"TokenValidation_{validGuid}", true);

        var services = new ServiceCollection();
        services.AddSingleton(userApi);
        var serviceProvider = services.BuildServiceProvider();

        var handler = new PassThroughAuthenticationHandler(_optionsMonitor, NullLoggerFactory.Instance, _encoder, _cache);

        var context = new DefaultHttpContext { RequestServices = serviceProvider };
        context.Request.Headers["Authorization"] = $"Bearer {validGuid}";

        await handler.InitializeAsync(new AuthenticationScheme("PassThrough", "PassThrough", typeof(PassThroughAuthenticationHandler)), context);

        // Act
        var result = await handler.AuthenticateAsync();

        // Assert
        Assert.True(result.Succeeded);
        await userApi.DidNotReceive().GetAsync();
    }

    [Fact]
    public async Task PassThroughAuthenticationHandler_ReturnsFail_WhenApiReturnsFailure()
    {
        // Arrange
        var userApi = Substitute.For<IUserApi>();
        userApi.GetAsync().Returns(new ItemResponse<UserInfo>(false, null!));

        var services = new ServiceCollection();
        services.AddSingleton(userApi);
        var serviceProvider = services.BuildServiceProvider();

        var handler = new PassThroughAuthenticationHandler(_optionsMonitor, NullLoggerFactory.Instance, _encoder, _cache);

        var context = new DefaultHttpContext { RequestServices = serviceProvider };
        var validGuid = Guid.NewGuid().ToString();
        context.Request.Headers["Authorization"] = $"Bearer {validGuid}";

        await handler.InitializeAsync(new AuthenticationScheme("PassThrough", "PassThrough", typeof(PassThroughAuthenticationHandler)), context);

        // Act
        var result = await handler.AuthenticateAsync();

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("Invalid Token", result.Failure?.Message);
        await userApi.Received(1).GetAsync();

        // Verify it was cached as failure
        var secondResult = await handler.AuthenticateAsync();
        Assert.False(secondResult.Succeeded);
        await userApi.Received(1).GetAsync(); // Still only 1 call
    }

    [Fact]
    public async Task PassThroughAuthenticationHandler_ReturnsFail_WhenHeaderMissing()
    {
        // Arrange
        var handler = new PassThroughAuthenticationHandler(_optionsMonitor, NullLoggerFactory.Instance, _encoder, _cache);

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
        var handler = new PassThroughAuthenticationHandler(_optionsMonitor, NullLoggerFactory.Instance, _encoder, _cache);

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
