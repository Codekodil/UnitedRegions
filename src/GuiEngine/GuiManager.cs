using AssetExtractor;
using Gen4Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UnhedderEngine;
using UnhedderEngine.Input;
using UnhedderEngine.Workflows.Core;
using Math = UnhedderEngine.Math;

namespace GuiEngine
{
    public class GuiManager<T> where T : UserControl
    {
        private static readonly List<(Type T, Form Window)> _openWindows = new List<(Type T, Form Window)>();
        public T Control { get; private set; }
        public Scene Scene { get; }
        public Assets Assets { get; }
        public GuiManager(Scene scene, Assets assets)
        {
            Scene = scene;
            Assets = assets;

            lock (_openWindows)
            {
                bool IsType((Type T, Form) window) => window.T == typeof(T);
                while (_openWindows.Count(IsType) >= 2)
                {
                    var firstWindowIndex = _openWindows.FindIndex(IsType);
                    var form = _openWindows[firstWindowIndex].Window;
                    _openWindows.RemoveAt(firstWindowIndex);
                    form.Invoke(new Action(() => form.Close()));
                }
            }

            Task.Run(() =>
            {
                try
                {
                    var form = new HiddenForm();
                    var control = Activator.CreateInstance<T>();
                    form.Controls.Add(control);
                    FindAssets(control);
                    form.Load += (s, e) =>
                    {
                        lock (_openWindows)
                            _openWindows.Add((typeof(T), form));
                        Control = control;
                        Scene.Update += OnUpdate;
                    };
                    form.ShowDialog();
                }
                catch (Exception ex)
                {
                    CoreLogger.Write(ex);
                }
            });
        }

        private class ImageCache
        {
            public AssetImage Control { get; set; }
            public List<Control> ParentChain { get; set; }
            public SingleRenderer renderer { get; set; }
        }
        private class FontCache
        {
            public AssetLabel Control { get; set; }
            public List<Control> ParentChain { get; set; }
            public SingleRenderer renderer { get; set; }
            public float Ratio { get; set; }
            public string Text { get; set; }
        }

        private readonly List<ImageCache> _imageAssets = new List<ImageCache>();
        private readonly List<FontCache> _fontAssets = new List<FontCache>();

        private void FindAssets(Control parentControl)
        {
            var order = 10000;
            findControls(parentControl, new List<Control>());
            void findControls(Control control, List<Control> chain)
            {
                switch (control)
                {
                    case AssetImage image:
                        var renderer = new SingleRenderer
                        {
                            Enabled = false,
                            Material = new Material(CustomShader.GuiShader),
                            Mesh = Mesh.ScreenSquare,
                            Order = order--
                        };
                        renderer.Material.SetAttribute("Albedo", image.GetAsset(Assets).Value);
                        Scene.Add(renderer);
                        _imageAssets.Add(new ImageCache { Control = image, ParentChain = chain, renderer = renderer });
                        break;
                    case AssetLabel label:
                        var rendererLabel = new SingleRenderer
                        {
                            Enabled = false,
                            Material = new Material(CustomShader.GuiFontShader),
                            Mesh = Mesh.ScreenSquare,
                            Order = order--
                        };
                        rendererLabel.Material.SetAttribute("Albedo", label.GetAsset(Assets).Value.Texture);
                        Scene.Add(rendererLabel);
                        _fontAssets.Add(new FontCache { Control = label, ParentChain = chain, renderer = rendererLabel });
                        break;
                    default:
                        chain = chain.ToList();
                        chain.Add(control);
                        foreach (var v in control.Controls.OfType<Control>())
                            findControls(v, chain);
                        break;
                }

            }
        }

        private void OnUpdate(FrameData frameData)
        {
            var pixelSize = 2f / Control.Size.Height * .5f;
            var sizeSum = Scene.Cameras.Aggregate(Vec3.Zero, (c, n) => c + n.AverageDisplaySize?.Xy1 ?? Vec3.Zero);
            var ratio = sizeSum.Z > 0 ? sizeSum.X / sizeSum.Y : 1;

            Control.BeginInvoke(new Action(() =>
            {
                Control.Size = new System.Drawing.Size(Math.RoundToInt(Control.Size.Height * ratio), Control.Size.Height);

                foreach (var image in _imageAssets)
                {
                    ConfigureRenderer(image.renderer, image.ParentChain, image.Control);
                    image.renderer.Enabled = image.Control.Visible;
                }
                foreach (var label in _fontAssets)
                {
                    ConfigureRenderer(label.renderer, label.ParentChain, label.Control);
                    var labelRatio = label.renderer.Scale.X / label.renderer.Scale.Y;
                    if (label.Text != label.Control.TextContent || label.Ratio != labelRatio)
                    {
                        label.Ratio = labelRatio;
                        label.renderer.Mesh = GenerateFontMesh(label.Text = label.Control.TextContent, label.Control, label.Control.GetAsset(Assets), .1f);
                    }
                    label.renderer.Material.SetAttribute("Color", label.Control.Color);
                    label.renderer.Enabled = label.Control.Visible;
                }
            }));

            void ConfigureRenderer(SingleRenderer renderer, List<Control> parentChain, Control control)
            {
                var size = new Vec2(control.Size.Width, control.Size.Height);
                var offset = new Vec2(control.Location.X, control.Location.Y) + size * .5f;
                foreach (var v in parentChain)
                    offset += new Vec2(v.Location.X, v.Location.Y);
                renderer.Position = (new Vec3(offset.X, Control.Size.Height - offset.Y, 0f) * pixelSize).Scaled(1 / ratio, 1, 1) * 2 - new Vec3(1, 1, 0);
                renderer.Scale = size.Xy0 * pixelSize;
            }
        }

