using System.Text.Json;
using System.Text.Json.Serialization;
using Farmacontrol.Model;
using Farmacontrol.Model.ProductEntity;

namespace Farmacontrol.Services.Converter
{
    public class ProductConverter : JsonConverter<Product>
    {
        public override Product? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using JsonDocument document = JsonDocument.ParseValue(ref reader);

            string? type = document.RootElement.GetProperty("ProductType").GetString();
            string json = document.RootElement.GetRawText();

            JsonSerializerOptions cleanOptions = CreateOptionsWithoutThisConverter(options);

            return type switch
            {
                "Medicina" => JsonSerializer.Deserialize<Medicine>(json, cleanOptions),
                "Suplemento" => JsonSerializer.Deserialize<Supplement>(json, cleanOptions),
                "Cosmetico" => JsonSerializer.Deserialize<Cosmetic>(json, cleanOptions),
                "Suministro" => JsonSerializer.Deserialize<Supply>(json, cleanOptions),
                _ => throw new JsonException($"Tipo de producto desconocido: {type}")
            };
        }

        public override void Write(Utf8JsonWriter writer, Product value, JsonSerializerOptions options)
        {
            JsonSerializerOptions cleanOptions = CreateOptionsWithoutThisConverter(options);

            switch (value)
            {
                case Medicine medicine:
                    JsonSerializer.Serialize(writer, medicine, cleanOptions);
                    break;

                case Supplement supplement:
                    JsonSerializer.Serialize(writer, supplement, cleanOptions);
                    break;

                case Cosmetic cosmetic:
                    JsonSerializer.Serialize(writer, cosmetic, cleanOptions);
                    break;

                case Supply supply:
                    JsonSerializer.Serialize(writer, supply, cleanOptions);
                    break;

                default:
                    throw new JsonException($"Tipo de producto desconocido: {value.GetType().Name}");
            }
        }

        private static JsonSerializerOptions CreateOptionsWithoutThisConverter(JsonSerializerOptions options)
        {
            JsonSerializerOptions cleanOptions = new(options);

            JsonConverter? converter = cleanOptions.Converters
                .FirstOrDefault(converter => converter is ProductConverter);

            if (converter != null)
                cleanOptions.Converters.Remove(converter);

            return cleanOptions;
        }
    }
}