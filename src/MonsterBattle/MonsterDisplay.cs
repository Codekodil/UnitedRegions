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

        public Monster Monster { get; }

        public BattleScene BattleScene { get; }
        public SingleRenderer MonsterRenderer { get; }

        public float SpriteFrontHeightOffset { get; private set; }
        public float SpriteBackHeightOffset { get; private set; }

        private float _backHeightOffset;
        public float BackHeightOffset
        {
            get => _backHeightOffset;
            set
            {
                _backHeightOffset = value;
                UpdatePosition();
            }
        }
        public float HeightOffset => ShowBack ? SpriteBackHeightOffset + BackHeightOffset : SpriteFrontHeightOffset;

        public Texture FrontTexture { get; private set; }
        public Texture BackTexture { get; private set; }

        private Vec3 _origin;
        public Vec3 Origin
        {
            get => _origin;
            set
            {
                _origin = value;
                UpdatePosition();
            }
        }

        private bool _showBack;
        public bool ShowBack
        {
            get => _showBack;
            set
            {
                _showBack = value;
                var texture = _showBack ? BackTexture : FrontTexture;
                if (texture != null)
                    MonsterRenderer.Material.SetAttribute("Albedo", texture);
                UpdatePosition();
            }
        }

        private void UpdatePosition()
        {
            MonsterRenderer.Position = Origin + Vec3.Up * HeightOffset;
        }

        public float Width { get; private set; } = 1;

        public MonsterDisplay(BattleScene scene, Monster monster, bool opponent, bool primary)
        {
            Monster = monster;
            BattleScene = scene;
            MonsterRenderer = new SingleRenderer
            {
                Mesh = _spriteMesh,
                Material = new Material(CustomShader.CutoffShader),
                Enabled = false
            };

            new Thread(() =>
            {
                var sprites = monster.LoadBattleTexture();
                FrontTexture = sprites.Front.Texture;
                BackTexture = sprites.Back.Texture;
                SpriteFrontHeightOffset = -sprites.Front.MinBoundingBox.Y;
                SpriteBackHeightOffset = -sprites.Back.MinBoundingBox.Y;
                Width = sprites.Front.MaxBoundingBox.X - sprites.Front.MinBoundingBox.X;
                ShowBack = ShowBack;
                MonsterRenderer.Enabled = true;
            }).Start();

            BattleScene.Scene.Add(MonsterRenderer);
        }
    }
}
