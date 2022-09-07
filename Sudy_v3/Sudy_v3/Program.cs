using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Audio;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sudy_v3;

await MainAsync();

async Task MainAsync()
{
    // Подключение зависимостей
    using var services = ConfigureServices();

    var config = services.GetRequiredService<ConfigurationBot>();

    DiscordSocketClient client = services.GetRequiredService<DiscordSocketClient>();

    if (config.Token == null) { Console.WriteLine("No configuration file found!"); throw new Exception(); }


    client.Log += Log;
    await client.LoginAsync(TokenType.Bot, config.Token);
    await client.StartAsync();

    await services.GetRequiredService<CommandHandlingService>().InitializeAsync();

    await Task.Delay(-1);

}


ServiceProvider ConfigureServices()
{
    return new ServiceCollection()
        .AddSingleton(new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json").Build()
            .GetSection(nameof(ConfigurationBot))
            .Get<ConfigurationBot>()
        )
        .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
        {
            MessageCacheSize = 500
        }))
        .AddSingleton(new CommandService(new CommandServiceConfig
        {
            LogLevel = LogSeverity.Info,
            DefaultRunMode = RunMode.Async,
            CaseSensitiveCommands = false
        }))
        .AddSingleton<CommandHandlingService>()
        .BuildServiceProvider();
}

Task Log(LogMessage msg)
{
    Console.WriteLine(msg.ToString());
    return Task.CompletedTask;
}