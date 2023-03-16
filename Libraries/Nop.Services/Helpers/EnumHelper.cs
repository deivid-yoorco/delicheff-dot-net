using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.Helpers
{
    public static class EnumHelper
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            var att = enumValue.GetType().GetMember(enumValue.ToString())
                           .First()
                           .GetCustomAttribute<DisplayAttribute>();
            if (att == null) return enumValue.ToString();
            return att.Name;
        }

        public static string GetDisplayName(Type EnumType, int value)
        {
            var nameValue = EnumType.GetEnumValues().GetValue(value).ToString();
            var displayAttribute = EnumType.GetMember(nameValue)
                           .First()
                           .GetCustomAttribute<DisplayAttribute>(); 
            if (displayAttribute != null)
            {
                return displayAttribute.Name;
            }
            return nameValue.ToString();
        }
    }
}
