using allstars.Utils;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using NLog;
using NLog.Conditions;
using NLog.Config;
using NLog.Targets;
using System;
using System.IO;
using System.Threading.Tasks;

namespace allstars.Services
{
    public class LoggingService
    {
        private readonly Logger Log = LogManager.GetCurrentClassLogger();
        private readonly DiscordSocketClient DiscordClient;
        private readonly CommandService CommandService;

        private string LogDirectory => Path.Combine(AppContext.BaseDirectory, Constants.LogsFolderName);
        private string LogFile => Path.Combine(LogDirectory, $"{DateTime.UtcNow.ToString("yyyy-MM-dd")}.log");

        public LoggingService(DiscordSocketClient discord, CommandService commandService)
        {
            DiscordClient = discord;
            CommandService = commandService;

            Init();

            DiscordClient.Log += OnLogAsync;
            CommandService.Log += OnLogAsync;
        }

        internal void Init()
        {
            try
            {
                var logConfig = new LoggingConfiguration();
                var consoleTarget = new ColoredConsoleTarget();
                var fileTarget = new FileTarget();

                var highlightRuleInfo = new ConsoleRowHighlightingRule
                {
                    Condition = ConditionParser.ParseExpression("level == LogLevel.Info"),
                    ForegroundColor = ConsoleOutputColor.DarkGreen
                };

                consoleTarget.RowHighlightingRules.Add(highlightRuleInfo);

                consoleTarget.Layout = Constants.LogMessageLayout;
                fileTarget.FileName = Path.Combine(LogDirectory, $"{DateTime.UtcNow.ToString("yyyy-MM-dd")}.log");
                fileTarget.Layout = Constants.LogMessageLayout;

                logConfig.AddTarget("console", consoleTarget);
                logConfig.AddTarget("file", fileTarget);

                var rule1 = new LoggingRule("*", LogLevel.Debug, consoleTarget);
                logConfig.LoggingRules.Add(rule1);

                var rule2 = new LoggingRule("*", LogLevel.Debug, fileTarget);
                logConfig.LoggingRules.Add(rule2);

                LogManager.Configuration = logConfig;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        internal Task OnLogAsync(LogMessage message)
        {
            switch (message.Severity)
            {
                case LogSeverity.Info:
                    Log.Info($"[{message.Source}] {message.Message}");
                    break;

                case LogSeverity.Warning:
                    Log.Warn($"[{message.Source}] {message.Message} | {message.Exception}");
                    break;

                case LogSeverity.Error:
                    Log.Error($"[{message.Source}] {message.Message} | {message.Exception}");
                    break;

                case LogSeverity.Critical:
                    Log.Fatal($"[{message.Source}] {message.Message} | {message.Exception}");
                    break;

                case LogSeverity.Debug:
                    Log.Debug($"[{message.Source}] {message.Message}");
                    break;

                case LogSeverity.Verbose:
                    Log.Trace($"[{message.Source}] {message.Message}");
                    break;
            }

            return Task.CompletedTask;
        }
    }
}