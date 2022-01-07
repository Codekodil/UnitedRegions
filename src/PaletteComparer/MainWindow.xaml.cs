using AssetExtractor;
using Microsoft.Win32;
using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Image = System.Windows.Controls.Image;

namespace PaletteComparer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private string _imageFile;
        private string _mapFile;
        private string[] _paletteFiles;
        private int _width => int.TryParse(Width.Text, out var i) ? i : 512;
        private int _height => int.TryParse(Height.Text, out var i) ? i : 256;

        private static BitmapImage Convert(Bitmap src)
        {
            MemoryStream ms = new MemoryStream();
            src.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            return image;
        }

        private void UpdateDisplay()
        {
            if (_imageFile == null) return;
            if (_paletteFiles == null) return;
            displays.Children.Clear();
            foreach (var p in _paletteFiles)
            {
                var bitmap = SpriteLoader.LoadBitmap(_imageFile, p, _mapFile, _width, _height, false);
                var panel = new StackPanel();
                panel.Children.Add(new Image
                {
                    Source = Convert(bitmap),
                    Width = Math.Max(512, _width)
                });
                panel.Children.Add(new Label
                {
                    Content = Path.GetFileName(p)
                });
                displays.Children.Add(panel);
            }
        }

        private void OpenImage(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image (*.RGCN)|*.RGCN;*bin|All files (*.*)|*.*"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                _imageFile = openFileDialog.FileName;
                imageLabel.Text = Path.GetFileName(_imageFile);
                UpdateDisplay();
            }
        }

        private void OpenPalettes(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Palette (*.RLCN)|*.RLCN|All files (*.*)|*.*",
                Multiselect = true
            };
            if (openFileDialog.ShowDialog() == true)
            {
                _paletteFiles = openFileDialog.FileNames;
                UpdateDisplay();
            }
        }

        private void OpenMap(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Sprite (*.RECN)|*.RECN;*bin|All files (*.*)|*.*"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                _mapFile = openFileDialog.FileName;
                mapLabel.Text = Path.GetFileName(_mapFile);
                UpdateDisplay();
            }
        }

        private void RemoveSprite(object sender, RoutedEventArgs e)
        {
            if (_mapFile == null) return;
            _mapFile = null;
            UpdateDisplay();
        }

        private void WidthHeightChanged(object sender, TextChangedEventArgs e)
        {
            UpdateDisplay();
        }
    }
}