        private Mesh GenerateFontMesh(string text, AssetLabel label, AssetManager.FontAsset font, float characterSpacing)
        {
            var fontValue = font.Value;
            var data = new List<float>();

            var ratio = label.Size.Width / Math.Max(1f, label.Size.Height);
            var maxWidth = ratio * label.Lines;

            {//Insert Newlines
                var stringBuilder = new StringBuilder(text);
                var splitPoint = -1;
                var lengthBuffer = 0f;
                for (var i = 0; i < stringBuilder.Length; i++)
                {
                    var c = stringBuilder[i];
                    if (c == '\n')
                    {
                        splitPoint = -1;
                        lengthBuffer = 0;
                        continue;
                    }
                    if (c == ' ')
                        splitPoint = i;
                    if (!fontValue.SymbolData.TryGetValue(c, out var symbol)) continue;
                    lengthBuffer += symbol.TotalWidth + characterSpacing;
                    if (lengthBuffer > maxWidth)
                    {
                        lengthBuffer = 0;
                        if (splitPoint < 0)
                            splitPoint = i;
                        if (stringBuilder[splitPoint] == ' ')
                            stringBuilder[splitPoint] = '\n';
                        else
                            stringBuilder.Insert(splitPoint, '\n');
                    }
                }
                text = stringBuilder.ToString();
            }

            var totalSymbols = 0;
            var position = 0f;
            var line = 0;
            for (var i = 0; i < text.Length; ++i)
            {
                var c = text[i];
                if (c == '\n')
                {
                    position = 0;
                    ++line;
                    continue;
                }
                if (!fontValue.SymbolData.TryGetValue(c, out var symbol)) continue;
                if (symbol.SymbolWidth > 0)
                {
                    data.AddRange(new Vec3(symbol.Offset + position, -1 - line));
                    data.AddRange(symbol.UvMin);
                    data.AddRange(Vec3.OneZ);
                    data.AddRange(Vec3.OneX);

                    data.AddRange(new Vec3(symbol.Offset + symbol.SymbolWidth + position, -1 - line));
                    data.AddRange(new Vec2(symbol.UvMax.X, symbol.UvMin.Y));
                    data.AddRange(Vec3.OneZ);
                    data.AddRange(Vec3.OneX);

                    data.AddRange(new Vec3(symbol.Offset + symbol.SymbolWidth + position, -line));
                    data.AddRange(symbol.UvMax);
                    data.AddRange(Vec3.OneZ);
                    data.AddRange(Vec3.OneX);

                    data.AddRange(new Vec3(symbol.Offset + position, -line));
                    data.AddRange(new Vec2(symbol.UvMin.X, symbol.UvMax.Y));
                    data.AddRange(Vec3.OneZ);
                    data.AddRange(Vec3.OneX);

                    totalSymbols++;
                }
                position += symbol.TotalWidth + characterSpacing;
            }
            position = Math.Max(0, position - characterSpacing);

            var leftAlign = ((int)label.TetxAlignment & 0b100010001) != 0;
            var rightAlign = ((int)label.TetxAlignment & 0b10001000100) != 0;
            var alignment = ((rightAlign ? 1 : 0) - (leftAlign ? 1 : 0) + 1) * .5f;

            var alignOffset = (1 - position / maxWidth) * 2 * alignment;
            for (var i = 1; i < data.Count; i += 11)
            {
                data[i - 1] = data[i - 1] * 2 / maxWidth - 1 + alignOffset;
                data[i] = data[i] / label.Lines * 2 + 1;
            }

            var indices = new uint[totalSymbols * 6];
            for (uint i = 0; i < totalSymbols; ++i)
            {
                indices[i * 6] = i * 4;
                indices[i * 6 + 1] = i * 4 + 1;
                indices[i * 6 + 2] = i * 4 + 2;
                indices[i * 6 + 3] = i * 4;
                indices[i * 6 + 4] = i * 4 + 2;
                indices[i * 6 + 5] = i * 4 + 3;
            }
            var result = new Mesh(false);
            result.SetAttributes(Mesh.EAttribute.Vec3, Mesh.EAttribute.Vec2, Mesh.EAttribute.Vec3, Mesh.EAttribute.Vec3);
            result.SetData(data.ToArray(), indices);
            return result;
        }
    }
}
