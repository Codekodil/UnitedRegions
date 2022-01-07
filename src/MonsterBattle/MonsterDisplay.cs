using Gen4Assets;
using MonsterData;
using System.Threading;
using UnhedderEngine;
using UnhedderEngine.Workflows.Core;
using static UnhedderEngine.Mesh;

namespace MonsterBattle
{
    public class MonsterDisplay
    {
        private static readonly Mesh _spriteMesh;
        static MonsterDisplay()
        {
            _spriteMesh = new Mesh(false);
            _spriteMesh.SetAttributes(EAttribute.Vec3, EAttribute.Vec2, EAttribute.Vec3, EAttribute.Vec3);
            _spriteMesh.SetData(new[] {
                -.5f,0,0, 0,0, 0,0,1, 1,0,0,
                .5f,0,0, .5f,0, 0,0,1, 1,0,0,
                -.5f,1,0, 0,1, 0,0,1, 1,0,0,
                .5f,1,0, .5f,1, 0,0,1, 1,0,0},
                new uint[] { 0, 1, 2, 2, 1, 3 });
        }

        public BattleScene BattleScene { get; }
        public SingleRenderer MonsterRenderer { get; }
        public SingleRenderer HealthRenderer { get; }

        public float FrontHeightOffset { get; private set; }
        public float BackHeightOffset { get; private set; }
        public float Width { get; private set; } = 1;

        public MonsterDisplay(BattleScene scene, Monster monster, bool opponent, bool primary)
        {
            BattleScene = scene;
            MonsterRenderer = new SingleRenderer
            {
                Mesh = _spriteMesh,
                Material = new Material(CustomShader.CutoffShader)
            };

            const float height = .15f;
            HealthRenderer = new SingleRenderer
            {
                Mesh = ScreenSquare,
                Material = new Material(CustomShader.GuiShader)
                {
                    { "Alignment", opponent ? -1f : 1f }
                },
                Position = new Vec3(
                    primary ? (opponent ? -1 : 1) * (height * .5f) : 0f,
                    (opponent ? 1f - 3 * height : height - 1f) + (primary ? 0f : height * 2),
                    0),
                Scale = new Vec3(4, 1, 4) * height,
                Order = 1000
            };
            new Thread(() =>
                HealthRenderer.Material.SetAttribute("Albedo", (opponent ?
                    BattleScene.Assets.CommonBattleAssets.OpponentHealthDisplay :
                    BattleScene.Assets.CommonBattleAssets.PlayerHealthDisplay)
                    .Value)).Start();

            new Thread(() =>
            {
                var sprites = monster.LoadBattleTexture();
                MonsterRenderer.Material.SetAttribute("Albedo", sprites.Front.Texture);
                FrontHeightOffset = -sprites.Front.MinBoundingBox.Y;
                BackHeightOffset = -sprites.Back.MinBoundingBox.Y;
                Width = sprites.Front.MaxBoundingBox.X - sprites.Front.MinBoundingBox.X;
                MonsterRenderer.Position += Vec3.OneY * FrontHeightOffset;
            }).Start();

            BattleScene.Scene.Add(MonsterRenderer);
            BattleScene.Scene.Add(HealthRenderer);
        }
    }
}
