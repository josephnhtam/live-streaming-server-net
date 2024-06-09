using LiveStreamingServerNet.Networking;
using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Auth;
using LiveStreamingServerNet.Rtmp.Auth.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net;

namespace LiveStreamingServerNet.AuthorizationDemo
{
    public static class Program
    {
        public static async Task Main()
        {
            using var liveStreamingServer = CreateLiveStreamingServer();

            await liveStreamingServer.RunAsync(new ServerEndPoint(new IPEndPoint(IPAddress.Any, 1935), false));
        }

        private static ILiveStreamingServer CreateLiveStreamingServer()
        {
            return LiveStreamingServerBuilder.Create()
                .ConfigureRtmpServer(options =>
                {
                    options.Services.AddSingleton<IPasswordValidator, DemoPasswordValidator>();
                    options.AddAuthorizationHandler<DemoAuthorizationHandler>();
                })
                .ConfigureLogging(options => options.AddConsole().SetMinimumLevel(LogLevel.Debug))
                .Build();
        }
    }

    public interface IPasswordValidator
    {
        bool ValidatePassword(string password);
    }

    public class DemoPasswordValidator : IPasswordValidator
    {
        public bool ValidatePassword(string password)
        {
            return password == "123456";
        }
    }

    public class DemoAuthorizationHandler : IAuthorizationHandler
    {
        private readonly IPasswordValidator _passwordValidator;

        public DemoAuthorizationHandler(IPasswordValidator passwordValidator)
        {
            _passwordValidator = passwordValidator;
        }

        public Task<AuthorizationResult> AuthorizePublishingAsync(
            IClientInfo client,
            string streamPath,
            IReadOnlyDictionary<string, string> streamArguments,
            string publishingType)
        {
            // Accepting only the publishing path that includes a valid password parameter
            // For example: rtmp://127.0.0.1:1935/live/stream?password=123456
            if (streamArguments.TryGetValue("password", out var password) && _passwordValidator.ValidatePassword(password))
                return Task.FromResult(AuthorizationResult.Authorized());

            return Task.FromResult(AuthorizationResult.Unauthorized("incorrect password"));
        }

        public Task<AuthorizationResult> AuthorizeSubscribingAsync(
            IClientInfo client,
            string streamPath,
            IReadOnlyDictionary<string, string> streamArguments)
        {
            return Task.FromResult(AuthorizationResult.Authorized());
        }
    }
}
