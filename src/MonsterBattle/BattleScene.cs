using Gen4Assets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnhedderEngine;
using UnhedderEngine.Input;
using UnhedderEngine.Workflows.Core;
using static UnhedderEngine.Mesh;
using Math = UnhedderEngine.Math;

namespace MonsterBattle
{
    public class BattleScene
    {



        public BattleSeed Seed { get; }
        public Camera Camera { get; }
        public Assets Assets { get; }
        public bool Done { get; private set; }
        public event Action DoneChanged;
        public Scene Scene { get; }



        private readonly List<IEnumerator> _routines = new List<IEnumerator>();
        private IEnumerator[] GetRoutines() => _routines.ToArray();
        private void AddRoutine(IEnumerator routine, bool inFront = false)
        {
            if (inFront)
                _routines.Insert(0, routine);
            else
                _routines.Add(routine);
        }
        private void RemoveRoutine(IEnumerator routine) => _routines.Remove(routine);
        private bool RoutineDone(IEnumerator routine) => _routines.Contains(routine);
        private bool RoutinesDone(params IEnumerator[] routines) => routines.All(RoutineDone);

        private FrameData FrameData;
        private void OnUpdate(FrameData data)
        {
            FrameData = data;
            foreach (var routine in GetRoutines())
                if (!routine.MoveNext())
                    RemoveRoutine(routine);
        }

        public BattleScene(BattleSeed seed, Assets assets)
        {
            Seed = seed;
            Assets = assets;

            Scene = new Scene();
            var skybox = new SingleRenderer
            {
                Mesh = Sphere,
                Material = new Material(CustomShader.BattleSkyboxShader)
                { { "Albedo", Assets.BattleBackgroundAssets.ForestDay.Value } },
                Scale = Vec3.One * 100,
                Order = -100
            };
            Scene.Add(skybox);

            var o0 = new MonsterDisplay(this, Seed.OpponentTeam[0], true, true);
            var o1 = new MonsterDisplay(this, Seed.OpponentTeam[1], true, false);

            var p0 = new MonsterDisplay(this, Seed.PlayerTeam[0], false, true);
            var p1 = new MonsterDisplay(this, Seed.PlayerTeam[1], false, false);



            Scene.Add(new SingleRenderer
            {
                Mesh = ScreenSquare,
                Material = new Material(CustomShader.BattleGroundShader)
                {
                    { "Albedo", Assets.CommonBattleAssets.BattleGroundCircle.Value},
                    { "Center1", new Vec2(-.5f, 2)},
                    { "Radius1", 1f},
                    { "Center2", new Vec2(.5f, 2)},
                    { "Radius2", 1f},
                    { "Center3", new Vec2(-.5f, -2)},
                    { "Radius3", 1f},
                    { "Center4", new Vec2(.5f, -2)},
                    { "Radius4", 1f}
                },
                Scale = new Vec3(5),
                Rotation = Quaternion.FromAxis(Vec3.Left, Math.Pi * .5f)
            });


            o0.MonsterRenderer.Position += new Vec3(-.5f, 0, -2);
            o1.MonsterRenderer.Position += new Vec3(.5f, 0, -2);

            p0.MonsterRenderer.Position += new Vec3(-.5f, 0, 2);
            p1.MonsterRenderer.Position += new Vec3(.5f, 0, 2);

            Camera = new Camera
            {
                Scene = Scene,
                Position = new Vec3(1, 2, 4),
                Rotation = Quaternion.LookAt(-new Vec3(1, 2, 4), Vec3.Up)
            };

            AddRoutine(CameraMove());
            AddRoutine(MonsterSpriteAndHealth(o0));
            AddRoutine(MonsterSpriteAndHealth(o1));
            AddRoutine(MonsterSpriteAndHealth(p0));
            AddRoutine(MonsterSpriteAndHealth(p1));
            AddRoutine(test());

            IEnumerator test()
            {
                var clock = 0f;
                while ((clock += FrameData.DeltaTime) < 5)
                    yield return null;
                AddRoutine(Exit(1));
            }
            Scene.Update += OnUpdate;
        }

        private float _removeUi = 0f;
        private IEnumerator Exit(float time)
        {
            var clock = FrameData.DeltaTime;
            while (clock < time)
            {
                _removeUi = clock / time;
                yield return null;
                clock += FrameData.DeltaTime;
            }
            _removeUi = 1;
            Done = true;
            DoneChanged?.Invoke();
        }


        private IEnumerator DoInOrder(params IEnumerator[] routines)
        {
            foreach (var routine in routines)
            {
                AddRoutine(routine);
                while (!RoutineDone(routine))
                    yield return null;
            }
        }

        private IEnumerator MonsterSpriteAndHealth(MonsterDisplay sprite)
        {
            while (true)
            {
                sprite.MonsterRenderer.Rotation = Quaternion.LookAt(Camera.Forward.Scaled(1, 0, 1), Vec3.Up);
                sprite.HealthRenderer.Scale = sprite.HealthRenderer.Scale.Zyz.Scaled(1 - _removeUi, 1, 1);
                yield return null;
            }
        }


        private IEnumerator CameraMove()
        {
            var time = 0f; ;
            while (true)
            {
                time += FrameData.DeltaTime;
                Camera.Rotation = Quaternion.FromAxis(Vec3.Up, time * .25f) * Quaternion.FromAxis(Vec3.Left, Math.Sin(time * .5f) * .25f + .4f);
                Camera.Position = Camera.Backward * 3;
                yield return null;
            }
        }
    }
}
