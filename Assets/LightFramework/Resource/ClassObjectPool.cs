using System.Collections.Generic;

namespace LightFramework.Resource
{
    public class ClassObjectPool<T> where T : class, new()
    {
        protected Stack<T> pool=new Stack<T>();

        //<=0 infinity
        protected int capacity = 0;

        protected int unRecycledCount = 0;

        public ClassObjectPool(int capacity)
        {
            this.capacity = capacity;
            for (int i = 0; i < capacity; i++)
            {
                pool.Push(new T());
            }
        }
        public T Spawn(bool creatIfPoolEmpty=true)
        {
            if (pool.Count > 0)
            {
                T res = pool.Pop();
                if (res == null)
                {
                    if (creatIfPoolEmpty)
                    {
                        res = new T();
                    }
                }
                unRecycledCount++;
                return res;
            }
            if (creatIfPoolEmpty)
            {
                T res = new T();
                unRecycledCount++;
                return res;
            }
            return null;
        }
        public bool Recycle(T obj)
        {
            if (obj == null) return false;
            unRecycledCount--;
            if (pool.Count >= capacity && capacity > 0)
            {
                return false;
            }
            pool.Push(obj);
            return true;
        }
    }
}