using Gen4Assets;
using System;
using System.Collections.Generic;
using UnhedderEngine;
using UnhedderEngine.Input;
using UnhedderEngine.Workflows.Core;
using Math = UnhedderEngine.Math;

namespace GameplaySwitch
{
    public class SceneTransition
    {
        private static Shader _transitionShader;
        static SceneTransition()
        {
            _transitionShader = new Shader(@"
uniform instanced mat4 Model;
uniform mat4 View;
uniform mat4 Projection;
uniform sampler2D Albedo = vec3(1.0, 0.0, 1.0);
uniform sampler2D Buffer = vec3(1.0, 0.0, 1.0);
uniform float DesiredRatio = 1.0;
uniform float Ratio;
uniform mat4 TransitionCameraStart;
uniform mat4 TransitionCameraEnd;
uniform float TransformAnimation;
uniform float ColorAnimation;
uniform vec3 TransitionMove;
vec4 Vertex(vec3 position, vec2 uv, vec3 normal, vec3 tangent)
{
    vec4 start = (TransitionCameraStart * Model * vec4(position, 1.0)) * vec4(max(1.0, DesiredRatio / Ratio), 1.0, 1.0, 1.0);
    vec4 end = (TransitionCameraEnd * Model * vec4(position + TransitionMove, 1.0)) * vec4(max(1.0, DesiredRatio / Ratio), 1.0, 1.0, 1.0);
    return mix(start, end, TransformAnimation);
}
vec4 Fragment()
{
    vec4 color = texture(Albedo, uv);
    vec3 bufferColor = texture(Buffer, (start.xy * vec2(min(1.0, Ratio / DesiredRatio), 1.0) + vec2(1.0, 1.0)) * 0.5).xyz;
    if(color.a < 0.5)
        discard;
    return vec4(mix(bufferColor, color.xyz, ColorAnimation), 1.0);
}
");
        }

        private struct TransitionData
        {
            public Scene Scene;
            public List<SingleMatrixRenderer> Renderers;
            public float Progress;
            public float Length;
            public Func<float, float> TransformAnimation;
            public Func<float, float> ColorAnimation;
        }

        private readonly object _locker = new object();
        private readonly Framebuffer _originDisplay = new Framebuffer(2520, 1080, false, EColorAttachment.RGB);
        private TransitionData? _currentTransition;

        private void RemoveTransition()
        {
            lock (_locker)
            {
                if (!_currentTransition.HasValue) return;
                foreach (var renderer in _currentTransition.Value.Renderers)
                    _currentTransition.Value.Scene.Remove(renderer);
                _originDisplay.Camera = null;
                EventManager.Update -= OnUpdate;
                _currentTransition = null;
            }
        }
        private void OnUpdate(FrameData data)
        {
            lock (_locker)
            {
                if (!_currentTransition.HasValue)
                {
                    EventManager.Update -= OnUpdate;
                    return;
                }
                var transition = _currentTransition.Value;
                transition.Progress += data.DeltaTime / _currentTransition.Value.Length;
                if (transition.Progress >= 1)
                {
                    RemoveTransition();
                    return;
                }
                var transformAnimation = transition.TransformAnimation(transition.Progress);
                var colorAnimation = transition.ColorAnimation(transition.Progress);
                foreach (var renderer in transition.Renderers)
                {
                    renderer.Material.SetAttribute("TransformAnimation", transformAnimation);
                    renderer.Material.SetAttribute("ColorAnimation", colorAnimation);
                }
                _currentTransition = transition;
            }
        }
        private void SetTransition(TransitionData transition)
        {
            lock (_locker)
            {
                if (_currentTransition.HasValue)
                    RemoveTransition();
                _currentTransition = transition;
                EventManager.Update += OnUpdate;
            }
        }

        public Camera Camera { get; private set; }

        private TransitionAssets _assets = new TransitionAssets();

        public void InstantTransition(Camera camera)
        {
            lock (_locker)
            {
                _originDisplay.Camera = Camera;
                Camera = camera;
            }
        }
        public void Transition(Camera camera)
        {
            lock (_locker)
            {
                if (camera == Camera) return;

                _originDisplay.Camera = Camera;
                Camera = camera;

                var scene = camera?.Scene as Scene;
                if (scene == null) return;

                var transitionStart = Mat4.Scale(.75f, .75f, -.01f) * Quaternion.FromAxis(Vec3.Right, 1.1f).ToMat4();
                var transitionEnd = Mat4.Scale(1, 1, -.01f) * Quaternion.FromAxis(Vec3.Right, 0.1f).ToMat4();

                var transforms = new[] {
                    (Mat4.FromPositionRotationScale(new Vec3(0, 0, 1.5f), Quaternion.NoRotation, Vec3.One), new Vec3(0, -5.5f, 0)),

                    (Mat4.FromPositionRotationScale(new Vec3(1.25f, 0, 0), Quaternion.FromAxis(Vec3.Backward, .1f), Vec3.One), new Vec3(.5f, -.7f, 0)),
                    (Mat4.FromPositionRotationScale(new Vec3(-1.25f, 0, 0), Quaternion.FromAxis(Vec3.Forward, .1f), Vec3.One), new Vec3(-.5f, -.7f, 0)),

                    (Mat4.FromPositionRotationScale(new Vec3(2f, 0, 2.5f), Quaternion.FromAxis(Vec3.Backward, .3f), Vec3.One), new Vec3(.2f, -3, 0)),
                    (Mat4.FromPositionRotationScale(new Vec3(-2f, 0, 2.5f), Quaternion.FromAxis(Vec3.Forward, .3f), Vec3.One), new Vec3(-.2f, -3, 0))};

                var renderers = new List<SingleMatrixRenderer>();

                foreach (var p in transforms)
                    foreach (var v in _assets.Tree.Value.Data)
                    {
                        var material = new Material(_transitionShader);
                        material.SetAttribute("Albedo", v.Texture);
                        material.SetAttribute("Buffer", _originDisplay.ColorTexture);
                        material.SetAttribute("TransitionCameraStart", transitionStart);
                        material.SetAttribute("TransitionCameraEnd", transitionEnd);
                        material.SetAttribute("DesiredRatio", _originDisplay.Size.X / _originDisplay.Size.Y);
                        material.SetAttribute("TransitionMove", p.Item2);
                        var treeRenderer = new SingleMatrixRenderer
                        {
                            Mesh = v.Mesh,
                            Material = material,
                            Transform = p.Item1,
                            Order = 1000000
                        };
                        scene.Add(treeRenderer);
                        renderers.Add(treeRenderer);
                    }

                SetTransition(new TransitionData
                {
                    Renderers = renderers,
                    Length = 1,
                    Scene = scene,
                    TransformAnimation = p => Math.Sqr(Math.Max(0f, p * 1.5f - .5f)),
                    ColorAnimation = p => p
                });
            }

        }
    }
}
