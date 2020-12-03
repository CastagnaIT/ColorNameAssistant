using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Media;

namespace ColorNameAssistant
{
    public static class Common
    {
        public static string chosenColorHex = null;
        public static SolidColorBrush chosenColorBrush = null;

        public static List<JObject> colorsLists = new List<JObject>();

        public static ColorMine.ColorSpaces.Comparisons.IColorSpaceComparison spaceComparison = new ColorMine.ColorSpaces.Comparisons.CieDe2000Comparison();

        public static JObject settingsData = new JObject();
    }
}
