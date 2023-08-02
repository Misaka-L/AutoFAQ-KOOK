using Kook;
using Kook.Commands;
using Kook.WebSocket;
using AutoFAQ.Core.CardMessages;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace AutoFAQ.Core.Services {
    public class CommandHandleService : IHostedService {
        private readonly KookSocketClient _socketClient;
        private readonly ILogger<CommandHandleService> _logger;
        private readonly CommandService _commandService;
        private readonly IServiceProvider _serviceProvider;

        public CommandHandleService(ILogger<CommandHandleService> logger, CommandService commandService, KookSocketClient socketClient, IServiceProvider serviceProvider) {
            _logger = logger;
            _commandService = commandService;
            _serviceProvider = serviceProvider;
            _socketClient = socketClient;
        }

        public async Task StartAsync(CancellationToken cancellationToken) {
            _commandService.Log += _commandService_Log;
            _commandService.CommandExecuted += _commandService_CommandExecuted;
            _socketClient.MessageReceived += _socketClient_MessageReceived;
            _socketClient.DirectMessageReceived += _socketClient_MessageReceived;

            await _commandService.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);
        }

        private async Task _commandService_CommandExecuted(Optional<CommandInfo> command, ICommandContext context, IResult result) {
            if (!result.IsSuccess && result.Error is CommandError error && error != CommandError.UnknownCommand && error != CommandError.UnmetPrecondition) {
                await context.Channel.SendCardMessageAsync(new CommandErrorCardMessage(command, context, result).Build());
            }
        }

        private async Task _socketClient_MessageReceived(SocketMessage arg) {
            if (arg is SocketUserMessage message && message.Type == MessageType.KMarkdown) {
                int argPos = 0;

                if (message.HasCharPrefix('.', ref argPos) && !message.Author.IsBot.GetValueOrDefault(false)) {
                    var context = new SocketCommandContext(_socketClient, message);
                    await _commandService.ExecuteAsync(context, argPos, _serviceProvider);
                }
            }
        }

        private Task _commandService_Log(LogMessage message) {
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
                _logger.Log(severity, message.Exception, $"[{message.Source}] {message.Message}");
            } else {
                _logger.Log(severity, message.Message, $"[{message.Source}] {message.Message}");
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) {
            return Task.CompletedTask;
        }
    }
}
