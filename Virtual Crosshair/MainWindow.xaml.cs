﻿using System;
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
        private CrosshairSettingsModel _settingsModel = new CrosshairSettingsModel();
        CrosshairSettingsWindow _settingsWindow = null;
        private int _imageHeight;
        private int _imageWidth;
        public MainWindow()
        {
            InitializeComponent();
            MigrateSettingsIfNeeded();
            _settingsWindow = new CrosshairSettingsWindow(_settingsModel);
            RedisplayMainWindowForMonitorChange();
            RedisplayCrosshair();
            this.WindowStyle = WindowStyle.None;
            this.Show();
            InitialDisplayCrosshairSettingsWindow();
        }
        private void MigrateSettingsIfNeeded()
        {
            if (Properties.Settings.Default.NeedsUpgrade)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.NeedsUpgrade = false;
                Properties.Settings.Default.Save();
            }
        }
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            WindowsServices.SetWindowExTransparent(hwnd);
        }
        private void InitialDisplayCrosshairSettingsWindow()
        {
            _settingsWindow.SettingsChanged += SettingsWindowSettingsChanged;
            _settingsWindow.Show();
        }

        private void SettingsWindowSettingsChanged(CrosshairSettingsModel settings, bool monitorChanged)
        {
            if (monitorChanged)
            {
                RedisplayMainWindowForMonitorChange();
            }
            RedisplayCrosshair();
        }
        private void RedisplayMainWindowForMonitorChange()
        {
            Rectangle workingArea = GetMonitorWorkingArea();

            this.Left = workingArea.Left;
            this.Top = workingArea.Top;
            this.Width = workingArea.Width;
            this.Height = workingArea.Height;

            this.WindowState = System.Windows.WindowState.Normal;
            this.WindowState = System.Windows.WindowState.Maximized;

            var mainMatrix = GetDpiScaling(Application.Current.MainWindow);
            var settingsMatrix = GetDpiScaling(_settingsWindow);
            string info = string.Format("{0},{1}-{2}x{3}",
                    workingArea.Left, workingArea.Top, workingArea.Width, workingArea.Height)
                + string.Format(" mdpi:{0}x{1}, sdpi:{2}x{3}",
                    mainMatrix.M11, mainMatrix.M22, settingsMatrix.M11, settingsMatrix.M22);

            this._settingsWindow.DisplayWorkArea(info);
        }
        private void RedisplayCrosshair()
        {
            imgCrosshair.Visibility = System.Windows.Visibility.Hidden;
            SetCurrentImage();
            ScaleTransform();
            SetCurrentLocation();
            imgCrosshair.Visibility = System.Windows.Visibility.Visible;
        }
        private void ScaleTransform()
        {
            double scaling = _settingsModel.Scaling;
            imgCrosshair.Height = _imageHeight * scaling;
            imgCrosshair.Width = _imageWidth * scaling;
        }
        private void SetCurrentLocation()
        {
            double horizontalOffset = _settingsModel.HorizontalOffset;
            double verticalOffset = _settingsModel.VerticalOffset;


            double imgLeft = ((this.Width - imgCrosshair.Width) / 2.0) + horizontalOffset;
            Canvas.SetLeft(imgCrosshair, imgLeft);
            double imgTop = ((this.Height - imgCrosshair.Height) / 2.0) - verticalOffset;
            Canvas.SetTop(imgCrosshair, imgTop);
        }
        private Matrix GetDpiScaling(UIElement elent)
        {
            Matrix mtrix = new Matrix();
            PresentationSource source = PresentationSource.FromVisual(elent);
            if (source != null && source.CompositionTarget != null)
            {
                mtrix = source.CompositionTarget.TransformFromDevice;
            }
            return mtrix;
        }
        private void SetCurrentImage()

        {
            string imageName = _settingsModel.ImageName;
            if (string.IsNullOrEmpty(imageName)) { return; }

            string imageDirectory = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName) + @"\Images\";
            string imageFilepath = Path.Combine(imageDirectory, imageName);

            if (!File.Exists(imageFilepath)) { return; }
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
            int monitor = GetCurrentMonitorIndex();
            System.Windows.Forms.Screen screen = System.Windows.Forms.Screen.AllScreens[monitor];
            return screen.WorkingArea;
        }
        private int GetCurrentMonitorIndex()
        {
            int index = _settingsModel.MonitorIndex;
            if (index + 1 > System.Windows.Forms.Screen.AllScreens.Length)
            {
                index = 0;
            }
            return index;
        }
        private bool EqStr(string str1, string str2)
        {
            return (string.Compare(str1, str2, ignoreCase: true) == 0);
        }
    }
}
