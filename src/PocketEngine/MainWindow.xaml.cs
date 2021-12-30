using AssetExtractor;
using GameplaySwitch;
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
        private World _world;
        public MainWindow()
        {
            RomLoader.EnsureAssetsExist();
            InitializeComponent();

            var assets = new CommonMapAssets();

            var mainSwitch = new MainSwitch();

            _world = new World();
            foreach (var type in typeof(CPlayer).Assembly.GetTypes())
                if (typeof(MapSystem).IsAssignableFrom(type))
                    _world.AddSystem(type);

            _world.GetSystem<SCameraManager>().CameraOffset = new Vec3(0, 32, 24);

            var generator = new RandomGenerator(69);
            generator.GenerateMap();
            _world.AddMap("main", generator.Map);

            _world.ShouldPauseLogic += () => !mainSwitch.ShowingWorld;


            var player = _world.NewEntity("main");
            player.Add<CPlayer>();
            player.Add<CMovedByInput>();
            player.Add<CMoveable>().Speed = 4;
            player.Get<CMoveable>().Blockable = true;
            player.Add<COrientation>().Direction = COrientation.EDirection.Left;
            {
                var sprite = player.Add<CSprite>();
                sprite.Textures = assets.FemalePlayer.Value.Frames;
                sprite.RightIndex = 3;
                sprite.BackwardIndex = 6;
                sprite.LeftIndex = 9;
                sprite.AnimationOffsets = new[] { 0, 1, 0, 2 };
                sprite.OnlyAnimateWhenMoving = true;
                sprite.AnimationFps = 8;
            }

            for (var i = 0; i < 4; ++i)
            {
                var npc = _world.NewEntity("main");
                npc.Position = new Coord(i - 2, 5);
                npc.Add<CSprite>().Textures = assets.FemalePlayer.Value.Frames;
            }

            player.Map = generator.Map;


            var scene = _world.GetScene("main");

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



            mainSwitch.CameraChange += camera =>
            {
                mainDisplay.Camera = camera;
            };
            mainSwitch.EnterWorld(_world);

            _world.GetSystem<SPlayer>().StepOntoTallGrass += p =>
            {
                mainSwitch.SwitchToBattle();
            };

            Camera cameraWide = new Camera { Position = Vec3.Up * 100, Rotation = Quaternion.LookAt(new Vec3(0, -4, -3), Vec3.Up), Scene = scene };
            Camera cameraAngle = new Camera { Position = Vec3.Up * 10, Rotation = Quaternion.LookAt(new Vec3(-1, -1, -2), Vec3.Up), Scene = scene };

            DataContext = new { cameraWide, cameraAngle };
        }

        private readonly HashSet<Key> _keys = new HashSet<Key>();

        private void UnhedderWpf_KeyDown(object sender, KeyEventArgs e) =>
            _world.GetSystem<SInput>().KeyDown(e.Key.ToString());

        private void UnhedderWpf_KeyUp(object sender, KeyEventArgs e) =>
            _world.GetSystem<SInput>().KeyUp(e.Key.ToString());
    }
}
