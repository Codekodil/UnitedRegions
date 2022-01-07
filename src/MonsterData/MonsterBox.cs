using System;

namespace MonsterData
{
    public class MonsterBox
    {
        private static object _locker = new object();

        public int Size { get; }
        public bool FlowForward { get; }
        private readonly Monster[] _monsters;


        public MonsterBox(int size, bool flowForward)
        {
            _monsters = new Monster[Size = size];
            FlowForward = flowForward;
        }

        public Monster this[int index]
        {
            get
            {
                if (index < 0 || index >= Size)
                    throw new ArgumentOutOfRangeException(nameof(index));
                lock (_locker)
                    return _monsters[index];
            }
            set
            {
                if (index < 0 || index >= Size)
                    throw new ArgumentOutOfRangeException(nameof(index));
                lock (_locker)
                {
                    _monsters[index] = value;
                    Flow();
                }
            }
        }

        public bool Add(Monster monster)
        {
            lock (_locker)
            {
                for (var i = 0; i < Size; ++i)
                    if (_monsters[i] == null)
                    {
                        _monsters[i] = monster;
                        return true;
                    }
                return false;
            }
        }

        private void Flow()
        {
            if (!FlowForward) return;
            lock (_locker)
            {
                var end = 0;
                for (var i = 0; i < Size; ++i)
                {
                    if (_monsters[i] != null)
                    {
                        if (end < i)
                        {
                            _monsters[end] = _monsters[i];
                            _monsters[i] = null;
                        }
                        ++end;
                    }
                }
            }
        }

        public static void Switch(MonsterBox box1, int index1, MonsterBox box2, int index2)
        {
            if (box1 == null) throw new ArgumentNullException(nameof(box1));
            if (box2 == null) throw new ArgumentNullException(nameof(box2));
            if (index1 < 0 || index1 >= box1.Size)
                throw new ArgumentOutOfRangeException(nameof(index1));
            if (index2 < 0 || index2 >= box2.Size)
                throw new ArgumentOutOfRangeException(nameof(index2));
            lock (_locker)
            {
                var buffer = box1._monsters[index1];
                box1._monsters[index1] = box2._monsters[index2];
                box2._monsters[index2] = buffer;
                box1.Flow();
                box2.Flow();
            }
        }
    }
}
