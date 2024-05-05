using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace SwitchBotRemoteController.Extensions
{
    public static class JsonExtensions
    {
        public static readonly JsonSerializerOptions? DefaultSerializerSettings = new()
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            WriteIndented = true
        };

        public static string JsonFormatting(this string o)
        {
            var jsonDocument = JsonDocument.Parse(o);

            return JsonSerializer.Serialize(jsonDocument, DefaultSerializerSettings);
        }

        public static string JsonSerialize<T>(this T o)
        {
            return JsonSerializer.Serialize<T>(o, DefaultSerializerSettings);
        }

        public static string JsonSerialize<T>(this T o, JsonSerializerOptions? options)
        {
            return JsonSerializer.Serialize<T>(o, options);
        }

        public static T? JsonDeserialize<T>(this string o)
        {
            return JsonSerializer.Deserialize<T>(o, DefaultSerializerSettings);
        }

        public static T? JsonDeserialize<T>(this string o, JsonSerializerOptions? options)
        {
            return JsonSerializer.Deserialize<T>(o, options);
        }
    }
}

