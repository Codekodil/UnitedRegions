using AssetExtractor;
using Gen4Assets;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using UnhedderEngine;
using Math = UnhedderEngine.Math;

namespace GuiEngine
{
    public partial class AssetLabel : UserControl
    {
        private static readonly Dictionary<string, (PropertyInfo, PropertyInfo)> _dynamicAssets;
        static AssetLabel()
        {
            _dynamicAssets = typeof(Assets)
                .GetProperties()
                .Where(p => typeof(AssetManager).IsAssignableFrom(p.PropertyType))
                .SelectMany(pManager => pManager
                    .PropertyType
                    .GetProperties()
                    .Where(p => p.PropertyType == typeof(AssetManager.FontAsset))
                    .Select(p => (pManager, p)))
                .ToDictionary(v => $"{v.pManager.Name}.{v.p.Name}");
        }
        [TypeConverter(typeof(FontAssetConverter))]
        public string AssetPath { get; set; }

        public AssetManager.FontAsset GetAsset(Assets assets) =>
            _dynamicAssets.TryGetValue(AssetPath, out var p) ?
            (AssetManager.FontAsset)p.Item2.GetValue(p.Item1.GetValue(assets)) :
            null;

        public Vec3 Color => new Vec3(ForeColor.R, ForeColor.G, ForeColor.B) / 255;

        public string TextContent
        {
            get => previewLabel.Text ?? "";
            set => previewLabel.Text = value;
        }
        public ContentAlignment TetxAlignment
        {
            get => previewLabel.TextAlign;
            set => previewLabel.TextAlign = value;
        }
        private int _lines = 1;
        public int Lines
        {
            get => _lines;
            set
            {
                _lines = Math.Max(1, value);
                AssetLabel_SizeChanged(this, null);
            }
        }


        public AssetLabel()
        {
            InitializeComponent();
            AssetLabel_SizeChanged(this, null);
        }

        private void AssetLabel_SizeChanged(object sender, EventArgs e)
        {
            previewLabel.Font = new Font(previewLabel.Font.FontFamily, Height * .4f / _lines);
        }





        public class FontAssetConverter : ExpandableObjectConverter
        {
            public FontAssetConverter()
            {
                standardValues = _dynamicAssets.Keys.ToArray();
            }
            private readonly string[] standardValues;

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context) => true;
            public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) => true;
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) =>
                new StandardValuesCollection(standardValues);
        }
    }
}
