using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace ColorNameAssistant
{
    public static class Extensions
    {
        /// <summary>
        /// Get value of specified property type and fallback to default value when not exists
        /// </summary>
        /// <param name="jObject"></param>
        /// <param name="propertyName">Property name</param>
        /// <param name="defaultValue">Default value returned when the property not exists</param>
        /// <returns>T value</returns>
        public static T GetValue<T>(this JObject jObject, string propertyName, T defaultValue)
        {
            JToken value;
            if (jObject.TryGetValue(propertyName, out value) == true)
                return value.ToObject<T>();
            else
                return defaultValue;
        }

        /// <summary>
        /// Convert the color to hex value (no alpha channel)
        /// </summary>
        /// <param name="color"></param>
        /// <returns>Hex value</returns>
        public static string ToHex(this Color color)
        {
            return string.Format("#{0:X2}{1:X2}{2:X2}", color.R, color.G, color.B);
        }
    }
}
