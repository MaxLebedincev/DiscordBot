public class ConfigurationBot
{
    public string? Token { get; set; }
    public string? Prefix { get; set; }
    public string? Currently { get; set; }

    public string? StatusText { get; set; }

    public string? Status { get; set; }

    public Dictionary<string, string>? UserMusic { get; set; }

    public List<string>? IgnorGuild { get; set; }

}