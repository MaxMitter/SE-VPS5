using System;
using System.Threading;

namespace VPS.ToiletSimulation {
    public class FIFOQueue : Queue {
        private Semaphore sem = new Semaphore(0, Parameters.Producers * Parameters.JobsPerProducer);

        public FIFOQueue() { }

        public override void Enqueue(IJob job) {
            lock (queue) queue.Add(job);
            sem.Release();
        }

        public override bool TryDequeue(out IJob job) {
            sem.WaitOne();
            lock (queue) {
                if (queue.Count > 0) {
                    job = queue[0];
                    queue.RemoveAt(0);
                    return true;
                } else {
                    job = null;
                    return false;
                }
            }
        }

        public override void CompleteAdding() {
            base.CompleteAdding();
            if (producersComplete == Parameters.Producers)
                sem.Release(Parameters.Producers);
        }
    }
}
