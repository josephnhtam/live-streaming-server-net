using AutoFixture;
using FluentAssertions;
using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Auth;
using LiveStreamingServerNet.Rtmp.Auth.Contracts;
using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Authorization;
using LiveStreamingServerNet.Rtmp.Internal.Authorization.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace LiveStreamingServerNet.Rtmp.Test.Authorization
{
    public class StreamAuthorizationTest
    {
        private readonly IFixture _fixture;
        private readonly string _authCode;
        private readonly IRtmpClientContext _clientContext;
        private readonly IRtmpServerContext _serverContext;
        private readonly ServiceCollection _services;

        public StreamAuthorizationTest()
        {
            _fixture = new Fixture();
            _clientContext = Substitute.For<IRtmpClientContext>();

            _authCode = _fixture.Create<string>();
            _serverContext = Substitute.For<IRtmpServerContext>();
            _serverContext.AuthCode.Returns(_authCode);

            _services = new ServiceCollection();
            _services.AddSingleton<IStreamAuthorization, StreamAuthorization>()
                     .AddSingleton(_serverContext);
        }

        [Fact]
        public async Task AuthorizePublishingAsync_Should_CheckAuthorizationHandlersInOrder()
        {
            // Arrange
            var streamPath = _fixture.Create<string>();
            var publishingType = _fixture.Create<string>();
            var streamArguments = new Dictionary<string, string>();

            var authorizationHandler1 = Substitute.For<IAuthorizationHandler>();
            authorizationHandler1.GetOrder().Returns(0);
            authorizationHandler1.AuthorizePublishingAsync(
                _clientContext.Client, streamPath, streamArguments, publishingType)
                .Returns(AuthorizationResult.Authorized());

            var authorizationHandler2 = Substitute.For<IAuthorizationHandler>();
            authorizationHandler2.GetOrder().Returns(1);
            authorizationHandler2.AuthorizePublishingAsync(
                _clientContext.Client, streamPath, streamArguments, publishingType)
                .Returns(AuthorizationResult.Authorized());

            _services.AddSingleton(authorizationHandler2);
            _services.AddSingleton(authorizationHandler1);

            var sut = _services.BuildServiceProvider().GetRequiredService<IStreamAuthorization>();

            // Act
            var result = await sut.AuthorizePublishingAsync(_clientContext, streamPath, publishingType, streamArguments);

            // Assert
            result.IsAuthorized.Should().BeTrue();

            Received.InOrder(() =>
            {
                authorizationHandler1.AuthorizePublishingAsync(
                    _clientContext.Client, streamPath, streamArguments, publishingType);

                authorizationHandler2.AuthorizePublishingAsync(
                    _clientContext.Client, streamPath, streamArguments, publishingType);
            });
        }

        [Fact]
        public async Task AuthorizePublishingAsync_Should_ReturnAuthorizedResult_WhenNoAuthorizationHandler()
        {
            // Arrange
            var streamPath = _fixture.Create<string>();
            var publishingType = _fixture.Create<string>();
            var streamArguments = new Dictionary<string, string>();

            var sut = _services.BuildServiceProvider().GetRequiredService<IStreamAuthorization>();

            // Act
            var result = await sut.AuthorizePublishingAsync(_clientContext, streamPath, publishingType, streamArguments);

            // Assert
            result.IsAuthorized.Should().BeTrue();
        }

        [Fact]
        public async Task AuthorizePublishingAsync_Should_ReturnUnauthorizedResult_WhenNotAuthorizedByAnyAuthorizationHandler()
        {
            // Arrange
            var streamPath = _fixture.Create<string>();
            var publishingType = _fixture.Create<string>();
            var streamArguments = new Dictionary<string, string>();

            _services.AddSingleton<IAuthorizationHandler, AlwaysAuthorizedHandler>();
            _services.AddSingleton<IAuthorizationHandler, AlwaysUnauthorizedHandler>();
            var sut = _services.BuildServiceProvider().GetRequiredService<IStreamAuthorization>();

            // Act
            var result = await sut.AuthorizePublishingAsync(_clientContext, streamPath, publishingType, streamArguments);

            // Assert
            result.IsAuthorized.Should().BeFalse();
        }

        [Fact]
        public async Task AuthorizePublishingAsync_Should_ReturnAuthorizedResult_WhenAuthorized()
        {
            // Arrange
            var streamPath = _fixture.Create<string>();
            var publishingType = _fixture.Create<string>();
            var streamArguments = new Dictionary<string, string>();

            _services.AddSingleton<IAuthorizationHandler, AlwaysAuthorizedHandler>();
            var sut = _services.BuildServiceProvider().GetRequiredService<IStreamAuthorization>();

            // Act
            var result = await sut.AuthorizePublishingAsync(_clientContext, streamPath, publishingType, streamArguments);

            // Assert
            result.IsAuthorized.Should().BeTrue();
        }

        [Fact]
        public async Task AuthorizePublishingAsync_Should_ReturnAuthorizedResult_WhenAuthCodeIsProvided()
        {
            // Arrange
            var streamPath = _fixture.Create<string>();
            var publishingType = _fixture.Create<string>();

            var streamArguments = new Dictionary<string, string>() { { "code", _authCode } };

            _services.AddSingleton<IAuthorizationHandler, AlwaysAuthorizedHandler>();
            _services.AddSingleton<IAuthorizationHandler, AlwaysUnauthorizedHandler>();
            var sut = _services.BuildServiceProvider().GetRequiredService<IStreamAuthorization>();

            // Act
            var result = await sut.AuthorizePublishingAsync(_clientContext, streamPath, publishingType, streamArguments);

            // Assert
            result.IsAuthorized.Should().BeTrue();
        }

        [Fact]
        public async Task AuthorizeSubscribingAsync_Should_CheckAuthorizationHandlersInOrder()
        {
            // Arrange
            var streamPath = _fixture.Create<string>();
            var streamArguments = new Dictionary<string, string>();

            var authorizationHandler1 = Substitute.For<IAuthorizationHandler>();
            authorizationHandler1.GetOrder().Returns(0);
            authorizationHandler1.AuthorizeSubscribingAsync(
                _clientContext.Client, streamPath, streamArguments)
                .Returns(AuthorizationResult.Authorized());

            var authorizationHandler2 = Substitute.For<IAuthorizationHandler>();
            authorizationHandler2.GetOrder().Returns(1);
            authorizationHandler2.AuthorizeSubscribingAsync(
                _clientContext.Client, streamPath, streamArguments)
                .Returns(AuthorizationResult.Authorized());

            _services.AddSingleton(authorizationHandler2);
            _services.AddSingleton(authorizationHandler1);

            var sut = _services.BuildServiceProvider().GetRequiredService<IStreamAuthorization>();

            // Act
            var result = await sut.AuthorizeSubscribingAsync(_clientContext, streamPath, streamArguments);

            // Assert
            result.IsAuthorized.Should().BeTrue();

            Received.InOrder(() =>
            {
                authorizationHandler1.AuthorizeSubscribingAsync(
                    _clientContext.Client, streamPath, streamArguments);

                authorizationHandler2.AuthorizeSubscribingAsync(
                    _clientContext.Client, streamPath, streamArguments);
            });
        }

        [Fact]
        public async Task AuthorizeSubscribingAsync_Should_ReturnAuthorizedResult_WhenNoAuthorizationHandler()
        {
            // Arrange
            var streamPath = _fixture.Create<string>();
            var streamArguments = new Dictionary<string, string>();

            var sut = _services.BuildServiceProvider().GetRequiredService<IStreamAuthorization>();

            // Act
            var result = await sut.AuthorizeSubscribingAsync(_clientContext, streamPath, streamArguments);

            // Assert
            result.IsAuthorized.Should().BeTrue();
        }

        [Fact]
        public async Task AuthorizeSubscribingAsync_Should_ReturnUnauthorizedResult_WhenNotAuthorizedByAnyAuthorizationHandler()
        {
            // Arrange
            var streamPath = _fixture.Create<string>();
            var streamArguments = new Dictionary<string, string>();

            _services.AddSingleton<IAuthorizationHandler, AlwaysAuthorizedHandler>();
            _services.AddSingleton<IAuthorizationHandler, AlwaysUnauthorizedHandler>();
            var sut = _services.BuildServiceProvider().GetRequiredService<IStreamAuthorization>();

            // Act
            var result = await sut.AuthorizeSubscribingAsync(_clientContext, streamPath, streamArguments);

            // Assert
            result.IsAuthorized.Should().BeFalse();
        }

        [Fact]
        public async Task AuthorizeSubscribingAsync_Should_ReturnAuthorizedResult_WhenAuthorized()
        {
            // Arrange
            var streamPath = _fixture.Create<string>();
            var streamArguments = new Dictionary<string, string>();

            _services.AddSingleton<IAuthorizationHandler, AlwaysAuthorizedHandler>();
            var sut = _services.BuildServiceProvider().GetRequiredService<IStreamAuthorization>();

            // Act
            var result = await sut.AuthorizeSubscribingAsync(_clientContext, streamPath, streamArguments);

            // Assert
            result.IsAuthorized.Should().BeTrue();
        }

        [Fact]
        public async Task AuthorizeSubscribingAsync_Should_ReturnAuthorizedResult_WhenAuthCodeIsProvided()
        {
            // Arrange
            var streamPath = _fixture.Create<string>();

            var streamArguments = new Dictionary<string, string>() { { "code", _authCode } };

            _services.AddSingleton<IAuthorizationHandler, AlwaysAuthorizedHandler>();
            _services.AddSingleton<IAuthorizationHandler, AlwaysUnauthorizedHandler>();
            var sut = _services.BuildServiceProvider().GetRequiredService<IStreamAuthorization>();

            // Act
            var result = await sut.AuthorizeSubscribingAsync(_clientContext, streamPath, streamArguments);

            // Assert
            result.IsAuthorized.Should().BeTrue();
        }

        private class AlwaysAuthorizedHandler : IAuthorizationHandler
        {
            public int GetOrder() => 0;

            public Task<AuthorizationResult> AuthorizePublishingAsync(IClientInfo client, string streamPath, IReadOnlyDictionary<string, string> streamArguments, string publishingType)
            {
                return Task.FromResult(AuthorizationResult.Authorized());
            }

            public Task<AuthorizationResult> AuthorizeSubscribingAsync(IClientInfo client, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
            {
                return Task.FromResult(AuthorizationResult.Authorized());
            }
        }

        private class AlwaysUnauthorizedHandler : IAuthorizationHandler
        {
            public int GetOrder() => 10;

            public Task<AuthorizationResult> AuthorizePublishingAsync(IClientInfo client, string streamPath, IReadOnlyDictionary<string, string> streamArguments, string publishingType)
            {
                return Task.FromResult(AuthorizationResult.Unauthorized("test"));
            }

            public Task<AuthorizationResult> AuthorizeSubscribingAsync(IClientInfo client, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
            {
                return Task.FromResult(AuthorizationResult.Unauthorized("test"));
            }
        }
    }
}
