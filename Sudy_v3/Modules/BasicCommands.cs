using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace Sudy_v3.Modules
{
    public class BasicCommands : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly ConfigurationBot _config;
        public BasicCommands(IServiceProvider services)
        {
            _config = services.GetRequiredService<ConfigurationBot>();
        }

        [SlashCommand("help", "Показать информацию о боте.")]
        public async Task Help()
            => await RespondAsync(@"
```
hi              - Поприветствовать Sudy.
apple [@User]   - Поделиться яблочком.
avatar          - Получить аватар пользователя.
thanks          - Ссылка на вспомогательный код (.NET Core 3.0).
```
");                                                                      //TODO Доработать, возможно убрать исходник


        [SlashCommand("hi", "Поприветствовать Sudy.")]
        public async Task Hello()
            => await RespondAsync($"Привет, **{Context.User.Username}**!");

        [SlashCommand("apple", "Поделиться яблочком.")]
        public async Task Apple(SocketUser? user = null)
        {
            if (user == null)
                await RespondAsync($"{Context.User.Mention} не с кем поделиться яблочком... :(");
            else if (Context.User.Id == user.Id)
                await RespondAsync($"{Context.User.Mention} грустно сидит и одиноко ест яблочко... (Т_Т)");
            else if (user.Id == Context.Client.CurrentUser.Id)
                await RespondAsync($"Ой, как мило, спасибо... :hearts:");
            else
                await RespondAsync($"{Context.User.Mention} поделился яблочком с **{user.Username}** :apple:");
        }

        [SlashCommand("avatar", "Получить аватар пользователя.")]
        public async Task GetAvatar(SocketUser user)
            => await RespondAsync($"Аватарка - **{user.Username}** \n{Functions.GetAvatarUrl(user)}");

        [SlashCommand("thanks", "Ссылка на вспомогательный код.")]
        public async Task Source()
            => await RespondAsync($":heart: **{Context.Client.CurrentUser}** is based on this source code:\nhttps://github.com/VACEfron/Discord-Bot-Csharp");

        [SlashCommand("ffmpeg", "Ссылка на вспомогательный код.")]
        public async Task SourceFFmpeg()
        {
            string msg = FFmpeg.AutoGen.ffmpeg.avfilter_configuration();

            await RespondAsync($"{msg}");
        }
    }
}
