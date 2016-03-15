using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.IO;
using System.Drawing;

namespace Virtual_Crosshair
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CrosshairSettingsWindow _settingsWindow = null;
        private int _imageHeight;
        private int _imageWidth;
        private string originalImage;
        public MainWindow()
        {
            InitializeComponent();
            originalImage = Properties.Settings.Default.Image;
            _settingsWindow = new CrosshairSettingsWindow(originalImage);
            RedisplayCrosshair();
            this.WindowStyle = WindowStyle.None;
            this.Show();
            InitialDisplayCrosshairSettingsWindow();
        }
        private void InitialDisplayCrosshairSettingsWindow()
        {
            _settingsWindow.OffsetChanged += _settingsWindow_OffsetChanged;
            _settingsWindow.Show();
        }
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            WindowsServices.SetWindowExTransparent(hwnd);
        }
        void _settingsWindow_OffsetChanged(CrosshairSettingsWindow.CrosshairSettings setting)
        {
            RedisplayCrosshair();
        }
        private void RedisplayCrosshair()
        {
            SetCurrentImage();
            ScaleTransform();
            SetCurrentLocation();
        }
        private void ScaleTransform()
        {
            double scaling = _settingsWindow.GetCurrentScaling();
            imgCrosshair.Height = _imageHeight * scaling;
            imgCrosshair.Width = _imageWidth * scaling;
        }
        private void SetCurrentLocation()
        {
            Rectangle workingArea = GetMonitorWorkingArea();

            double horizontalOffset = _settingsWindow.GetHorizontalOffset();
            double verticalOffset = _settingsWindow.GetVerticalOffset();

            this.Left = workingArea.Left + horizontalOffset;
            this.Top = workingArea.Top - verticalOffset;
            this.Width = workingArea.Width;
            this.Height = workingArea.Height;
        }
        private void SetCurrentImage()
        {
            string imageName = _settingsWindow.GetCurrentImageName();

            string imageDirectory = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName) + @"\Images\";
            string imageFilepath = Path.Combine(imageDirectory, imageName);

            MemoryStream ms = new MemoryStream();
            using (FileStream fs = File.OpenRead(imageFilepath))
            {
                fs.CopyTo(ms);
            }
            Bitmap bitmap = new Bitmap(ms);
            _imageWidth = bitmap.Width;
            _imageHeight = bitmap.Height;

            Uri imageUri = new Uri(imageFilepath);
            var bitmapImage = new BitmapImage(imageUri);
            imgCrosshair.Source = bitmapImage;
        }

        protected static Stream GetResourceStreamUnused(string resourcePath)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            List<string> resourceNames = new List<string>(assembly.GetManifestResourceNames());

            resourcePath = resourcePath.Replace(@"/", ".");
            resourcePath = resourceNames.FirstOrDefault(r => r.Contains(resourcePath));

            if (resourcePath == null)
                throw new FileNotFoundException("Resource not found");

            return assembly.GetManifestResourceStream(resourcePath);
        }
        private Rectangle GetMonitorWorkingArea()
        {
            int monitor = GetMonitorIndex();
            System.Windows.Forms.Screen screen = System.Windows.Forms.Screen.AllScreens[monitor];
            return screen.WorkingArea;
        }
        /// <summary>
        ///  Return 0-based monitor from user config
        /// </summary>
        /// <returns></returns>
        private int GetMonitorIndex()
        {
            if (System.Windows.Forms.Screen.AllScreens.Length == 1) { return 0; }
            string monitorToUse = Properties.Settings.Default.Monitor;
            if (EqStr(monitorToUse, "Primary")) { monitorToUse = "1"; }
            if (EqStr(monitorToUse, "Secondary")) { monitorToUse = "2"; }
            if (EqStr(monitorToUse, "Tertiary")) { monitorToUse = "3"; }
            int chosen = 0;
            if (int.TryParse(monitorToUse, out chosen))
            {
                chosen = chosen - 1; // convert 1-based to 0-based
                if (chosen <= System.Windows.Forms.Screen.AllScreens.Length) { return chosen; }
            }
            // If fail to parse or monitor does not exist, just give back 0
            return 0;
        }
        private bool EqStr(string str1, string str2)
        {
            return (string.Compare(str1, str2, ignoreCase: true) == 0);
        }

    }
}
