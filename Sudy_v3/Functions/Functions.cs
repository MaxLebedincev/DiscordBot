using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace Sudy_v3
{
    internal class Functions
    {
        private readonly ConfigurationBot _config;
        public Functions(IServiceProvider services)
        {
            _config = services.GetRequiredService<ConfigurationBot>();
        }

        /// <summary>
        /// Установка статуса для бота
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public async Task SetBotStatusAsync(DiscordSocketClient client)
        {
            string? currently = _config.Currently;
            string? statusText = _config.StatusText;
            string? onlineStatus = _config.Status;

            if (!string.IsNullOrEmpty(onlineStatus))
            {
                UserStatus userStatus = onlineStatus switch
                {
                    "dnd"       => UserStatus.DoNotDisturb,
                    "idle"      => UserStatus.Idle,
                    "offline"   => UserStatus.Invisible,
                    "afk"       => UserStatus.AFK,
                    "invisible" => UserStatus.Invisible,
                    _ => UserStatus.Online
                };

                await client.SetStatusAsync(userStatus);
                Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} | Online status set | {userStatus}");
            }

            if (!string.IsNullOrEmpty(currently) && !string.IsNullOrEmpty(statusText))
            {
                ActivityType activity = currently switch
                {
                    "listening"     => ActivityType.Listening,
                    "watching"      => ActivityType.Watching,
                    "streaming"     => ActivityType.Streaming,
                    "customstatus"  => ActivityType.CustomStatus,
                    "competing"     => ActivityType.Competing,
                    _ => ActivityType.Playing
                };

                await client.SetGameAsync(statusText, type: activity);
                Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} | Playing status set | {activity}: {statusText}");
            }
        }

        /// <summary>
        /// Получает аватар пользователя с расширением на 1024
        /// </summary>
        /// <param name="user"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static string GetAvatarUrl(SocketUser user, ushort size = 1024)
        {
            return user.GetAvatarUrl(size: size) ?? user.GetDefaultAvatarUrl();
        }
    }
}
