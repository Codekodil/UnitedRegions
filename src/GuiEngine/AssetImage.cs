using AssetExtractor;
using Gen4Assets;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace GuiEngine
{
    public partial class AssetImage : UserControl
    {
        private static readonly Dictionary<string, (PropertyInfo, PropertyInfo)> _dynamicAssets;
        static AssetImage()
        {
            _dynamicAssets = typeof(Assets)
                .GetProperties()
                .Where(p => typeof(AssetManager).IsAssignableFrom(p.PropertyType))
                .SelectMany(pManager => pManager
                    .PropertyType
                    .GetProperties()
                    .Where(p => p.PropertyType == typeof(AssetManager.SpriteAsset))
                    .Select(p => (pManager, p)))
                .ToDictionary(v => $"{v.pManager.Name}.{v.p.Name}");

#if DEBUG
            try
            {
                var pdbContent = File.ReadAllText(typeof(AssetImage).Assembly.Location.Replace(".dll", ".pdb"));
                var csPath = $"{nameof(GuiEngine)}\\{nameof(AssetImage)}.cs";
                var index = pdbContent.IndexOf(csPath);
                pdbContent = pdbContent.Substring(0, index);
                index = pdbContent.LastIndexOf("\0");
                pdbContent = pdbContent.Substring(index + 1);
                var binDebugPath = Path.Combine(pdbContent, "PocketEngine", "bin", "Debug");
                RomLoader.BasePathOverride = binDebugPath;
            }
            catch (Exception) { }
            Assets = new Assets();
#endif
        }

#if DEBUG
        private static readonly Assets Assets;
#endif

        private string _assetPath;
        [TypeConverter(typeof(SpriteAssetConverter))]
        public string AssetPath
        {
            get => _assetPath;
            set
            {
                _assetPath = value;
#if DEBUG
                try
                {
                    var sprite = GetAsset(Assets);
                    mainPictureBox.Image = sprite.LoadBitmap();
                    mainPictureBox.Visible = true;
                }
                catch (Exception ex)
                {
                    debugLabel.Text += ex.Message + "\n" + ex.StackTrace + "\n";
                    debugLabel.Visible = true;
                }
#endif
            }
        }

        public AssetManager.SpriteAsset GetAsset(Assets assets) =>
            _dynamicAssets.TryGetValue(AssetPath, out var p) ?
            (AssetManager.SpriteAsset)p.Item2.GetValue(p.Item1.GetValue(assets)) :
            null;


        public AssetImage()
        {
            InitializeComponent();
        }

        public class SpriteAssetConverter : ExpandableObjectConverter
        {
            public SpriteAssetConverter()
            {
                standardValues = _dynamicAssets.Keys.ToArray();
            }
            private readonly string[] standardValues;

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context) => true;
            public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) => true;
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) =>
                new StandardValuesCollection(standardValues);
        }

        public class PictureBoxWithInterpolationMode : PictureBox
        {
            public InterpolationMode InterpolationMode { get; set; }

            protected override void OnPaint(PaintEventArgs paintEventArgs)
            {
                paintEventArgs.Graphics.InterpolationMode = InterpolationMode;
                paintEventArgs.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
                base.OnPaint(paintEventArgs);
            }
        }
    }
}
