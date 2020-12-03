using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace ColorNameAssistant
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public class LVColorItem
        {
            public string SourceName { get; set; }
            public string Name { get; set; }
            public string HexCode { get; set; }
            public SolidColorBrush Color { get; set; }
            public JToken jToken { get; set; }
        }
        ObservableCollection<LVColorItem> listColorBestResults = new ObservableCollection<LVColorItem>();
        object _objLock = new object();

        CollectionViewSource viewSource = new CollectionViewSource();

        Task RCPTask = null;
        CancellationTokenSource RCPcancelTokenSource;
        CancellationToken RCPcancelToken;


        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Enable cross-thread collection change notification for the ObservableCollection
            BindingOperations.EnableCollectionSynchronization(listColorBestResults, _objLock);
            
            Helper.LoadFilesColors();

            viewSource.Source = listColorBestResults;
            // Sort the list of best color matches for SourceName
            viewSource.SortDescriptions.Add(new SortDescription("SourceName", ListSortDirection.Ascending));
            LViewColors.ItemsSource = viewSource.View;

            LoadSettings();
        }

        private void LoadSettings()
        {
            Helper.LoadSettingsFile();

            // Assume these settings as disabled (events are not fired in window loading when unchecked)
            MICompareColorPreview_Unchecked(MICompareColorPreview, null);
            BtnShowDetails_Unchecked(BtnShowDetails, null);

            // Set the settings
            BtnShowDetails.IsChecked = Common.settingsData.GetValue("show_details", false);
            MIAutoSelectBestColor.IsChecked = Common.settingsData.GetValue("autoselect_best_color", true);
            MICompareColorPreview.IsChecked = Common.settingsData.GetValue("compare_color_preview", false);

            string metricType = Common.settingsData.GetValue("distance_metric_type", "CIE2000");
            foreach (MenuItem item in MIComputeMetricDist.Items)
            {
                if (item.Tag.ToString() == metricType)
                {
                    item.IsChecked = true;
                    break;
                }
            }
        }

        private void SaveSettings()
        {
            Common.settingsData["show_details"] = BtnShowDetails.IsChecked;
            Common.settingsData["autoselect_best_color"] = MIAutoSelectBestColor.IsChecked;
            Common.settingsData["compare_color_preview"] = MICompareColorPreview.IsChecked;
            foreach (MenuItem item in MIComputeMetricDist.Items)
            {
                if (item.IsChecked)
                {
                    Common.settingsData["distance_metric_type"] = item.Tag.ToString();
                    break;
                }
            }

            Helper.SaveSettingsFile();
        }

        private void BtnPickColorRealTime_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (RCPTask != null)
                {
                    RCPcancelTokenSource.Cancel();
                    RCPTask.Wait();
                    RCPTask = null;
                }
                RCPcancelTokenSource = new CancellationTokenSource();
                RCPcancelToken = RCPcancelTokenSource.Token;
                RCPTask = new Task(async () => await RealTimeColorPick(null, RCPcancelToken));
                RCPTask.Start();
            }
            catch (Exception exc)
            {
                MessageBox.Show(this, exc.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnPickColorRealTime_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                RCPcancelTokenSource.Cancel();
                RCPTask.Wait();
                RCPTask = null;
            }
            catch (Exception exc)
            {
                MessageBox.Show(this, exc.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task RealTimeColorPick(string arg, CancellationToken token)
        {
            while (token.IsCancellationRequested == false)
            {
                string hexCode = ApiHelper.GetHexColorFromCursorPosition();
                bool updateUI = false;
                lock (_objLock)
                {
                    updateUI = Common.chosenColorHex != hexCode;
                    Common.chosenColorHex = hexCode;
                }
                if (updateUI)
                {
                    Dispatcher.Invoke(() =>
                    {
                        SetChosenColorInfo();
                    });
                }
                await Task.Delay(80);  // Prevents problems with other GUI operations due to a too fast loop
            }
        }

        private void BtnPickColor_Click(object sender, RoutedEventArgs e)
        {
            if (BtnPickColorRealTime.IsChecked == true)
                BtnPickColorRealTime.IsChecked = false;

            this.WindowState = WindowState.Minimized;
            // Wait a bit that system animation is completed
            Thread.Sleep(300);
            WindowColorPick wnd = new WindowColorPick();
            wnd.Owner = this;
            if (wnd.ShowDialog() == true)
            {
                Common.chosenColorHex = wnd.chosenColor.ToHex();
                SetChosenColorInfo();
            }
            this.WindowState = WindowState.Normal;
        }

        private void TBoxChosenColorHexCode_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case System.Windows.Input.Key.Enter:
                    try
                    {
                        if (TBoxChosenColorHexCode.Text.Contains("#") == false)
                            throw new Exception();
                        new BrushConverter().ConvertFromString(TBoxChosenColorHexCode.Text);
                        Common.chosenColorHex = TBoxChosenColorHexCode.Text.ToUpper();
                        SetChosenColorInfo();
                        TBoxChosenColorHexCode.CaretIndex = TBoxChosenColorRGBCode.Text.Length;
                    }
                    catch (Exception)
                    {
                        MessageBox.Show(this, "The hex color code is not valid.", this.Title,  MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                    break;
                default:
                    break;
            }
        }

        private void TBoxChosenColorRGBCode_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case System.Windows.Input.Key.Enter:
                    try
                    {
                        if (TBoxChosenColorRGBCode.Text.Count(txt => txt == ',') != 2)
                            throw new Exception();
                        string[] rgb = TBoxChosenColorRGBCode.Text.Split(",");
                        int r = int.Parse(rgb[0]);
                        int g = int.Parse(rgb[1]);
                        int b = int.Parse(rgb[2]);
                        string colorHex = Helper.ColorRGBToHex(r, g, b);
                        if (Helper.IsValidHexColorCode(colorHex) == false)
                            throw new Exception();
                        Common.chosenColorHex = colorHex.ToUpper();
                        SetChosenColorInfo();
                        TBoxChosenColorRGBCode.CaretIndex = TBoxChosenColorRGBCode.Text.Length;
                    }
                    catch (Exception)
                    {
                        MessageBox.Show(this, "The RGB value is not valid.", this.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                    break;
                default:
                    break;
            }
        }

        private void SetChosenColorInfo()
        {
            Common.chosenColorBrush = (SolidColorBrush)new BrushConverter().ConvertFromString(Common.chosenColorHex);
            
            // Set chosen color info
            TBoxChosenColorHexCode.Text = Common.chosenColorHex;
            TBoxChosenColorRGBCode.Text = Helper.ColorHexToRGB(Common.chosenColorHex);

            // Get the best colors that match the chosen color
            GetBestColorMatches();

            UpdatePreview();
        }

        /// <summary>
        /// Gets the best matching colors through all color lists available
        /// </summary>
        private void GetBestColorMatches()
        {
            int lViewCurrentSelectedIndex = LViewColors.SelectedIndex;

            listColorBestResults.Clear();

            object _objLock = new object();

            // Execute computing operations over all lists in parallel to speedup the operation
            Parallel.ForEach(Common.colorsLists, (jColorList) =>
            {
                JToken jTokenColor = FindColor.GetNearestColor(Common.chosenColorHex, jColorList);

                LVColorItem lVColorItem = new LVColorItem
                {
                    SourceName = jColorList["Info"]["source"].ToString(),
                    Name = jTokenColor["name"].ToString(),
                    HexCode = jTokenColor["hex"].ToString(),
                    Color = (SolidColorBrush)new BrushConverter().ConvertFromString(jTokenColor["hex"].ToString()),
                    jToken = jTokenColor
                };

                lock (_objLock)  // Prevent access to the variable from multiple thread at same time
                {
                    lVColorItem.Color.Freeze();  // Make color variable unmodifieable, avoid thread issues
                    listColorBestResults.Add(lVColorItem);
                }
            });

            LVColorItem itemBest = FindBestSimilarColor();

            if (itemBest != null)
            {
                // TODO: Seem (not sure) that the SortDescription added to the CollectionViewSource causes
                //   a delay in the passage of items in the ListView.Items probably asynchronously,
                //   then the ListView.Items are populated late and this cause the failure of ListView.SelectedIndex
                //   i have not found a good solution to this problem,
                //   force the Refresh is a ugly workaround fortunately we have few items only
                viewSource.View.Refresh();

                // Auto-select the best result on the ListView
                if (listColorBestResults.Count > 0)
                {
                    if (MIAutoSelectBestColor.IsChecked == true)
                    {
                        LViewColors.SelectedItem = itemBest;
                    }
                    else
                    {
                        if (lViewCurrentSelectedIndex == -1)
                            LViewColors.SelectedIndex = 0;
                        else
                        {
                            LViewColors.SelectedIndex = lViewCurrentSelectedIndex;
                        }

                    }
                }

                // Report info to the left grid

                TBoxApproxColorName.Text = itemBest.jToken["name"].ToString();
                // Try get the best hue name from results (find the lower distance value with hue name)
                var itemBestHue = listColorBestResults.OrderBy(o => o.jToken["distance"].ToObject<double>()).FirstOrDefault(f => (f.jToken as JObject).ContainsKey("hueName"));
                if (itemBestHue != null)
                    TBoxApproxHueName.Text = itemBestHue.jToken["hueName"].ToString();
                else
                    TBoxApproxHueName.Text = "";
            }
            else
            {
                TBoxApproxColorName.Text = "";
                TBoxApproxHueName.Text = "";
            }
        }

        private LVColorItem FindBestSimilarColor()
        {
            if (listColorBestResults.Count == 0)
                return null;

            // Find the lower distance value
            var itemBest = listColorBestResults.OrderBy(o => o.jToken["distance"].ToObject<double>()).First();
            var lowerDistance = itemBest.jToken["distance"].ToObject<double>();

            // Check for similar items with same distance
            var similarItems = listColorBestResults.Where(o => o.jToken["distance"].ToObject<double>() == lowerDistance);
            // Try get the similar item with the most common hex code
            var itemSimilar = similarItems.GroupBy(g => g.HexCode.ToLower()).OrderByDescending(o => o.Count())
                   .SelectMany(x => x).FirstOrDefault();
            if (itemSimilar != null)
                itemBest = itemSimilar;

            // Check if there are more results with same hex, and prefer the item result with Hue info
            if (listColorBestResults.Count(o => string.Compare(o.HexCode, itemBest.HexCode, true) == 0) > 1)
            {
                var itemWithHue = listColorBestResults.FirstOrDefault(o => (o.jToken as JObject).ContainsKey("hueName") && string.Compare(o.HexCode, itemBest.HexCode, true) == 0);
                if (itemWithHue != null)
                    itemBest = itemWithHue;
            }
            return itemBest;
        }


        private void UpdatePreview()
        {
            if (Common.chosenColorBrush == null)
                return;

            SolidColorBrush colorLeft = Common.chosenColorBrush;
            SolidColorBrush colorRight = colorLeft;
            if (LViewColors.SelectedItem != null)
                colorRight = (LViewColors.SelectedItem as LVColorItem).Color;

            if (MICompareColorPreview.IsChecked == false)
                colorRight = colorLeft;

            // Set preview on black background
            RectSelColorPrevBgBlack_Left.Fill = colorLeft;
            RectSelColorPrevBgBlack_Right.Fill = colorRight;

            // Set preview on white background
            RectSelColorPrevBgWhite_Left.Fill = colorLeft;
            RectSelColorPrevBgWhite_Right.Fill = colorRight;
        }

        private void LViewColors_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView LViewObj = (ListView)sender;
            if (LViewObj.SelectedIndex == -1)
                return;
            LoadColorInfo(LViewObj.SelectedItem as LVColorItem);
        }

        private void LoadColorInfo(LVColorItem lVColorItem)
        {
            JObject jTokenColor = (JObject)lVColorItem.jToken;

            UpdatePreview();

            // Set main color info
            TBoxColorName.Text = jTokenColor["name"].ToString();
            TBoxColorRGBCode.Text = jTokenColor["rgb"].ToString();
            TBoxColorHexCode.Text = jTokenColor["hex"].ToString();

            TBoxColorDistance.Text = jTokenColor["distance"].ToString();

            // Set Hue info
            if (jTokenColor.ContainsKey("hueName"))
                TBoxHueColorName.Text = jTokenColor["hueName"].ToString();
            else
                TBoxHueColorName.Text = "";
        }

        private void MIMetricDist_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)sender;
            foreach (MenuItem item in MIComputeMetricDist.Items)
            {
                item.IsChecked = item.Tag.ToString() == menuItem.Tag.ToString();
            }

            switch (menuItem.Tag.ToString())
            {
                case "CIE76":
                    Common.spaceComparison = new ColorMine.ColorSpaces.Comparisons.Cie1976Comparison();
                    break;
                case "CMC84":
                    Common.spaceComparison = new ColorMine.ColorSpaces.Comparisons.CmcComparison();
                    break;
                case "CIE94":
                    Common.spaceComparison = new ColorMine.ColorSpaces.Comparisons.Cie94Comparison();
                    break;
                default:
                    Common.spaceComparison = new ColorMine.ColorSpaces.Comparisons.CieDe2000Comparison();
                    break;
            }
        }

        private void MIClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnFindEncycolorpedia_Click(object sender, RoutedEventArgs e)
        {
            if (Common.chosenColorHex == null || Common.chosenColorHex.Length < 7)
                return;
            string url;
            // Encycolorpedia website languages
            string[] langAvailables = new string[] { "it", "cn", "es", "in", "pt", "jp", "ru", "de", "kr", "fr", "pl", "nl", "se", "id", "gr", "ir", "ro" };
            // Get system language
            var ci = System.Globalization.CultureInfo.InstalledUICulture;
            string sysLang = ci.TwoLetterISOLanguageName;

            if (langAvailables.Contains(sysLang))
            {
                url = string.Format("https://encycolorpedia.{0}/{1}", sysLang, Common.chosenColorHex.Replace("#", ""));
            }
            else
            {
                url = string.Format("https://encycolorpedia.com/{0}", Common.chosenColorHex.Replace("#", ""));
            }
            try
            {
                // Use shell due to https://github.com/dotnet/corefx/issues/10361
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception exc)
            {
                MessageBox.Show(this, exc.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MIAbout_Click(object sender, RoutedEventArgs e)
        {
            Window wnd = new WindowAbout();
            wnd.Owner = this;
            wnd.ShowDialog();
        }

        private void MICompareColorPreview_Click(object sender, RoutedEventArgs e)
        {
            UpdatePreview();
        }

        private void MICompareColorPreview_Checked(object sender, RoutedEventArgs e)
        {
            LineSeparatorBlack.Visibility = Visibility.Visible;
            LineSeparatorWhite.Visibility = Visibility.Visible;
        }

        private void MICompareColorPreview_Unchecked(object sender, RoutedEventArgs e)
        {
            LineSeparatorBlack.Visibility = Visibility.Hidden;
            LineSeparatorWhite.Visibility = Visibility.Hidden;
        }

        private void BtnShowDetails_Checked(object sender, RoutedEventArgs e)
        {
            GridDetails.Visibility = Visibility.Visible;
        }

        private void BtnShowDetails_Unchecked(object sender, RoutedEventArgs e)
        {
            GridDetails.Visibility = Visibility.Collapsed;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                if (RCPTask != null)
                {
                    RCPcancelTokenSource.Cancel();
                    RCPTask.Wait();
                    RCPTask = null;
                }
            }
            catch (Exception)
            {
            }
            SaveSettings();
        }

    }
}
