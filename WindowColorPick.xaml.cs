using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ColorNameAssistant
{
    /// <summary>
    /// Logica di interazione per WindowColorPick.xaml
    /// </summary>
    public partial class WindowColorPick : Window
    {
        BitmapSource BSSceenshot;
        public System.Windows.Media.Color chosenColor = System.Windows.Media.Colors.Black;

        public WindowColorPick()
        {
            InitializeComponent();

            // Determine the size of the "virtual screen" which includes all monitors
            int screenLeft = (int)SystemParameters.VirtualScreenLeft;
            int screenTop = (int)SystemParameters.VirtualScreenTop;
            int screenWidth = (int)SystemParameters.VirtualScreenWidth;
            int screenHeight = (int)SystemParameters.VirtualScreenHeight;
            // Expand the window to all monitors
            this.Top = 0;
            this.Left = 0;
            this.Width = screenWidth;
            this.Height = screenHeight;
            // Take a screenshot of all monitors
            using (Bitmap bmp = new Bitmap(screenWidth, screenHeight))
            {
                // Draw the screenshot into our bitmap.
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(screenLeft, screenTop, 0, 0, bmp.Size);
                }
                // Convert Bitmap to ImageBrush and set as background
                BSSceenshot = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    bmp.GetHbitmap(),
                    IntPtr.Zero,
                    System.Windows.Int32Rect.Empty,
                    BitmapSizeOptions.FromWidthAndHeight(bmp.Width, bmp.Height));
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ApiHelper.HideWindowFromTaskSwitcher(this);
            GridMain.Background = new ImageBrush(BSSceenshot);
            
            // Set initial position/color of ellipse
            Window_MouseMove(sender, null);

            // Force the activation of the window, borderless window are not activated by default when opened,
            // you can see because if you comment this the KeyPress events will not work
            this.Activate();
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            //System.Windows.Point currentPos = Mouse.GetPosition(this);  // Mouse.GetPosition not allow get the position when the window is not loaded 
            System.Windows.Point currentPos = ApiHelper.GetCursorPosition();
            
            // Safe check to avoid crashing with wrong value
            if (currentPos.X >= 0 && currentPos.X <= BSSceenshot.PixelWidth &&
                currentPos.Y >= 0 && currentPos.Y <= BSSceenshot.PixelHeight)
            {
                double ellipseX = currentPos.X - (EllipseCursor.Width / 2);
                double ellipseY = currentPos.Y - (EllipseCursor.Height / 2);

                // Move the ellipse on cursor
                EllipseCursor.Margin = new Thickness() { Top = ellipseY, Left = ellipseX };

                // Get the corrent color under the mouse pointer
                byte[] pixels = new byte[4];
                BSSceenshot.CopyPixels(new Int32Rect((int)currentPos.X, (int)currentPos.Y, 1, 1), pixels, 4, 0);
                chosenColor = System.Windows.Media.Color.FromArgb(pixels[3], pixels[2], pixels[1], pixels[0]);
                // Set the color to the ellipse
                EllipseCursor.Stroke = new SolidColorBrush(chosenColor);
            }
        }

        private void GridMain_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DialogResult = true;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Space:
                case Key.Return:
                    this.DialogResult = true;
                    break;
                case Key.Escape:
                    this.Close();
                    break;
                default:
                    break;
            }
        }

    }
}
