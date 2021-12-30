using MapEngine;
using MapGameplay;
using System;
using UnhedderEngine;
using UnhedderEngine.Workflows.Core;

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

        public void SwitchToBattle()
        {
            lock (_locker)
            {
                ShowingWorld = false;
                _transition.Transition(new Camera { Scene = new Scene() });
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
            CameraChange?.Invoke(changed);
        }
    }
}
