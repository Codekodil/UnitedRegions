using Gen4Assets;
using GuiDesign;
using GuiEngine;
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

        private readonly GuiManager<MainBattleGui> _guiManager;


        private readonly List<IEnumerator> _routines = new List<IEnumerator>();
        private IEnumerator[] GetRoutines() => _routines.ToArray();
        private IEnumerator AddRoutine(Func<IEnumerator> routine, bool inFront = false)
        {
            var enumerator = routine();
            AddRoutine(enumerator, inFront);
            return enumerator;
        }
        private IEnumerator AddRoutine(IEnumerator routine, bool inFront = false)
        {
            if (inFront)
                _routines.Insert(0, routine);
            else
                _routines.Add(routine);
            return routine;
        }
        private void RemoveRoutine(IEnumerator routine) => _routines.Remove(routine);
        private bool RoutineDone(IEnumerator routine) => !_routines.Contains(routine);
        private bool RoutinesDone(params IEnumerator[] routines) => routines.All(RoutineDone);

        private FrameData FrameData;
        private void OnUpdate(FrameData data)
        {
            FrameData = data;

            if (_guiManager.Control == null)
                Do();
            else
                _guiManager.Control.Invoke(new Action(Do));
            void Do()
            {
                foreach (var routine in GetRoutines())
                {
                    try
                    {
                        if (!routine.MoveNext())
                            RemoveRoutine(routine);
                    }
                    catch (Exception ex)
                    {
                        CoreLogger.Write(ex);
                        RemoveRoutine(routine);
                    }
                }
            }
        }

        private MonsterDisplay _playerDisplayLeft;
        private MonsterDisplay _playerDisplayRight;
        private MonsterDisplay _opponentDisplayLeft;
        private MonsterDisplay _opponentDisplayRight;

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

            _guiManager = new GuiManager<MainBattleGui>(Scene, Assets);

            _opponentDisplayLeft = new MonsterDisplay(this, Seed.OpponentTeam[0], true, true);
            _opponentDisplayRight = new MonsterDisplay(this, Seed.OpponentTeam[1], true, false);

            _playerDisplayLeft = new MonsterDisplay(this, Seed.PlayerTeam[0], false, true);
            _playerDisplayRight = new MonsterDisplay(this, Seed.PlayerTeam[1], false, false);
            _playerDisplayLeft.ShowBack = true;
            _playerDisplayRight.ShowBack = true;
            _playerDisplayRight.BackHeightOffset = .2f;

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


            _opponentDisplayLeft.Origin = new Vec3(-.5f, 0, -2);
            _opponentDisplayRight.Origin = new Vec3(.5f, 0, -2);

            _playerDisplayLeft.Origin = new Vec3(-.5f, 0, 2);
            _playerDisplayRight.Origin = new Vec3(.5f, 0, 2);

            Camera = new Camera { Scene = Scene };
            Camera.SetFoV(.4f);

            AddRoutine(Intro);
            AddRoutine(MonsterSpriteRotate(_opponentDisplayLeft));
            AddRoutine(MonsterSpriteRotate(_opponentDisplayRight));
            AddRoutine(MonsterSpriteRotate(_playerDisplayLeft));
            AddRoutine(MonsterSpriteRotate(_playerDisplayRight));

            Scene.Update += OnUpdate;
        }

        private IEnumerator Exit()
        {
            Done = true;
            DoneChanged?.Invoke();
            yield break;
        }



        private IEnumerator Intro()
        {
            Vec3 zoomOpponentFocus = new Vec3(0, 0, -3);
            Vec3 zoomOpponentPosition = new Vec3(0, 1, 2);

            var lookAt = AddRoutine(AnimateCamera(
                t => Curve.Bezier(t, new Vec3(1, 1, 0), zoomOpponentPosition),
                t => Curve.Bezier(t, new Vec3(-2, 0, 0), zoomOpponentFocus, zoomOpponentFocus),
                3));

            var showUi = AddRoutine(DoInOrder(
                () => Delay(2),
                EnsureGuiLoaded));

            AddRoutine(DoAfter(() => MonsterSpriteHealthDisplay(_opponentDisplayLeft, _guiManager.Control.OpponentHealthDisplay1), showUi));
            AddRoutine(DoAfter(() => MonsterSpriteHealthDisplay(_opponentDisplayRight, _guiManager.Control.OpponentHealthDisplay2), showUi));
            var showTextbox = AddRoutine(DoAfter(WildMonsterAppearTextbox, showUi));

            while (!RoutinesDone(lookAt, showUi, showTextbox))
                yield return null;

            Vec3 trainerFocus = new Vec3(-.5f, 0, 0);
            Vec3 trainerPosition = new Vec3(2.3f, 2.45f, 5.7f);

            lookAt = AddRoutine(AnimateCamera(
                t => Curve.Bezier(t, zoomOpponentPosition, trainerPosition, trainerPosition),
                t => Curve.Bezier(t, zoomOpponentFocus, trainerFocus, trainerFocus),
                3));

            showUi = AddRoutine(Delay(2));

            AddRoutine(DoAfter(() => MonsterSpriteHealthDisplay(_playerDisplayLeft, _guiManager.Control.PlayerHealthDisplay1), showUi));
            AddRoutine(DoAfter(() => MonsterSpriteHealthDisplay(_playerDisplayRight, _guiManager.Control.PlayerHealthDisplay2), showUi));

            while (!RoutinesDone(lookAt, showUi))
                yield return null;

            AddRoutine(DoInOrder(() => Delay(5), Exit));

            //while (true)
            //{
            //    Camera.Position = trainerPosition;
            //    Camera.Rotation = Quaternion.LookAt(trainerFocus - Camera.Position, Vec3.Up);
            //    yield return null;
            //}
        }


        private IEnumerator WildMonsterAppearTextbox()
        {
            _guiManager.Control.SetInfo($"A wild {_opponentDisplayLeft.Monster.DisplayName} and {_opponentDisplayRight.Monster.DisplayName} appeared!");
            return Animate(time =>
                _guiManager.Control.SetOffScreen(1 - time),
                .25f);
        }


        private IEnumerator AnimateCamera(Func<float, Vec3> position, Func<float, Vec3> focus, float duration) =>
            Animate(time =>
            {
                Camera.Position = position(time);
                Camera.Rotation = Quaternion.LookAt(focus(time) - Camera.Position, Vec3.Up);
            }, duration);



        private IEnumerator DoAfter(Func<IEnumerator> followUp, params IEnumerator[] routines)
        {
            while (!RoutinesDone(routines))
                yield return null;
            var routine = followUp();
            AddRoutine(routine);
            while (!RoutineDone(routine))
                yield return null;
        }


        private IEnumerator DoInOrder(params Func<IEnumerator>[] routines)
        {
            foreach (var routine in routines)
            {
                var enumerator = routine();
                AddRoutine(enumerator);
                while (!RoutineDone(enumerator))
                    yield return null;
            }
        }


        private IEnumerator MonsterSpriteHealthDisplay(MonsterDisplay sprite, IHealthDisplay healthDisplay)
        {
            healthDisplay.MonsterName = sprite.Monster.DisplayName;
            healthDisplay.Level = sprite.Monster.Level;
            if (healthDisplay.OffScreen > 0)
                AddRoutine(Animate(time => healthDisplay.OffScreen = 1 - time, .25f));
            while (true)
            {
                healthDisplay.MaxHp = sprite.Monster.Stats.HP;
                healthDisplay.CurrentHp = sprite.Monster.CurrentHP;
                yield return null;
            }
        }


        private IEnumerator MonsterSpriteRotate(MonsterDisplay sprite)
        {
            while (true)
            {
                sprite.MonsterRenderer.Rotation = Quaternion.LookAt(Camera.Forward.Scaled(1, 0, 1), Vec3.Up);
                yield return null;
            }
        }

        private IEnumerator GuiShowAtStart() =>
            Animate(time =>
                _guiManager.Control.PlayerHealthDisplay1.OffScreen =
                _guiManager.Control.PlayerHealthDisplay2.OffScreen =
                _guiManager.Control.OpponentHealthDisplay1.OffScreen =
                _guiManager.Control.OpponentHealthDisplay2.OffScreen =
                1f - time,
            .25f);

        private IEnumerator Delay(float duration)
        {
            var time = FrameData.DeltaTime;
            while (time < duration)
            {
                yield return null;
                time += FrameData.DeltaTime;
            }
        }

        private IEnumerator Animate(Action<float> action, float duration)
        {
            var time = FrameData.DeltaTime;
            while (time < duration)
            {
                action(time / duration);
                yield return null;
                time += FrameData.DeltaTime;
            }
            action(1);
        }

        private IEnumerator EnsureGuiLoaded()
        {
            while (_guiManager.Control == null)
                yield return null;
        }

    }
}
