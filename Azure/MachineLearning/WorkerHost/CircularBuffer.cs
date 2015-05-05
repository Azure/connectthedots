using System.Collections.Generic;

namespace WorkerHost
{
    public class CircularBuffer<T>
    {
        private readonly Queue<T> _queue;
        private readonly int _size;

        public CircularBuffer(int size)
        {
            _queue = new Queue<T>(size);
            _size = size;
        }

        public int Count
        {
            get { return _queue.Count; }
        }

        public void Add(T obj)
        {
            if (_queue.Count == _size)
            {
                _queue.Dequeue();
            }

            _queue.Enqueue(obj);
        }

        public T[] GetAll()
        {
            return _queue.ToArray();
        }
    }
}
