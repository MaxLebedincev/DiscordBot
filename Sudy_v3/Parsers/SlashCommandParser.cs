using Discord;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Sudy_v3.Parsers
{
    internal class SlashCommandParser
    {
        private List<ApplicationCommandProperties> _appCommandProp = new();
        public SlashCommandParser(string path)
        {
            List<Property>? properties = null;
            
            try
            {
                var fullPath = @$"{AppDomain.CurrentDomain.BaseDirectory}{path}";

                properties = JsonSerializer.Deserialize<List<Property>>(File.ReadAllText(fullPath));
            }
            catch(Exception)
            {
                throw new Exception(); // TODO: Add exception class!
            }

            if (properties == null) throw new Exception(); // TODO: Add exception class!

            foreach (var property in properties)
            {
                SlashCommandBuilder temp = new SlashCommandBuilder()
                    .WithName(property.name)
                    .WithDescription(property.description);

                foreach (var option in property.options)
                {
                    temp.AddOption(
                        option.name,
                        option.type,
                        option.description,
                        option.required
                    );
                }

                _appCommandProp.Add(temp.Build());
            }
        }

        public ApplicationCommandProperties[] GetCommandProperties()
        {
            return _appCommandProp.ToArray();
        }

        private class Property
        {
            public string? name { get; set; }
            public string? description { get; set; }
            public List<Option> options { get; set; } = new();

            public class Option
            {
                public string? name { get; set; }
                [JsonConverter(typeof(JsonStringEnumConverter))]
                public ApplicationCommandOptionType type { get; set; }
                public string? description { get; set; }
                public bool required { get; set; }
            }
        }
    }
}
