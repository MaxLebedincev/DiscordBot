using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Sudy_v3.Modules;
using Sudy_v3.Parsers;
using System;
using System.Reflection;

namespace Sudy_v3
{
    internal class CommandHandlingService
    {
        private readonly InteractionService _interactionService;
        private readonly DiscordSocketClient _client;
        private readonly ConfigurationBot _config;
        private readonly IServiceProvider _services;

        public CommandHandlingService(IServiceProvider services)
        {
            _interactionService = services.GetRequiredService<InteractionService>();
            _client = services.GetRequiredService<DiscordSocketClient>();
            _config = services.GetRequiredService<ConfigurationBot>();
            _services = services;

            // Event handlers
            _client.Ready += ClientReadyAsync;
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
            SlashCommandParser slashParser = new SlashCommandParser(_config.LocalStorage.SlashCommand);

            await _client.BulkOverwriteGlobalApplicationCommandsAsync(slashParser.GetCommandProperties());

            await new Functions(_services).SetBotStatusAsync(_client);
        }

        public async Task InitializeModules()
        {
            await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }
    }
}
 