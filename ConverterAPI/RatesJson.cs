using System.Text.Json.Serialization;

namespace ConverterAPI
{
    public class RatesJson
    {
        public DateTime Date { get; set; }

        [JsonPropertyName("Valute")]
        public Dictionary<string, Valute> Rates { get; set; } = [];
    }
}
