using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ColorNameAssistant
{
    /// <summary>
    /// Logica di interazione per About.xaml
    /// </summary>
    public partial class WindowAbout : Window
    {
        public WindowAbout()
        {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            // Hide window icon, WPF does not allow...
            ApiHelper.RemoveWindowIcon(this);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LblVersion.Content = "Version " + Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        private void HLLinkGithub_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            try
            {
                // Use shell due to https://github.com/dotnet/corefx/issues/10361
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = e.Uri.OriginalString,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception)
            {
            }
        }

    }
}
