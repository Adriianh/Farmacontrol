using System.Text.Json;
using System.Text.Json.Serialization;
using Farmacontrol.Model;
using Farmacontrol.Model.UserEntity;

namespace Farmacontrol.Services.Converter
{
    public class UserConverter : JsonConverter<User>
    {
        public override User? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            JsonDocument document =  JsonDocument.ParseValue(ref reader);
            string? role =  document.RootElement.GetProperty("Role").GetString();
            string json = document.RootElement.GetRawText();
            
            return role switch {
                "Administrador" => JsonSerializer.Deserialize<Administrator>(json, options),
                "Empleado" => JsonSerializer.Deserialize<Employee>(json, options),
                _ => throw new JsonException($"Tipo de usuario desconocido: {role}")
            };
        }

        public override void Write(Utf8JsonWriter writer, User value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, options);
        }
    }
}