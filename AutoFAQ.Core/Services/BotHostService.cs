using Kook;
using Kook.WebSocket;
using AutoFAQ.Core.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AutoFAQ.Core.Services {
    public class BotHostService : IHostedService {
        private readonly IOptions<KookSettings> _options;
        private readonly ILogger<BotHostService> _logger;
        private readonly ILogger<KookSocketClient> _socketClientLogger;
        private readonly IHostApplicationLifetime _appLifetime;
        private readonly KookSocketClient _socketClient;

        public BotHostService(ILogger<BotHostService> logger,
            ILogger<KookSocketClient> socketClientLogger,
            IHostApplicationLifetime appLifetime,
            KookSocketClient socketClient,
            IOptions<KookSettings> options) {
            _logger = logger;
            _appLifetime = appLifetime;
            _socketClientLogger = socketClientLogger;
            _socketClient = socketClient;
            _options = options;
        }

        public async Task StartAsync(CancellationToken cancellationToken) {
            _socketClient.Log += _socketClient_Log;

            await _socketClient.LoginAsync(TokenType.Bot, _options.Value.Token);
            await _socketClient.StartAsync();
        }

        private Task _socketClient_Log(LogMessage message) {
            var severity = message.Severity switch {
                LogSeverity.Critical => LogLevel.Critical,
                LogSeverity.Error => LogLevel.Error,
                LogSeverity.Warning => LogLevel.Warning,
                LogSeverity.Info => LogLevel.Information,
                LogSeverity.Verbose => LogLevel.Trace,
                LogSeverity.Debug => LogLevel.Debug,
                _ => LogLevel.Information
            };

            if (message.Exception != null) {
                _socketClientLogger.Log(severity, message.Exception, $"[{message.Source}] {message.Message}");
            } else {
                _socketClientLogger.Log(severity, message.Message, $"[{message.Source}] {message.Message}");
            }

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken) {
            await _socketClient.StopAsync();
            await _socketClient.LogoutAsync();
        }
    }
}
