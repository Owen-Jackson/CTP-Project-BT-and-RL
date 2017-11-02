using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CustomExtensions
{
    //from Eric J. at https://stackoverflow.com/questions/273313/randomize-a-listt
    public static class ThreadSafeRandom
    {
        [ThreadStatic] private static Random local;
        public static Random ThisThreadsRandom
        {
            get { return local ?? (local = new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId))); }
        }
    }

    //from Eric J. at https://stackoverflow.com/questions/273313/randomize-a-listt
    public static class MyExtensions
    {
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while(n>1)
            {
                n--;
                int k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }

}