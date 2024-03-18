using Discord;
using Discord.Audio;
using Discord.Audio.Streams;
using Discord.Commands;
using Discord.Interactions;
using FFmpeg.AutoGen;
using Microsoft.Extensions.DependencyInjection;
using Sudy_v3.FFmpegDyHelper;
using System.Diagnostics;
using System.Net.Sockets;
using System.Runtime.Intrinsics.X86;
using VideoLibrary;

namespace Sudy_v3.Modules
{
    public class AudioCommands : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly ConfigurationBot _config;

        public AudioCommands(IServiceProvider services)
        {
            _config = services.GetRequiredService<ConfigurationBot>();
        }

        [SlashCommand("join", "Пригласить бота в канал.")]
        public async Task JoinChannel()
        {
            var channel = (Context.User as IGuildUser)?.VoiceChannel ?? null;

            if (channel == null) { await RespondAsync($"А, {Context.User.Mention}, выйдет?"); return; }

            IAudioClient? audio = await channel.ConnectAsync();

            await SendAsync(audio, $@"{AppDomain.CurrentDomain.BaseDirectory}{_config.LocalStorage.Music}test_norm_samle_48000_s16le.dat");//uwu-very.mp3");

        }

        [SlashCommand("play", "Проиграть музыку.")]
        public async Task PlayMusic(string url)
        {

            var channel = (Context.User as IGuildUser)?.VoiceChannel ?? null;

            if (channel == null) { await RespondAsync($"А, {Context.User.Mention}, выйдет?"); return; }

            IAudioClient? audio = await channel.ConnectAsync();

            //YouTube youtube = YouTube.Default;

            //YouTubeVideo youtubeVideo = await youtube.GetVideoAsync(@"https://www.youtube.com/watch?v=__-vp0g_BhA");
            //string guidYoutube = $"{Guid.Parse("7dc1b152-5e36-4a96-bb14-45773e6f8d14")}";

            //string outFileName = $@"{AppDomain.CurrentDomain.BaseDirectory}{_config.LocalStorage.Youtube}{guidYoutube}.mp4";
            string staticFileMp3 = $@"{AppDomain.CurrentDomain.BaseDirectory}{_config.LocalStorage.Youtube}sample-12s.mp3";
            string staticOutFile = $@"{AppDomain.CurrentDomain.BaseDirectory}{_config.LocalStorage.Youtube}sample-12s.dat";

            await RespondAsync("Звуки ада!");

            unsafe
            {
                FFmpegDy.decode_audio(staticFileMp3, staticOutFile);
            }

            await SendAsync(audio, staticOutFile);
        }

        private async Task SendAsync(IAudioClient client, string path)
        {
            // Create FFmpeg using the previous example
            //using (var ffmpeg = CreateStream(path))
            //using (var output = ffmpeg.StandardOutput.BaseStream)
            using (var output = new FileStream(path, FileMode.Open))
            using (var discord = client.CreatePCMStream(AudioApplication.Mixed))
            {
                try { await output.CopyToAsync(discord); }
                finally { await discord.FlushAsync(); }
            }
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

        [SlashCommand("leave", "Выгнать бота.")]
        public async Task LeaveChannel()
        {
            var channel = (Context.User as IGuildUser)?.VoiceChannel ?? null;
            
            if (channel == null) { await RespondAsync($"Ты что-то говорил, {Context.User.Mention}? тебя не слышно"); return; }

            await RespondAsync($"Всем пока!");

            await channel.DisconnectAsync();
        }
    }
}
