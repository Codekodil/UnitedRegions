using AssetExtractor;
using Gen4Assets;
using MapData;
using MapEngine;
using MapGameplay;
using MapGenerator;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using UnhedderEngine;
using UnhedderEngine.Workflows.Core;

namespace PocketEngine
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            RomLoader.EnsureAssetsExist();
            InitializeComponent();

            var assets = new CommonMapAssets();

            var world = new World();
            world.AddSystem<SMoveable>();
            world.AddSystem<SRendering>();
            var player = world.NewEntity();
            player.Add<CMoveable>().Speed = 6;
            player.Add<CRendering>();
            player.Add<CSprite>().Texture = assets.FemalePlayer.Value.Backward;

            for (var i = 0; i < 4; ++i)
            {
                var npc = world.NewEntity();
                npc.Position = new Coord(i - 2, 5);
                npc.Add<CRendering>();
                npc.Add<CSprite>().Texture = assets.FemalePlayer.Value.Backward;
            }

            var generator = new RandomGenerator(69);
            generator.GenerateMap();


            var scene = world.Scene;

            for (var x = 0; x < 5; ++x)
                for (var y = 0; y < 5; ++y)
                {
                    var material = DefaultMaterials.Textured.Clone();
                    material.SetAttribute("Albedo", BattleSpriteLoader.LoadSprite((1 + x + y * 5) * 6));
                    scene.Add(new SingleRenderer
                    {
                        Mesh = Mesh.Cube,
                        Material = material,
                        Scale = new Vec3(4, 2, 1),
                        Position = new Vec3(x * 4 + 8, y * 2, -14)
                    });
                }


            var modelGenerator = new ModelGenerator { CommonMapAssets = assets };
            foreach (var renderer in generator.Map.GetChunks().SelectMany(modelGenerator.GenerateRenderers))
                scene.Add(renderer);


            Camera camera = new Camera { Position = Vec3.Up * 30, Rotation = Quaternion.LookAt(new Vec3(0, -4, -3), Vec3.Up), Scene = scene };
            camera.SetFoV(Math.Pi * .15f);
            Camera cameraWide = new Camera { Position = Vec3.Up * 100, Rotation = Quaternion.LookAt(new Vec3(0, -4, -3), Vec3.Up), Scene = scene };
            Camera cameraAngle = new Camera { Position = Vec3.Up * 10, Rotation = Quaternion.LookAt(new Vec3(-1, -1, -2), Vec3.Up), Scene = scene };

            DataContext = new { camera, cameraWide, cameraAngle };
            camera.SetFoV(Math.Pi * .15f);

            camera.Update += data =>
            {
                lock (_keys)
                {
                    var move = new Coord(
                        (_keys.Contains(Key.A) ? -1 : 0) + (_keys.Contains(Key.D) ? 1 : 0),
                        (_keys.Contains(Key.S) ? -1 : 0) + (_keys.Contains(Key.W) ? 1 : 0));
                    if (move.Y != 0) move.X = 0;
                    player.Get<CMoveable>().NextMove = move;
                }
                var focusPosition = player.CenterVec;
                foreach (var cam in new[] { camera, cameraWide, cameraAngle })
                {
                    var forward = cam.Rotation.Forward;
                    cam.Position = focusPosition + forward / forward.Y * cam.Position.Y;
                }
            };
        }

        private readonly HashSet<Key> _keys = new HashSet<Key>();

        private void UnhedderWpf_KeyDown(object sender, KeyEventArgs e)
        {
            lock (_keys)
                _keys.Add(e.Key);
        }

        private void UnhedderWpf_KeyUp(object sender, KeyEventArgs e)
        {
            lock (_keys)
                _keys.Remove(e.Key);
        }
    }
}
