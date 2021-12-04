using System;
using System.Collections.Generic;
using System.Threading;

namespace VPS.ToiletSimulation
{
    public abstract class Queue : IQueue
    {
        protected IList<IJob> queue;
        protected int producersComplete;

        private object locker;

        protected Queue()
        {
            queue = new List<IJob>();
        }

        public abstract void Enqueue(IJob job);

        public abstract bool TryDequeue(out IJob job);

        public virtual void CompleteAdding()
        {
            //producersComplete++;

            //lock(locker) producersComplete++;

            Interlocked.Increment(ref producersComplete);
        }

        public virtual bool IsCompleted
        {
            get
            {
                lock(queue) return producersComplete == Parameters.Producers && queue.Count == 0;
            }
        }
    }
}
