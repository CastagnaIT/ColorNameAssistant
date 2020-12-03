using Newtonsoft.Json.Linq;
using System;
using System.Drawing;

namespace ColorNameAssistant
{
    public static class FindColor
    {
		/// <summary>
		/// Return best color match by computing the Euclidean sRGB metric distance (the first original method)
		/// </summary>
		public static JToken GetNearestColorSRGB(string colorHex, JObject jColorList)
		{
			int colorR = int.Parse(colorHex.Substring(1, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
			int colorG = int.Parse(colorHex.Substring(3, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
			int colorB = int.Parse(colorHex.Substring(5, 2), System.Globalization.NumberStyles.AllowHexSpecifier);

			double bestDistance = -1;
			JToken nearestColor = null;
			string nearestColorHex = null;

			JObject hueColorsList = null;
			if (jColorList["Info"]["hasHueValues"].ToObject<bool>() == true && jColorList.ContainsKey("HueColors"))
				hueColorsList = jColorList["HueColors"].ToObject<JObject>();

			foreach (var item in jColorList)
			{
				string itemColorHex = item.Key;
				if (string.Compare(item.Key, colorHex, true) == 0)
				{
					bestDistance = 0;
					nearestColor = item.Value;
					nearestColorHex = item.Key;
					break;
				}

				int nearestColorR = int.Parse(itemColorHex.Substring(1, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
				int nearestColorG = int.Parse(itemColorHex.Substring(3, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
				int nearestColorB = int.Parse(itemColorHex.Substring(5, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
				
				
				// Compute the Euclidean sRGB distance between the two colors (the alpha-component is not used)
				double powRed = Math.Pow(colorR - nearestColorR, 2.0);
				double powGreen = Math.Pow(colorG - nearestColorG, 2.0);
				double powBlue = Math.Pow(colorB - nearestColorB, 2.0);
				double deltaE = Math.Sqrt(powBlue + powGreen + powRed);

				if (bestDistance < 0 || bestDistance > deltaE)
				{
					bestDistance = deltaE;
					nearestColor = item.Value;
					nearestColorHex = item.Key;
				}
			}

			// Add color info
			if (nearestColor != null)
			{
				nearestColor["hex"] = nearestColorHex;
				int nearestColorR = int.Parse(nearestColorHex.Substring(1, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
				int nearestColorG = int.Parse(nearestColorHex.Substring(3, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
				int nearestColorB = int.Parse(nearestColorHex.Substring(5, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
				nearestColor["rgb"] = string.Format("{0}, {1}, {2}", nearestColorR, nearestColorG, nearestColorB);
				nearestColor["distance"] = bestDistance;

				// Find Hue color info
				if (hueColorsList != null)
				{
					string hueName = nearestColor["hueName"].ToString();
					if (hueColorsList.ContainsKey(hueName) == true)
						nearestColor["hueHex"] = hueColorsList[hueName]["hex"];
				}
			}
			return nearestColor;
		}

		/// <summary>
		/// Return best color match by computing the metric distance with the selected formula
		/// </summary>
		public static JToken GetNearestColor(string colorHex, JObject jColorList)
        {
			int colorR = int.Parse(colorHex.Substring(1, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
			int colorG = int.Parse(colorHex.Substring(3, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
			int colorB = int.Parse(colorHex.Substring(5, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
			var colorRgb = new ColorMine.ColorSpaces.Rgb { R = colorR, G = colorG, B = colorB };

			double bestDistance = -1;
			JToken nearestColor = null;
			string nearestColorHex = null;

			JObject hueColorsList = null;
			if (jColorList["Info"]["hasHueValues"].ToObject<bool>() == true && jColorList.ContainsKey("HueColors"))
				hueColorsList = jColorList["HueColors"].ToObject<JObject>();

			foreach (var item in jColorList["Colors"].ToObject<JObject>())
			{
				string itemColorHex = item.Key;
				if (string.Compare(item.Key, colorHex, true) == 0)
                {
					bestDistance = 0;
					nearestColor = item.Value;
					nearestColorHex = item.Key;
					break;
                }

				int nearestColorR = int.Parse(itemColorHex.Substring(1, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
				int nearestColorG = int.Parse(itemColorHex.Substring(3, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
				int nearestColorB = int.Parse(itemColorHex.Substring(5, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
				var itemRgb = new ColorMine.ColorSpaces.Rgb { R = nearestColorR, G = nearestColorG, B = nearestColorB };

				// Space comparison formulas: https://en.wikipedia.org/wiki/Color_difference
				// Note: Delta-e calculations are quasimetric,
				//   the result of comparing color a to b isn't always equal to comparing color b to a
				//   but it will probably be pretty close
				double deltaE = colorRgb.Compare(itemRgb, Common.spaceComparison);

				if (bestDistance < 0 || bestDistance > deltaE)
				{
					bestDistance = deltaE;
					nearestColor = item.Value;
					nearestColorHex = item.Key;
				}
			}

			// Add color info
			if (nearestColor != null)
            {
				nearestColor["hex"] = nearestColorHex;
				int nearestColorR = int.Parse(nearestColorHex.Substring(1, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
				int nearestColorG = int.Parse(nearestColorHex.Substring(3, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
				int nearestColorB = int.Parse(nearestColorHex.Substring(5, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
				nearestColor["rgb"] =  string.Format("{0}, {1}, {2}", nearestColorR, nearestColorG, nearestColorB);
				nearestColor["distance"] = bestDistance;

				// Find Hue color info
				if (hueColorsList != null)
				{
					string hueName = nearestColor["hueName"].ToString();
					if (hueColorsList.ContainsKey(hueName) == true)
						nearestColor["hueHex"] = hueColorsList[hueName]["hex"];
				}
			}
			return nearestColor;
		}

	}
}
