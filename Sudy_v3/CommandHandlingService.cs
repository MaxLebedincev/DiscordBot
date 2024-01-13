using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Sudy_v3.Modules;
using System;
using System.Reflection;

namespace Sudy_v3
{
    internal class CommandHandlingService
    {
        
        private readonly CommandService _commands;
        private readonly InteractionService _interactionService;
        private readonly DiscordSocketClient _client;
        private readonly ConfigurationBot _config;
        private readonly IServiceProvider _services;

        public CommandHandlingService(IServiceProvider services)
        {

            _commands = services.GetRequiredService<CommandService>();
            _interactionService = services.GetRequiredService<InteractionService>();
            _client = services.GetRequiredService<DiscordSocketClient>();
            _config = services.GetRequiredService<ConfigurationBot>();
            _services = services;

            // Event handlers
            _client.Ready += ClientReadyAsync;
            _client.MessageReceived += HandleCommandAsync;
            _client.JoinedGuild += SendJoinMessageAsync;
            _client.InteractionCreated += SlashCommandHandler;
        }

        private async Task SlashCommandHandler(SocketInteraction arg)
        {
            var context = new SocketInteractionContext(_client, arg);

            var result = await _interactionService.ExecuteCommandAsync(context, _services);

            if (!result.IsSuccess && result.Error.HasValue)
                return;
        }

        private async Task HandleCommandAsync(SocketMessage rawMessage)
        {
            if (rawMessage.Author.IsBot || !(rawMessage is SocketUserMessage message) || message.Channel is IDMChannel 
                || (_config.IgnorGuild != null && _config.IgnorGuild.Contains(((SocketGuildUser)rawMessage.Author).Guild.Id.ToString())))
                return;

            var context = new SocketCommandContext(_client, message);

            int argPos = 0;

            if (message.HasStringPrefix(_config.Prefix, ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                //if (rawMessage.Author.Id != 412277201917050881)
                //{
                //    //await rawMessage.Channel.SendMessageAsync("У вас не хватает прав для этого!");
                //    return;
                //}

                var result = await _commands.ExecuteAsync(context, argPos, _services);

                if (!result.IsSuccess && result.Error.HasValue)
                    return;
            }
        }

        private async Task SendJoinMessageAsync(SocketGuild guild)
        {
            
            string joinMessage = "Все привет!";

            if (string.IsNullOrEmpty(joinMessage))
                return;

            // Send the join message in the first channel where the bot can send messsages.
            foreach (var channel in guild.TextChannels.OrderBy(x => x.Position))
            {
                var botPerms = channel.GetPermissionOverwrite(_client.CurrentUser).GetValueOrDefault();

                if (botPerms.SendMessages == PermValue.Deny)
                    continue;

                try
                {
                    await channel.SendMessageAsync(joinMessage);
                    return;
                }
                catch
                {
                    continue;
                }
            }
        }

        private async Task ClientReadyAsync()
        {
            List<ApplicationCommandProperties> applicationCommandProperties = new();

            SlashCommandBuilder globalCommandHelp = new SlashCommandBuilder()
                .WithName("help")
                .WithDescription("Shows information about the bot.");
            applicationCommandProperties.Add(globalCommandHelp.Build());

            SlashCommandBuilder globalCommandHi = new SlashCommandBuilder()
                .WithName("hi")
                .WithDescription("Поприветствовать Sudy.");
            applicationCommandProperties.Add(globalCommandHi.Build());

            SlashCommandBuilder globalCommandapple = new SlashCommandBuilder()
                .WithName("apple")
                .WithDescription("Поделиться яблочком.")
                .AddOption("user", ApplicationCommandOptionType.User, "Кому вы дадите яблоко?", isRequired: false);
            applicationCommandProperties.Add(globalCommandapple.Build());

            SlashCommandBuilder globalCommandAva = new SlashCommandBuilder()
                .WithName("avatar")
                .WithDescription("Получить аватар пользователя.")
                .AddOption("user", ApplicationCommandOptionType.User, "Чья аватарка?", isRequired: true);
            applicationCommandProperties.Add(globalCommandAva.Build());

            SlashCommandBuilder globalCommandThanks = new SlashCommandBuilder()
                .WithName("thanks")
                .WithDescription("Ссылка на вспомогательный код.");
            applicationCommandProperties.Add(globalCommandThanks.Build());

            SlashCommandBuilder globalCommandFFmpeg = new SlashCommandBuilder()
                .WithName("ffmpeg")
                .WithDescription("Ссылка на вспомогательный код.");
            applicationCommandProperties.Add(globalCommandFFmpeg.Build());

            await _client.BulkOverwriteGlobalApplicationCommandsAsync(applicationCommandProperties.ToArray());

            await new Functions(_services).SetBotStatusAsync(_client);
        }

        public async Task InitializeModules()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }
    }
}
 