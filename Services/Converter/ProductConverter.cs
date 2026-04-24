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
            JsonDocument document = JsonDocument.ParseValue(ref reader);
            string? type = document.RootElement.GetProperty("ProductType").GetString();
            string json = document.RootElement.GetRawText();
            
            return type switch
            {
                "Medicina" => JsonSerializer.Deserialize<Medicine>(json, options),
                "Suplemento" => JsonSerializer.Deserialize<Supplement>(json, options),
                "Cosmetico" => JsonSerializer.Deserialize<Cosmetic>(json, options),
                "Suministro" => JsonSerializer.Deserialize<Supply>(json, options),
                _ => throw new JsonException($"Tipo de producto desconocido: {type}")
            };
        }

        public override void Write(Utf8JsonWriter writer, Product value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, options);
        }
    }
}