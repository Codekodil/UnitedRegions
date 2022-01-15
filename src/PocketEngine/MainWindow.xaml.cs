using AssetExtractor;
using GameplaySwitch;
using Gen4Assets;
using MapData;
using MapEngine;
using MapGameplay;
using MapGenerator;
using MonsterBattle;
using MonsterData;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using UnhedderEngine;

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
            CoreLogger.OnLog += m => System.Diagnostics.Debug.WriteLine(m);

            RomLoader.EnsureAssetsExist();
            InitializeComponent();

            var monsterDatabase = new MonsterDatabase();

            var assets = new Assets();

            var mainSwitch = new MainSwitch(assets);

            _world = new World();
            foreach (var type in typeof(CPlayer).Assembly.GetTypes())
                if (typeof(MapSystem).IsAssignableFrom(type))
                    _world.AddSystem(type);

            _world.MonsterTeam.Add(new Monster(monsterDatabase.Monsters[53], 10));
            _world.MonsterTeam.Add(new Monster(monsterDatabase.Monsters[398], 90));


            _world.GetSystem<SCameraManager>().CameraOffset = new Vec3(0, 32, 24);

            var generator = new RandomGenerator(69);
            generator.GenerateMap();
            _world.AddMap("main", generator.Map);

            _world.ShouldPauseLogic += () => !mainSwitch.ShowingWorld;


            var player = _world.NewEntity("main");
            player.Position = new Coord(0, 10);
            player.Add<CPlayer>();
            player.Add<CMovedByInput>();
            player.Add<CMoveable>().Speed = 4;
            player.Get<CMoveable>().Blockable = true;
            player.Add<COrientation>().Direction = COrientation.EDirection.Left;
            {
                var sprite = player.Add<CSprite>();
                sprite.Textures = assets.CommonMapAssets.FemalePlayer.Value.Frames;
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
                npc.Add<CSprite>().Textures = assets.CommonMapAssets.FemalePlayer.Value.Frames;
            }

            player.Map = generator.Map;


            var scene = _world.GetScene("main");

            var modelGenerator = new ModelGenerator { CommonMapAssets = assets.CommonMapAssets };
            foreach (var renderer in generator.Map.GetChunks().SelectMany(modelGenerator.GenerateRenderers))
                scene.Add(renderer);



            mainSwitch.CameraChange += camera =>
            {
                mainDisplay.Camera = camera;
            };
            mainSwitch.EnterWorld(_world);

            _world.GetSystem<SPlayer>().StepOntoTallGrass += p =>
            {
                var opponent = new MonsterBox(6, true);
                opponent.Add(new Monster(monsterDatabase.Monsters[193], 50) { Nickname = "Woopy Boi" });
                opponent.Add(new Monster(monsterDatabase.Monsters[78], 10));
                var battle = new BattleSeed(_world.MonsterTeam, opponent);
                mainSwitch.SwitchToBattle(battle);
            };

            Camera cameraWide = new Camera { Position = new Vec3(40, 70, 30), Rotation = Quaternion.LookAt(new Vec3(0, -5, -3), Vec3.Up), Scene = scene };
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
