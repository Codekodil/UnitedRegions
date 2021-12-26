using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenTK
{
    public static class GL
    {
        private const int CurrentSize = 11;
        public static void Color3(float r, float g, float b) { }
        public static void Vertex3(Vector3 vec)
        {
            if (_current == null)
                return;
            _current[0] = vec.X;
            _current[1] = vec.Y;
            _current[2] = vec.Z;
            _modeBuffer.Add((float[])_current.Clone());
        }
        public static void Begin(BeginMode mode)
        {
            _modeBuffer = new List<float[]>();
            _mode = mode;
        }
        public static void Normal3(float x, float y, float z) { }
        public static void End()
        {
            switch (_mode)
            {
                case BeginMode.QuadStrip:
                    if (_modeBuffer.Count >= 4)
                    {
                        var firstIndex = _dataBuffer.Count / CurrentSize;
                        _dataBuffer.AddRange(_modeBuffer[0]);
                        _dataBuffer.AddRange(_modeBuffer[1]);
                        for (var i = 3; i < _modeBuffer.Count; i += 2)
                        {
                            _dataBuffer.AddRange(_modeBuffer[i - 1]);
                            _dataBuffer.AddRange(_modeBuffer[i]);
                            _indexBuffer.AddRange(new[] { 0, 1, 2, 2, 1, 3 }.Select(u => (uint)(u + i + firstIndex - 3)));
                        }
                    }
                    break;
                case BeginMode.Quads:
                    if (_modeBuffer.Count >= 4)
                    {
                        var firstIndex = _dataBuffer.Count / CurrentSize;
                        for (var i = 3; i < _modeBuffer.Count; i += 4)
                        {
                            _dataBuffer.AddRange(_modeBuffer[i - 3]);
                            _dataBuffer.AddRange(_modeBuffer[i - 2]);
                            _dataBuffer.AddRange(_modeBuffer[i - 1]);
                            _dataBuffer.AddRange(_modeBuffer[i]);
                            _indexBuffer.AddRange(new[] { 0, 1, 2, 0, 2, 3 }.Select(u => (uint)(u + i + firstIndex - 3)));
                        }
                    }
                    break;
                case BeginMode.TriangleStrip:
                    if (_modeBuffer.Count >= 3)
                    {
                        var firstIndex = _dataBuffer.Count / CurrentSize;
                        var flipped = false;
                        _dataBuffer.AddRange(_modeBuffer[0]);
                        _dataBuffer.AddRange(_modeBuffer[1]);
                        for (var i = 2; i < _modeBuffer.Count; ++i)
                        {
                            _dataBuffer.AddRange(_modeBuffer[i]);
                            _indexBuffer.AddRange((flipped ? new[] { 1, 0, 2 } : new[] { 0, 1, 2 }).Select(u => (uint)(u + i + firstIndex - 2)));
                            flipped = !flipped;
                        }
                    }
                    break;
                case BeginMode.Triangles:
                    if (_modeBuffer.Count >= 3)
                    {
                        var firstIndex = _dataBuffer.Count / CurrentSize;
                        for (var i = 2; i < _modeBuffer.Count; i += 3)
                        {
                            _dataBuffer.AddRange(_modeBuffer[i - 2]);
                            _dataBuffer.AddRange(_modeBuffer[i - 1]);
                            _dataBuffer.AddRange(_modeBuffer[i]);
                            _indexBuffer.AddRange(new[] { 0, 1, 2 }.Select(u => (uint)(u + i + firstIndex - 2)));
                        }
                    }
                    break;
                default:
                    throw new NotImplementedException(_mode.ToString());
            }
            _modeBuffer = null;
            _mode = BeginMode.None;
        }
        public static void Flush() { }
        public static void TexCoord2(double u, double v)
        {
            if (_current == null)
                return;
            _current[3] = (float)u;
            _current[4] = (float)v;
        }

        [ThreadStatic]
        private static BeginMode _mode;
        [ThreadStatic]
        private static List<float> _dataBuffer;
        [ThreadStatic]
        private static List<uint> _indexBuffer;
        [ThreadStatic]
        private static List<float[]> _modeBuffer;
        [ThreadStatic]
        private static float[] _current;

        public static void StartNewModel()
        {
            _dataBuffer = new List<float>();
            _indexBuffer = new List<uint>();
            _modeBuffer = new List<float[]>();
            _mode = BeginMode.None;
            _current = new float[CurrentSize];
        }

        public static Tuple<float[], uint[]> GetModel()
        {
            var resultData = new float[_dataBuffer.Count];
            var resultIndices = new uint[_indexBuffer.Count];
            _dataBuffer.CopyTo(resultData);
            _indexBuffer.CopyTo(resultIndices);
            _dataBuffer = null;
            _indexBuffer = null;
            _modeBuffer = null;
            _current = null;
            _mode = BeginMode.None;
            return Tuple.Create(resultData, resultIndices);
        }
    }
}
