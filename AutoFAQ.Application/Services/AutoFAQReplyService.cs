using Kook.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AutoFAQ.Application.Services
{
    public class AutoFAQReplyService : IHostedService
    {
        private readonly AutoFAQService _autoFAQService;
        private readonly KookSocketClient _socketClient;
        private readonly ILogger<AutoFAQReplyService> _logger;

        public AutoFAQReplyService(AutoFAQService autoFAQService, KookSocketClient socketClient,
            ILogger<AutoFAQReplyService> logger)
        {
            _autoFAQService = autoFAQService;
            _socketClient = socketClient;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _socketClient.MessageReceived += _socketClient_MessageReceived;

            return Task.CompletedTask;
        }

        private async Task _socketClient_MessageReceived(SocketMessage socketMessage, SocketGuildUser socketGuildUser,
            SocketTextChannel socketTextChannel)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            //cancellationTokenSource.CancelAfter(1000);

            if (socketMessage is SocketUserMessage userMessage &&
                !userMessage.Author.IsBot.GetValueOrDefault(true) &&
                userMessage.MentionedUsers.Any(user => user.Id == _socketClient.CurrentUser.Id) &&
                userMessage.Channel is SocketTextChannel channel)
            {
                _logger.LogInformation("{author} 在 {guild} 的 {channel} ({channel_id}) 频道触发回复", userMessage.Author,
                    userMessage.Guild, channel.Name, channel.Id);

                try
                {
                    await Task.Run(async delegate
                    {
                        var questions = await _autoFAQService.GetQuestionsAsync(channel.Guild.Id);
                        foreach (var question in questions)
                        {
                            var regex = new Regex(question.Regex, RegexOptions.IgnoreCase);
                            if (regex.IsMatch(userMessage.Content))
                            {
                                _logger.LogInformation(
                                    "回复 {author} 在 {guild} 的 {channel} ({channel_id}) 的提问，问题 ID: {question_id}",
                                    userMessage.Author, userMessage.Guild, channel.Name, channel.Id, question.Id);
                                await userMessage.Channel.SendTextAsync(question.Answer);
                            }
                        }
                    }, cancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogError("{author} 在 {guild} 的 {channel} ({channel_id}) 频道的提问操作超时，强制取消", userMessage.Author,
                        userMessage.Guild, channel.Name, channel.Id);
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _socketClient.MessageReceived -= _socketClient_MessageReceived;
            return Task.CompletedTask;
        }
    }
}