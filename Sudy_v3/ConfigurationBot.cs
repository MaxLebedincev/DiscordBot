public class ConfigurationBot
{
    public string? Token { get; set; }
    public string? Prefix { get; set; }
    public string? Currently { get; set; }

    public string? StatusText { get; set; }

    public string? Status { get; set; }

    public Dictionary<string, string>? UserMusic { get; set; }

    public List<string>? IgnorGuild { get; set; }

    public Storage LocalStorage { get; set; } = new Storage();

    public class Storage 
    {
        public string? Main { get; set; }
        public string? Music { get; set; }
        public string? Youtube { get; set; }
        public string? SlashCommand { get; set; }
    }
}