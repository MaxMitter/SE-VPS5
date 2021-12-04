using System;
using System.Threading;
using System.Linq;
using System.Collections.Generic;

namespace VPS.ToiletSimulation {
    public static class QueueExtension {
        public static void InsertSorted(this IList<IJob> target, IJob newJob) {
            var index = 0;
            for (var i = 0; i < target.Count; i++) {
                if (target[i].WaitingTime < newJob.WaitingTime) {
                    index++;
                } else {
                    break;
                }
            }
            target.Insert(index, newJob);
        }
    }

    public class ToiletQueue : Queue {
        private Semaphore sem = new Semaphore(0, Parameters.Producers * Parameters.JobsPerProducer);

        public ToiletQueue() { }

        public override void Enqueue(IJob job) {
            lock (queue) {
                queue.InsertSorted(job);
            }
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
