using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using DSharpPlus.SlashCommands.EventArgs;
using Hylix_Bot.Commands;
using Hylix_Bot.Handler;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Hylix_Bot
{
    internal class Hylix
    {
        private static DiscordClient _client { get; set; }
        private static CommandsNextExtension _commands { get; set; }
        private static SlashCommandsExtension _slashCommands { get; set; }        

        static async Task Main(string[] args)
        {
            // Read base config for bot
            var JSONReader = new JSONReader();
            await JSONReader.ReadJSON();            

            await SupabaseHandler.SupabaseInit();

            string token = JSONReader.token;
            if (string.IsNullOrWhiteSpace(token))
            {
                Console.WriteLine("Error: No discord token found!");
                Environment.Exit(1);
            }

            // Client configuration
            var discordConfig = new DiscordConfiguration()
            {
                Intents = DiscordIntents.All,
                Token = token,
                TokenType = TokenType.Bot,
                AutoReconnect = true
            };

            // Commands Configuration
            var commandsConfig = new CommandsNextConfiguration()
            {
                StringPrefixes = new string[]
                {
                    JSONReader.prefix
                },
                EnableMentionPrefix = true,
                EnableDms = false,
                EnableDefaultHelp = false,
                
            };

            // Client Initiation
            _client = new DiscordClient(discordConfig);

            _commands = _client.UseCommandsNext(commandsConfig);
            _slashCommands = _client.UseSlashCommands();

            _slashCommands.RegisterCommands<SlashCommandsManager>();
            _slashCommands.RegisterCommands<SelfCommandsHandler>();

            _slashCommands.SlashCommandExecuted += deferredSlash;
            _slashCommands.SlashCommandErrored += OnErrorOccured;

            //_commands.RegisterCommands<CommandsManager>();
            //_commands.RegisterCommands<SelfCommandsHandler>();

            _client.Ready += ClientReady;

            _client.MessageCreated += MessageHandler;

            await _client.ConnectAsync();

            await Task.Delay(-1);
        }

        private static Task ClientReady(DiscordClient sender, ReadyEventArgs args)
        {
            return Task.CompletedTask;
        }

        private static async Task MessageHandler(DiscordClient Sender, MessageCreateEventArgs args)
        {
            if (args.Author != _client.CurrentUser)
            {
                var _messageHandler = new MessageHandler();

                await _messageHandler.HandleNewUser(args.Author);
                await _messageHandler.MonsterSpawn(args);
            }
        }

        private static async Task deferredSlash(SlashCommandsExtension sender, SlashCommandExecutedEventArgs args)
        {
            if (args.Context.CommandName == "adventure")
            {
                Stopwatch timer = new Stopwatch();
                timer.Start();

                var loadEmbed1 = new DiscordEmbedBuilder()
                {
                    Title = "Out on an adventure."
                };

                var loadEmbed2 = new DiscordEmbedBuilder()
                {
                    Title = "Out on an adventure.."
                };

                var loadEmbed3 = new DiscordEmbedBuilder()
                {
                    Title = "Out on an adventure..."
                };

                var loadEmbed4 = new DiscordEmbedBuilder()
                {
                    Title = "Out on an adventure...."
                };

                await Task.Delay(1000);

                await args.Context.EditResponseAsync(new DiscordWebhookBuilder().WithContent("").AddEmbed(loadEmbed1));

                await Task.Delay(1000);

                await args.Context.EditResponseAsync(new DiscordWebhookBuilder().WithContent("").AddEmbed(loadEmbed2));

                await Task.Delay(1000);

                await args.Context.EditResponseAsync(new DiscordWebhookBuilder().WithContent("").AddEmbed(loadEmbed3));

                await Task.Delay(1000);

                await args.Context.EditResponseAsync(new DiscordWebhookBuilder().WithContent("").AddEmbed(loadEmbed4));
            }
        }

        private static Task OnErrorOccured(SlashCommandsExtension sender, SlashCommandErrorEventArgs args)
        {
            args.Context.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            var errorEmbed = new DiscordEmbedBuilder().WithColor(DiscordColor.Red);

            if (args.Exception is SlashExecutionChecksFailedException)
            {
                var castedException = (SlashExecutionChecksFailedException)args.Exception;
                var timeLeft = string.Empty;

                foreach (var err in castedException.FailedChecks)
                {
                    var cooldown = (SlashCooldownAttribute)err;
                    TimeSpan rawTime = cooldown.GetRemainingCooldown(args.Context);

                    if (rawTime.Days != 0) timeLeft += $"{rawTime.Days} days, ";
                    if (rawTime.Hours != 0) timeLeft += $"{rawTime.Hours} hours, ";
                    if (rawTime.Minutes != 0) timeLeft += $"{rawTime.Minutes} minutes, ";
                    if (rawTime.Seconds != 0) timeLeft += $"{rawTime.Seconds} seconds";
                }

                errorEmbed.WithDescription($"Take a breather! You can use this command again in {timeLeft}");

                var res = new DiscordFollowupMessageBuilder().AddEmbed(errorEmbed);

                args.Context.FollowUpAsync(res);
            }

            return Task.CompletedTask;
        }
    }
}
