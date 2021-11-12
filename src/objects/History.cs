using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VGraph.src.objects
{
    /// <summary>
    /// This is a basic stack implementation with an array, except it has a soft cap on its capacity.
    /// Any elements that would push the stack beyond its capacity instead causes the oldest element to be deleted.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class History<T>
    {
        private T[] HistArray;
        public int Count { get; set; }
        public History(int capacity)
        {
            Count = 0;
            HistArray = new T[capacity + 1];
        }

        public T Pop()
        {
            if (Count == 0)
            {
                throw new InvalidOperationException("Cannot pop a value off an empty History object");
            }
            Count--;
            T rVal = HistArray[Count];
            HistArray[Count] = default;
            return rVal;
        }

        public T Peek()
        {
            return HistArray[Count];
        }

        public void Push(T value)
        {
            HistArray[Count] = value;
            if (Count == HistArray.Length - 1)
            {
                for (int i = 0; i < HistArray.Length - 1;i++)
                {
                    HistArray[i] = HistArray[i + 1];
                }
            }
            else
            {
                Count++;
            }
        }

        public void Clear()
        {
            Count = 0;
            for (int i = 0; i < HistArray.Length; i++)
            {
                HistArray[i] = default;
            }
        }
    }
}
