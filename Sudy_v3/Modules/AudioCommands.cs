using Discord;
using Discord.Audio;
using Discord.Audio.Streams;
using Discord.Commands;
using FFmpeg.AutoGen;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using VideoLibrary;

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

            await SendAsync(audio, $@"{AppDomain.CurrentDomain.BaseDirectory}{_config.LocalStorage.Music}uwu-very.mp3");

        }


        unsafe static AVFormatContext* _pAvFormatInputFileContext = null;
        unsafe static AVFormatContext* _pAvFormatOutputFileContext = null;

        [Command("play", RunMode = RunMode.Async)]
        public async Task JoinChannel(IVoiceChannel? channel = null, IMessage mess = null)
        {
            channel = channel ?? (Context.User as IGuildUser)?.VoiceChannel;
            if (channel == null) { await Context.Channel.SendMessageAsync($"А, {Context.User.Mention}, выйдет?"); return; }

            IAudioClient? audio = await channel.ConnectAsync();

            YouTube youtube = YouTube.Default;

            var v = youtube.GetVideo(@"https://www.youtube.com/watch?v=__-vp0g_BhA");

            string filePath = $@"{AppDomain.CurrentDomain.BaseDirectory}{_config.LocalStorage.Youtube}{Guid.NewGuid()}{v.FileExtension}";
            string outFileName = $@"{AppDomain.CurrentDomain.BaseDirectory}{_config.LocalStorage.Youtube}{Guid.NewGuid()}.mp3";

            File.WriteAllBytes(filePath, v.GetBytes());

            var errorCode = 0;

            unsafe
            {
                fixed (AVFormatContext** ppAvFormatInputFileContext = &_pAvFormatInputFileContext)
                fixed (AVFormatContext** ppAvFormatOutputFileContext = &_pAvFormatOutputFileContext)
                {
                    errorCode = ffmpeg.avformat_open_input(ppAvFormatInputFileContext, filePath, null, null);
                    ExitOnFfmpegError(errorCode, $"unable to open input file {filePath}");

                    AVOutputFormat* AVOF = ffmpeg.av_guess_format("acc", null ,null);
                    //AVOF->audio_codec = AVCodecID.AV_CODEC_ID_AAC;

                    errorCode = ffmpeg.avformat_alloc_output_context2(ppAvFormatOutputFileContext, AVOF, null, outFileName);
                    ExitOnFfmpegError(errorCode, $"unable to alloc context for output file {outFileName}");
                }

                CopyStreamInfo(_pAvFormatOutputFileContext, _pAvFormatInputFileContext);

                // method can produce memory leaks if it is called from several threads concurrently. So if your app is multithreaded, please use lock on some static variable
                errorCode = ffmpeg.avio_open(&_pAvFormatOutputFileContext->pb, outFileName, ffmpeg.AVIO_FLAG_WRITE);
                ExitOnFfmpegError(errorCode, $"error while trying to opening file {outFileName} for writing");

                errorCode = ffmpeg.avformat_write_header(_pAvFormatOutputFileContext, null);
                ExitOnFfmpegError(errorCode, $"error while trying to write header of file {outFileName}");

                CopyDataPackets(_pAvFormatOutputFileContext, _pAvFormatInputFileContext);

                errorCode = ffmpeg.av_write_trailer(_pAvFormatOutputFileContext);
                ExitOnFfmpegError(errorCode, $"error while trying to write trailer of file {outFileName}");

                Cleanup();
            }

            File.Delete(filePath);

        }

        public async Task JoinChannelPrivate(IVoiceChannel? channel, string idUser)
        {
            channel = channel ?? (Context.User as IGuildUser)?.VoiceChannel;
            if (channel == null) { return; }

            string currentPath;

            if (!_config.UserMusic.TryGetValue(idUser, out currentPath)) { return; }

            IAudioClient? audio = await channel.ConnectAsync();

            await SendAsync(audio, @$"{AppDomain.CurrentDomain.BaseDirectory}{_config.LocalStorage.Music}{currentPath}");

            await channel.DisconnectAsync();
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
        //private async Task Record(IAudioClient audio)
        //{
        //    MemoryStream mem = new MemoryStream(new byte[1024]);
        //    FileStream fileStream = new FileStream("fileM.mp3", FileMode.Create, System.IO.FileAccess.Write);
        //    using (AudioInStream c = audio.GetStreams().First().Value)
        //    {
        //        Console.WriteLine("Начал запись");
        //        await Task.Delay(2000);
        //        await c.CopyToAsync(mem);
        //        Console.WriteLine("Закончил запись");
        //    }
        //    mem.WriteTo(fileStream);
        //}

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

        unsafe private static void PrintStreamInfo(int streamIndex, AVStream* pAvStream)
        {
            // There is av_dump_format function that produces almost the same result =)

            Console.WriteLine($"Stream {streamIndex}");
            Console.WriteLine($"\tType: {pAvStream->codecpar->codec_type}");
            Console.WriteLine($"\tCodecId: {pAvStream->codecpar->codec_id}");

            var pCodecDescriptor = ffmpeg.avcodec_descriptor_get(pAvStream->codecpar->codec_id);
            string codecName, codecLongName;
            if (pCodecDescriptor is not null)
            {
                codecName = FfmpegHelper.GetString(pCodecDescriptor->name);
                codecLongName = FfmpegHelper.GetString(pCodecDescriptor->long_name);
            }
            else
            {
                codecName = codecLongName = "Unknown";
            }

            Console.WriteLine($"\tCodec name: {codecName}");
            Console.WriteLine($"\tCodec long name: {codecLongName}");

            Console.WriteLine($"\tBitrate: {pAvStream->codecpar->bit_rate}");

            if (pAvStream->duration > 0)
            {
                var durationInSeconds = pAvStream->duration * ffmpeg.av_q2d(pAvStream->time_base);
                Console.WriteLine($"\tDuration: {TimeSpan.FromSeconds(durationInSeconds)}");
            }

            if (pAvStream->codecpar->codec_type == AVMediaType.AVMEDIA_TYPE_VIDEO)
            {
                Console.WriteLine($"\tWidth: {pAvStream->codecpar->width}");
                Console.WriteLine($"\tHeight: {pAvStream->codecpar->height}");

                var avRationalAspectRatio = pAvStream->codecpar->sample_aspect_ratio;
                Console.WriteLine($"\tSample aspect ratio: {avRationalAspectRatio.num} / {avRationalAspectRatio.den}");

                Console.WriteLine($"\tLevel: {pAvStream->codecpar->level}");
            }
            else if (pAvStream->codecpar->codec_type == AVMediaType.AVMEDIA_TYPE_AUDIO)
            {
                Console.WriteLine($"\tChannels: {pAvStream->codecpar->channels}");
                Console.WriteLine($"\tSample rate: {pAvStream->codecpar->sample_rate}");
                Console.WriteLine($"\tFrame size: {pAvStream->codecpar->frame_size}");
            }

            foreach (var avDictionaryEntry in FfmpegHelper.GetEnumerator(pAvStream->metadata))
            {
                var key = FfmpegHelper.GetString(avDictionaryEntry.key);
                var value = FfmpegHelper.GetString(avDictionaryEntry.value);
                Console.WriteLine($"\t\t{key}: {value}");
            }
        }

        unsafe private static void Cleanup()
        {
            if (_pAvFormatInputFileContext is not null)
            {
                fixed (AVFormatContext** ppAvFormatFileContext = &_pAvFormatInputFileContext)
                    ffmpeg.avformat_close_input(ppAvFormatFileContext);
            }

            if (_pAvFormatOutputFileContext is not null)
            {
                fixed (AVFormatContext** ppAvFormatFileContext = &_pAvFormatOutputFileContext)
                    ffmpeg.avformat_close_input(ppAvFormatFileContext);
            }
        }
        private static void ExitOnFfmpegError(int ffmpegResultCode, string errorDescription)
        {
            if (ffmpegResultCode >= 0)
                return;

            var errorText = FfmpegHelper.GetErrorText(ffmpegResultCode);

            ExitWithErrorText($"{errorDescription}. Ffmpeg error text: {errorText}");
        }

        private static void ExitWithErrorText(string errorText)
        {
            Console.WriteLine(errorText);

            Cleanup();

            Environment.Exit(-1);
        }

        unsafe private static void CopyStreamInfo(AVFormatContext* pAvFormatOutputFileContext, AVFormatContext* pAvFormatInputFileContext)
        {
            for (var i = 0; i < pAvFormatInputFileContext->nb_streams; i++)
            {
                var pAvInputStream = pAvFormatInputFileContext->streams[i];

                if (pAvInputStream->codecpar->codec_type != AVMediaType.AVMEDIA_TYPE_AUDIO) continue;

                PrintStreamInfo(i, pAvInputStream);

                AVStream* pAvOutputStream = ffmpeg.avformat_new_stream(pAvFormatOutputFileContext, null);

                if (pAvOutputStream is null)
                    ExitWithErrorText($"Unable to create new stream #{i} for output file");

                //var errorCode = ffmpeg.avcodec_parameters_copy(pAvOutputStream->codecpar, pAvInputStream->codecpar);

                //ffmpeg.avcodec_open2(pAvFormatOutputFileContext, ffmpeg.avcodec_find_encoder_by_name("libfdk_aac"), null);

                //ExitOnFfmpegError(errorCode, $"error while trying to copy stream #{i} parameters");

                //AVCodecContext* encoder_ctx;

                //encoder_ctx->codec_id = AVCodecID.

                uint codecTag = 0;
                if (ffmpeg.av_codec_get_tag2(pAvFormatOutputFileContext->oformat->codec_tag,
                        pAvInputStream->codecpar->codec_id, &codecTag) == 0)
                {
                    Console.WriteLine(
                        $"Warning: could not find codec tag for codec id {pAvInputStream->codecpar->codec_id}, default to 0.");
                }

                pAvOutputStream->codecpar->codec_tag = codecTag;

                for (var j = 0; j < pAvInputStream->nb_side_data; j++)
                {
                    var avPacketSideData = pAvInputStream->side_data[j];

                    var pAvOutPacketSideData =
                        ffmpeg.av_stream_new_side_data(pAvOutputStream, avPacketSideData.type, avPacketSideData.size);

                    if (pAvOutPacketSideData is null)
                        ExitWithErrorText($"Unable to allocate side data structure for stream #{i} of output file");

                    Buffer.MemoryCopy(avPacketSideData.data, pAvOutPacketSideData, avPacketSideData.size,
                        avPacketSideData.size);
                }

                ffmpeg.av_dict_copy(&pAvOutputStream->metadata, pAvInputStream->metadata, ffmpeg.AV_DICT_APPEND);
            }
        }

        unsafe private static void CopyDataPackets(AVFormatContext* pAvFormatOutputFileContext, AVFormatContext* pAvFormatInputFileContext)
        {
            AVPacket avPacket;
            while (true)
            {
                ffmpeg.av_packet_unref(&avPacket);
                var errorCode = ffmpeg.av_read_frame(pAvFormatInputFileContext, &avPacket);

                if (errorCode == ffmpeg.AVERROR_EOF)
                {
                    break;
                }
                else
                {
                    ExitOnFfmpegError(errorCode, $"error while trying to read packet from input file");
                }

                if (avPacket.stream_index >= pAvFormatInputFileContext->nb_streams)
                {
                    Console.WriteLine(
                        $"Warning: found new stream #{avPacket.stream_index} in input file. It will be ignored.");
                    continue;
                }

                if (avPacket.flags.HasFlag(ffmpeg.AV_PKT_FLAG_CORRUPT))
                {
                    Console.WriteLine(
                        $"Warning: corrupt packet found in stream #{avPacket.stream_index}.");
                }

                var pAvInputStream = pAvFormatInputFileContext->streams[avPacket.stream_index];
                var pAvOutputStream = pAvFormatOutputFileContext->streams[avPacket.stream_index];

                avPacket.pts = ffmpeg.av_rescale_q_rnd(avPacket.pts, pAvInputStream->time_base,
                    pAvOutputStream->time_base, AVRounding.AV_ROUND_NEAR_INF | AVRounding.AV_ROUND_PASS_MINMAX);
                avPacket.dts = ffmpeg.av_rescale_q_rnd(avPacket.dts, pAvInputStream->time_base,
                    pAvOutputStream->time_base, AVRounding.AV_ROUND_NEAR_INF | AVRounding.AV_ROUND_PASS_MINMAX);
                avPacket.duration =
                    ffmpeg.av_rescale_q(avPacket.duration, pAvInputStream->time_base, pAvOutputStream->time_base);
                avPacket.pos = -1;

                ffmpeg.av_write_frame(pAvFormatOutputFileContext, &avPacket);
            }

            ffmpeg.av_packet_unref(&avPacket);
        }
    }
}
