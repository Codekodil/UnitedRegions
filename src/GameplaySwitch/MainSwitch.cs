using Gen4Assets;
using MapEngine;
using MapGameplay;
using MonsterBattle;
using System;
using UnhedderEngine;
using UnhedderEngine.Input;

namespace GameplaySwitch
{
    public class MainSwitch
    {
        private readonly object _locker = new object();
        public World World { get; private set; }
        private readonly SceneTransition _transition = new SceneTransition();

        public event Action<Camera> CameraChange;
        private Camera _lastCamera;
        public bool ShowingWorld { get; private set; }
        public Assets Assets { get; }

        public MainSwitch(Assets assets) => Assets = assets;

        public void EnterWorld(World world)
        {
            lock (_locker)
            {
                World = world;
                ShowingWorld = true;
                _transition.InstantTransition(World.GetSystem<SCameraManager>().GetCamera("main"));
            }
            CheckCamera();
        }

        public void SwitchToBattle(BattleSeed battle)
        {
            lock (_locker)
            {
                ShowingWorld = false;
                var battleScene = new BattleScene(battle, Assets);
                _transition.Transition(battleScene.Camera);
                battleScene.DoneChanged += () =>
                {
                    lock (_locker)
                    {
                        _transition.Transition(World.GetSystem<SCameraManager>().GetCamera("main"));
                        ShowingWorld = true;
                        CheckCamera();
                    }
                };
            }
            CheckCamera();
        }

        private void CheckCamera()
        {
            Camera changed;
            lock (_locker)
            {
                if (_transition.Camera == _lastCamera) return;
                changed = _lastCamera = _transition.Camera;
            }
            Action<FrameData> action = null;
            action = new Action<FrameData>(_ =>
            {
                if (action != null)
                {
                    CameraChange?.Invoke(changed);
                    EventManager.Update -= action;
                }
            });
            EventManager.Update += action;
        }
    }
}
