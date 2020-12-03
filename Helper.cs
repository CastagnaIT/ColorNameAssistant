using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace ColorNameAssistant
{
    public static class Helper
    {
        private static string settingsFilePath = Path.Combine(Directory.GetCurrentDirectory(), "settings.json");

        public static void LoadSettingsFile()
        {
            try
            {
                if (File.Exists(settingsFilePath) == true)
                    Common.settingsData = JObject.Parse(File.ReadAllText(settingsFilePath));
            }
            catch (Exception exc)
            {
                Debug.WriteLine("Error loading file: " + settingsFilePath);
                Debug.WriteLine("Error message:" + exc.Message);
            }
        }

        public static void SaveSettingsFile()
        {
            try
            {
                File.WriteAllText(settingsFilePath, Common.settingsData.ToString());
            }
            catch (Exception exc)
            {
                string msg = string.Format(
                       "Error saving file:\r\n{0}\r\n\r\nError message:\r\n{1}",
                       settingsFilePath,
                       exc.Message);

                MessageBox.Show(msg, "SaveSettingsFile error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void LoadFilesColors()
        {
            string[] colorFilesPaths = Directory.GetFiles(Directory.GetCurrentDirectory(), "Colors_*.json", SearchOption.AllDirectories);

            object _objLock = new object();

            // Load all files lists in parallel to speedup the operation
            Parallel.ForEach(colorFilesPaths, (filePath) =>
            {
                try
                {
                    // Load the json file
                    JObject jsonFile = JObject.Parse(LoadFileContent(filePath));

                    jsonFile["Info"]["fileName"] = Path.GetFileName(filePath);

                    lock (_objLock)  // Prevent access to the variable from multiple thread at same time
                    {
                        Common.colorsLists.Add(jsonFile);
                    }
                }
                catch (Exception exc)
                {
                    string msg = string.Format(
                        "Error reading file:\r\n{0}\r\n\r\nError message:\r\n{1}",
                        filePath,
                        exc.Message);

                    MessageBox.Show(msg, "LoadFilesColors error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
        }

        public static string LoadFileContent(string filePath)
        {
            return File.ReadAllText(filePath);
        }

        public static string ColorRGBToHex(int r, int g, int b)
        {
            return string.Format("#{0:X2}{1:X2}{2:X2}", r, g, b);
        }

        public static string ColorHexToRGB(string colorHex)
        {
            int red = int.Parse(colorHex.Substring(1, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
            int green = int.Parse(colorHex.Substring(3, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
            int blue = int.Parse(colorHex.Substring(5, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
            return red.ToString() + ", " + green.ToString() + ", " + blue.ToString();
        }

        public static bool IsValidHexColorCode(string colorHex)
        {
            // From http://stackoverflow.com/a/1636354/2343
            if (Regex.Match(colorHex, "^#(?:[0-9a-fA-F]{3}){1,2}$").Success)
                return true;
            return false;
        }

    }
}
