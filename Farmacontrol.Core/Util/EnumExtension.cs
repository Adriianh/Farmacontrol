using System.ComponentModel;
using System.Reflection;

namespace Farmacontrol.Core.Util
{
    public static class EnumExtension
    {
        public static string GetDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            if (field == null) return "";
            var attribute = field.GetCustomAttribute<DescriptionAttribute>();
            
            return attribute == null ? value.ToString() : attribute.Description;

        }
    }
}