using CoinGecko.Entities.Response.Coins;
using CoinGecko.Entities.Response.Simple;
using CoinGecko.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord.Rest;
using Discord.WebSocket;
using Discord;
using System.Net;
using System.Net.Http;
using System.IO;

namespace InverseCurveSidebarBot
{
    public class TimedBackgroundPegService : IHostedService, IDisposable
    {
        private readonly ILogger<TimedBackgroundPegService> _logger;
        private readonly BotSettings _settings;
        private DiscordRestClient _discordRestClient = null!;
        private DiscordSocketClient _discordSocketClient = null!;
        private CurveExchangeRateService _curveExchangeRateService = null!;

        private string _coinName = null!;
        private bool _firstRun = true;

        private Timer _timer = null!;

        private Dictionary<ulong, (ulong?, ulong?)> _guildToRoleIds;
        class PriceBotActivity : IActivity
        {
            public string Name { get; set; }

            public ActivityType Type => ActivityType.Playing;

            public ActivityProperties Flags => ActivityProperties.None;
            public string Details { get; set; }
        }

        public TimedBackgroundPegService(ILogger<TimedBackgroundPegService> logger, 
            IOptions<BotSettings> settings, 
            DiscordRestClient discordRestClient,
            DiscordSocketClient discordSocketClient,
            CurveExchangeRateService curveExchangeRateService)
        {
            _logger = logger;
            _settings = settings.Value;
            _discordRestClient = discordRestClient;
            _discordSocketClient = discordSocketClient;
            _curveExchangeRateService = curveExchangeRateService;

            _guildToRoleIds = new Dictionary<ulong, (ulong?, ulong?)>();
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            ExecuteAsync().Wait();
            return Task.CompletedTask;
        }

        private async Task ExecuteAsync()
        {
            _logger.LogInformation($"{nameof(TimedBackgroundPegService)} running.");
            
            await _discordRestClient.LoginAsync(TokenType.Bot, _settings.BotToken);
            await _discordSocketClient.LoginAsync(TokenType.Bot, _settings.BotToken);
            await _discordSocketClient.StartAsync();

            _discordSocketClient.Connected += DiscordSocketClientConnected;

            _timer = new Timer(DoWork, null, TimeSpan.FromSeconds(_settings.Delay),
                TimeSpan.FromSeconds(_settings.UpdateInterval));
        }

        private async Task DiscordSocketClientConnected()
        {
            await SetLogo("anchor.png");
            return;
        }

        private async Task SetLogo(string dir)
        {
            await _discordSocketClient.CurrentUser.ModifyAsync((p) =>
            {
                p.Avatar = new Optional<Image?>(new Image(dir));
            });
        }

        private void DoWork(object? state)
        {
            Process().GetAwaiter().GetResult();
        }

        private async Task Process()
        {
            try
            {
                string nickname = _settings.Nickname ?? "Curve Bot";
                string playing = string.Empty;

                var exchangeRate = await _curveExchangeRateService.GetExchangeRateWithoutFees();
                var exchangeRateFormated = exchangeRate.ToString("C4");
                var unitsFormatted = string.Format(new ShortHandCurrencyFormatter(), "{0:SH}", _settings.Units);

                playing = $"{exchangeRateFormated} @ {unitsFormatted}";

                await UpdateDiscordInfo(nickname, playing);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error occured while attempting to update, skipping this pass...");
            }
        }

        private async Task UpdateDiscordInfo(string nickname, string playing)
        {
            var guilds = await _discordRestClient.GetGuildsAsync();

            var activity = new PriceBotActivity();

            activity.Name = playing;
            activity.Details = playing;

            _logger.LogInformation($"{nickname} - {playing}");
            Console.WriteLine($"{nickname} - {playing}");

            await _discordSocketClient.SetActivityAsync(activity);

            foreach (var guild in guilds)
            {
                var user = await guild.GetCurrentUserAsync();

                try
                {
                    await user.ModifyAsync(x =>
                    {
                        x.Nickname = nickname;
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, null!, null!);
                    Console.Error.WriteLine(ex);
                }
            }
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"{nameof(TimedBackgroundPegService)} is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}

