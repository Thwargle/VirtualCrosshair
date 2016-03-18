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
using WindowPlacementUtil;

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
        protected class MonitorChoice
        {
            public string MonitorName { get; set; }
            public override string ToString()
            {
                return MonitorName;
            }
        }
        protected List<CrosshairImage> ImageNames = new List<CrosshairImage>();
        protected List<MonitorChoice> MonitorNames = new List<MonitorChoice>();
        public delegate void SettingsChangedEventHandler(CrosshairSettingsModel setting, bool monitorChanged);
        public event SettingsChangedEventHandler SettingsChanged;
        private readonly CrosshairSettingsModel _settingsModel;
        public CrosshairSettingsWindow(CrosshairSettingsModel model)
        {
            _settingsModel = model;
            InitializeComponent();
            LoadImageChoices();
            LoadMonitorChoices();

            LoadDefaultValues();

            Assembly assembly = Assembly.GetExecutingAssembly();
            this.Title = string.Format("Virtual Crosshair v{0} Settings", assembly.GetName().Version);
            FireSettingsChangedEvent(monitorChanged: true);
        }
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            LoadWindowSettings();
        }
        private void LoadWindowSettings()
        {
            this.SetPlacement(Properties.Settings.Default.CrosshairSettingsWindowPlacement);
        }
        private void SaveWindowSettings()
        {
            Properties.Settings.Default.CrosshairSettingsWindowPlacement = this.GetPlacement();
            Properties.Settings.Default.Save();
        }

        private void LoadDefaultValues()
        {
            Properties.Settings.Default.Reload();
            int hOffset = Properties.Settings.Default.HorizontalOffset;
            this.HorizontalOffsetCtl.Value = hOffset;
            // Find lowest horizontal scale holding current offset
            foreach (ComboBoxItem item in cboHorizScale.Items)
            {
                int value = int.Parse(item.Content.ToString());
                if (hOffset >= -value && hOffset <= value)
                {
                    cboHorizScale.SelectedItem = item;
                    break;
                }
            }
            int vOffset = Properties.Settings.Default.VerticalOffset;
            this.VerticalOffsetCtl.Value = vOffset;
            // Find lowest vertical scale holding current offset
            foreach (ComboBoxItem item in cboVertScale.Items)
            {
                int value = int.Parse(item.Content.ToString());
                if (vOffset >= -value && vOffset <= value)
                {
                    cboVertScale.SelectedItem = item;
                    break;
                }
            }
            this.SizeCtl.Value = Properties.Settings.Default.Scaling;
            this.ImageChoiceCtl.SelectedIndex = Properties.Settings.Default.SelectedImageIndex;
            this.MonitorChoiceCtl.SelectedIndex = Properties.Settings.Default.SelectedMonitorIndex;
        }
        private void BindImages()
        {
            var binding = new Binding("Image");
            binding.Source =  Properties.Settings.Default;
            binding.Mode = BindingMode.TwoWay;
            BindingOperations.SetBinding(this.ImageChoiceCtl, ComboBox.SelectedValueProperty, binding);
        }
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
        public void DisplayWorkArea(string info)
        {
            this.lblWorkAreaInfo.Content = info;
        }
        private void LoadMonitorChoices()
        {
            var names = GetMonitorNames();
            foreach (string name in names)
            {
                this.MonitorNames.Add(
                    new MonitorChoice() { MonitorName = name });
            }
            this.MonitorChoiceCtl.ItemsSource = this.MonitorNames;
        }
        private List<string> GetImageNames()
        {
            var imageNames = new List<string>();
            string imagePath = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName) + @"\Images\";
            foreach (string filepath in System.IO.Directory.EnumerateFiles(imagePath + ""))
            {
                string filename = System.IO.Path.GetFileName(filepath);
                imageNames.Add(filename);
            }
            return imageNames;
        }
        private List<string> GetMonitorNames()
        {
            var names = new List<string>();
            for (int i = 0; i < System.Windows.Forms.Screen.AllScreens.Length; ++i)
            {
                names.Add(string.Format("{0}", i + 1));
            }
            return names;
        }

        private void HorizontalOffsetCtl_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (e.NewValue == e.OldValue) { return; }
            _settingsModel.HorizontalOffset = (int)e.NewValue;
            if (txtHorizontal != null)
            {
                txtHorizontal.Text = HorizontalOffsetCtl.Value.ToString();
            }
            FireSettingsChangedEvent();
            e.Handled = true;
        }
        private void VerticalOffsetCtl_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (e.NewValue == e.OldValue) { return; }
            _settingsModel.VerticalOffset = (int)e.NewValue;
            txtVertical.Text = VerticalOffsetCtl.Value.ToString();
            FireSettingsChangedEvent();
            e.Handled = true;
        }

        private void SizeCtl_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (e.NewValue == e.OldValue) { return; }
            _settingsModel.Scaling = (40.0 + e.NewValue) / 40.0;
            txtSize.Text = SizeCtl.Value.ToString();
            FireSettingsChangedEvent();
            e.Handled = true;
        }

        private void ImageChoiceCtl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string newImageName = this.ImageChoiceCtl.SelectedValue.ToString();
            if (newImageName == _settingsModel.ImageName) { return; }
            this._settingsModel.ImageName = newImageName;
            FireSettingsChangedEvent();
        }

        private void MonitorChoiceCtl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string newMonitorIndexString = this.MonitorChoiceCtl.SelectedValue.ToString();
            int newMonitorIndex = int.Parse(newMonitorIndexString) - 1;
            if (newMonitorIndex == _settingsModel.MonitorIndex) { return; }
            this._settingsModel.MonitorIndex = newMonitorIndex;
            FireSettingsChangedEvent(monitorChanged: true);
        }

        private void cboHorizScale_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            double offset = HorizontalOffsetCtl.Value;
            ComboBoxItem item = (cboHorizScale.SelectedItem as ComboBoxItem);
            int scale = int.Parse(item.Content.ToString());
            HorizontalOffsetCtl.Minimum = -scale;
            HorizontalOffsetCtl.Maximum = scale;
        }

        private void cboVertScale_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            double offset = VerticalOffsetCtl.Value;
            ComboBoxItem item = (cboVertScale.SelectedItem as ComboBoxItem);
            int scale = int.Parse(item.Content.ToString());
            VerticalOffsetCtl.Minimum = -scale;
            VerticalOffsetCtl.Maximum = scale;
        }

        private void FireSettingsChangedEvent(bool monitorChanged = false)
        {
            if (SettingsChanged != null)
            {
                SettingsChanged(_settingsModel, monitorChanged);
            }
            Properties.Settings.Default.Save();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveWindowSettings();
            Application.Current.Shutdown();
        }
    }
}
