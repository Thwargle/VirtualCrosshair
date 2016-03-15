using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Virtual_Crosshair
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class CrosshairSettingsWindow : Window
    {
        protected class CrosshairImage
        {
            public string Filename { get; set; }
            public override string ToString()
            {
                return Filename;
            }
        }
        protected List<CrosshairImage> ImageNames = new List<CrosshairImage>();
        public class CrosshairSettings
        {
            public int HorizontalOffset = 0;
            public int VerticalOffset = 0;
            public double Scaling = 1.0;
            public string ImageName;
        }
        public delegate void OffsetChangedEventHandler(CrosshairSettings setting);
        public event OffsetChangedEventHandler OffsetChanged;
        private CrosshairSettings _currentSettings = new CrosshairSettings();
        public CrosshairSettingsWindow(string originalImage)
        {
            InitializeComponent();
            LoadImageChoices();
            _currentSettings.ImageName = originalImage;

            Assembly assembly = Assembly.GetExecutingAssembly();
            this.Title = string.Format("Virtual Crosshair v{0} Settings", assembly.GetName().Version);
            FireChangeEvent();
        }
        public string GetCurrentImageName() { return _currentSettings.ImageName; }
        public double GetCurrentScaling() { return _currentSettings.Scaling; }
        public double GetHorizontalOffset() { return _currentSettings.HorizontalOffset; }
        public double GetVerticalOffset() { return _currentSettings.VerticalOffset; }

        private void LoadImageChoices()
        {
            var imageNames = GetImageNames();
            foreach (string imageName in imageNames)
            {
                this.ImageNames.Add(
                    new CrosshairImage() { Filename = imageName });
            }
            this.ImageChoiceCtl.ItemsSource = this.ImageNames;
            
        }
        private List<string> GetImageNames()
        {
            List<string> imageNames = new List<string>();
            string imagePath = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName) + @"\Images\";
            foreach (string filepath in System.IO.Directory.EnumerateFiles(imagePath + ""))
            {
                string filename = System.IO.Path.GetFileName(filepath);
                imageNames.Add(filename);
            }
            return imageNames;
        }

        private void HorizontalOffsetCtl_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (e.NewValue == e.OldValue) { return; }
            _currentSettings.HorizontalOffset = (int)e.NewValue;
            txtHorizontal.Text = HorizontalOffsetCtl.Value.ToString();
            FireChangeEvent();
            e.Handled = true;
        }
        private void VerticalOffsetCtl_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (e.NewValue == e.OldValue) { return; }
            _currentSettings.VerticalOffset = (int)e.NewValue;
            txtVertical.Text = VerticalOffsetCtl.Value.ToString();
            FireChangeEvent();
            e.Handled = true;
        }

        private void SizeCtl_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (e.NewValue == e.OldValue) { return; }
            _currentSettings.Scaling = (40.0 + e.NewValue) / 40.0;
            txtSize.Text = SizeCtl.Value.ToString();
            FireChangeEvent();
            e.Handled = true;
        }
        private void FireChangeEvent()
        {
            if (OffsetChanged != null)
            {
                OffsetChanged(_currentSettings);
            }
        }

        private void ImageChoiceCtl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this._currentSettings.ImageName = this.ImageChoiceCtl.SelectedValue.ToString();
            FireChangeEvent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
