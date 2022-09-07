using Discord;
using Discord.Audio;
using Discord.Audio.Streams;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace Sudy_v3.Modules
{
    public class AudioCommands : ModuleBase<SocketCommandContext>
    {
        private readonly ConfigurationBot _config;
        public AudioCommands(IServiceProvider services)
        {
            _config = services.GetRequiredService<ConfigurationBot>();
        }

        [Command("join", RunMode = RunMode.Async)]
        public async Task JoinChannel(IVoiceChannel? channel = null)
        {
            channel = channel ?? (Context.User as IGuildUser)?.VoiceChannel;
            if (channel == null) { await Context.Channel.SendMessageAsync($"А, {Context.User.Mention}, выйдет?"); return; }

            IAudioClient? audio = await channel.ConnectAsync();

            await SendAsync(audio, "E:\\Project\\Sudy_v3\\rwby.mp3");

            await Record(audio);

            await SendAsync(audio, "E:\\Project\\Sudy_v3\\rwby.mp3");

        }

        private async Task SendAsync(IAudioClient client, string path)
        {
            // Create FFmpeg using the previous example
            using (var ffmpeg = CreateStream(path))
            using (var output = ffmpeg.StandardOutput.BaseStream)
            using (var discord = client.CreatePCMStream(AudioApplication.Mixed))
            {
                try { await output.CopyToAsync(discord); }
                finally { await discord.FlushAsync(); }
            }
        }

        //break
        private async Task Record(IAudioClient audio)
        {
            MemoryStream mem = new MemoryStream(new byte[1024]);
            FileStream fileStream = new FileStream("E:\\Project\\Sudy_v3\\fileM.mp3", FileMode.Create, System.IO.FileAccess.Write);
            using (AudioInStream c = audio.GetStreams().First().Value)
            {
                Console.WriteLine("Начал запись");
                await Task.Delay(2000);
                await c.CopyToAsync(mem);
                Console.WriteLine("Закончил запись");
            }
            mem.WriteTo(fileStream);
        }

        private Process? CreateStream(string path)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
            });
        }


        [Command("leave", RunMode = RunMode.Async)]
        public async Task LeaveChannel(IVoiceChannel? channel = null)
        {
            channel = channel ?? (Context.User as IGuildUser)?.VoiceChannel;
            if (channel == null) { await Context.Channel.SendMessageAsync($"Ты что-то говорил, {Context.User.Mention}? тебя не слышно"); return; }

            await Context.Channel.SendMessageAsync($"Всем пока!");

            await channel.DisconnectAsync();
        }
    }
}
