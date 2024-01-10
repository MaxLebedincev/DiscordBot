using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace Sudy_v3.Modules
{
    public class BasicCommands : ModuleBase<SocketCommandContext>
    {
        private readonly ConfigurationBot _config;
        public BasicCommands(IServiceProvider services)
        {
            _config = services.GetRequiredService<ConfigurationBot>();
        }

        [Command("help")]
        [Summary("Получить команды.")]
        public async Task Help()
            => await ReplyAsync(@"**s! - префикс**
```
hi              - Поприветствовать Sudy.
apple [@User]   - Поделиться яблочком.
avatar          - Получить аватар пользователя.
thanks          - Ссылка на вспомогательный код (.NET Core 3.0).
```");                                                                      //TODO Доработать, возможно убрать исходник


        [Command("hi")]
        [Summary("Поприветствовать Sudy.")]
        public async Task Hello()
            => await ReplyAsync($"Привет, **{Context.User.Username}**!");

        [Command("apple")]
        [Summary("Поделиться яблочком.")]
        public async Task Apple(SocketUser? user = null)
        {
            if (user == null)
                await ReplyAsync($"{Context.User.Mention} не с кем поделиться яблочком... :(");
            else if (Context.Message.Author.Id == user.Id)
                await ReplyAsync($"{Context.User.Mention} грустно сидит и одиноко ест яблочко... (Т_Т)");
            else if (user.Id == Context.Client.CurrentUser.Id)                                                 
                await ReplyAsync($"Ой, как мило, спасибо... :hearts:");
            else
                await ReplyAsync($"{Context.User.Mention} поделился яблочком с **{user.Username}** :apple:");
        }

        [Command("avatar")]
        [Summary("Получить аватар пользователя.")]
        public async Task GetAvatar([Remainder] SocketUser? user = null)
            => await ReplyAsync($"Аватарка - **{(user = user ?? Context.User as SocketUser).Username}** \n{Functions.GetAvatarUrl(user)}");

        [Command("thanks")]
        [Summary("Ссылка на вспомогательный код.")]
        public async Task Source()
            => await ReplyAsync($":heart: **{Context.Client.CurrentUser}** is based on this source code:\nhttps://github.com/VACEfron/Discord-Bot-Csharp");

        [Command("ffmpeg")]
        [Summary("Ссылка на вспомогательный код.")]
        public async Task SourceFFmpeg()
        {
            await ReplyAsync($"{FFmpeg.AutoGen.ffmpeg.avfilter_configuration()}");
        }
    }
}
