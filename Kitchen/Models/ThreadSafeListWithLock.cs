using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kitchen.Models
{
    public class ThreadSafeListWithLock<T> : IList<T>
    {
        private readonly List<T> _internalList;

        private readonly object _lockList = new object();

        public ThreadSafeListWithLock()
        {
            _internalList = new List<T>();
        }

        // Other Elements of IList implementation

        public IEnumerator<T> GetEnumerator()
        {
            return Clone().GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Clone().GetEnumerator();
        }

        public List<T> Clone()
        {
            ThreadLocal<List<T>> threadClonedList = new ThreadLocal<List<T>>(true);

            lock (_lockList)
            {
                if (threadClonedList.Value == null)
                {
                    threadClonedList.Value = new List<T>();
                }
                _internalList.ForEach(element => { threadClonedList.Value.Add(element); });
            }

            return (threadClonedList.Value);
        }

        public void AddRange(List<T> items)
        {
            foreach (var item in items)
            {
                Add(item);
            }
        }

        public void Add(T item)
        {
            lock (_lockList)
            {
                _internalList.Add(item);
            }
        }

        public bool Remove(T item)
        {
            bool isRemoved;

            lock (_lockList)
            {
                isRemoved = _internalList.Remove(item);
            }

            return (isRemoved);
        }

        public void Clear()
        {
            lock (_lockList)
            {
                _internalList.Clear();
            }
        }

        public bool Contains(T item)
        {
            bool containsItem;

            lock (_lockList)
            {
                containsItem = _internalList.Contains(item);
            }

            return (containsItem);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (_lockList)
            {
                _internalList.CopyTo(array, arrayIndex);
            }
        }

        public int Count
        {
            get
            {
                int count;

                lock ((_lockList))
                {
                    count = _internalList.Count;
                }

                return (count);
            }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public int IndexOf(T item)
        {
            int itemIndex;

            lock ((_lockList))
            {
                itemIndex = _internalList.IndexOf(item);
            }

            return (itemIndex);
        }

        public void Insert(int index, T item)
        {
            lock ((_lockList))
            {
                _internalList.Insert(index, item);
            }
        }

        public void RemoveAt(int index)
        {
            lock ((_lockList))
            {
                _internalList.RemoveAt(index);
            }
        }

        public T this[int index]
        {
            get
            {
                lock ((_lockList))
                {
                    return _internalList[index];
                }
            }
            set
            {
                lock ((_lockList))
                {
                    _internalList[index] = value;
                }
            }
        }
    }
}
