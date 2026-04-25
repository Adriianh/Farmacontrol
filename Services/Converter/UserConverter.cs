using System.Text.Json;
using System.Text.Json.Serialization;
using Farmacontrol.Model;
using Farmacontrol.Model.UserEntity;

namespace Farmacontrol.Services.Converter
{
    public class UserConverter : JsonConverter<User>
    {
        public override User Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using JsonDocument document = JsonDocument.ParseValue(ref reader);

            JsonElement root = document.RootElement;

            string role = root.GetProperty("Role").GetString()
                          ?? throw new JsonException("El usuario no tiene rol.");

            string name = root.GetProperty("Name").GetString()
                          ?? throw new JsonException("El usuario no tiene nombre.");

            string username = root.GetProperty("Username").GetString()
                              ?? throw new JsonException("El usuario no tiene nombre de usuario.");

            string password = root.GetProperty("Password").GetString()
                              ?? throw new JsonException("El usuario no tiene contraseña.");

            User user = role switch
            {
                "Administrador" => new Administrator(name, username, string.Empty),
                "Empleado" => new Employee(name, username, string.Empty),
                _ => throw new JsonException($"Tipo de usuario desconocido: {role}")
            };

            user.Password = password;
            return user;
        }

        public override void Write(Utf8JsonWriter writer, User value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WriteString("Name", value.Name);
            writer.WriteString("Username", value.Username);
            writer.WriteString("Password", value.Password);
            writer.WriteString("Role", value.Role);

            writer.WriteEndObject();
        }
    }
}