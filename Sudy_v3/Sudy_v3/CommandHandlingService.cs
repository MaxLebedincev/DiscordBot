using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Sudy_v3
{
    internal class CommandHandlingService
    {
        
        private readonly CommandService _commands;
        private readonly DiscordSocketClient _client;
        private readonly ConfigurationBot _config;
        private readonly IServiceProvider _services;

        public CommandHandlingService(IServiceProvider services)
        {

            _commands = services.GetRequiredService<CommandService>();
            _client = services.GetRequiredService<DiscordSocketClient>();
            _config = services.GetRequiredService<ConfigurationBot>();
            _services = services;

            // Event handlers
            _client.Ready += ClientReadyAsync;
            _client.MessageReceived += HandleCommandAsync;
            _client.JoinedGuild += SendJoinMessageAsync;
            //_client.Connected += SendAsync;
        }

        private async Task HandleCommandAsync(SocketMessage rawMessage)
        {
            if (rawMessage.Author.IsBot || !(rawMessage is SocketUserMessage message) || message.Channel is IDMChannel)
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

        //private async Task SendAsync()
        //{
        //    int a = 5;
        //}

        private async Task ClientReadyAsync()
            => await new Functions(_services).SetBotStatusAsync(_client);

        public async Task InitializeAsync()
            => await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
    }
}
