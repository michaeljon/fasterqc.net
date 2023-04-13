using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Ovation.BgzMR
{
    public static class ThreadPoolUtils
    {
        public static IReadOnlyCollection<T> RunOnThreadPool<T>(IReadOnlyList<Func<T>> workFunctions)
        {
            var results = new ConcurrentBag<T>();

            using (var countdownEvent = new CountdownEvent(workFunctions.Count))
            {
                for (var i = 0; i < workFunctions.Count; i++)
                {
                    var idx = i;
                    ThreadPool.QueueUserWorkItem(_ =>
                    {
                        var res = workFunctions[idx]();
                        results.Add(res);
                        countdownEvent.Signal();
                    });
                }

                countdownEvent.Wait();
            }

            return results;
        }
    }
}
