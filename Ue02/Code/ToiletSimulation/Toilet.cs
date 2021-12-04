using System;
using System.Threading;

namespace VPS.ToiletSimulation {
    public class Toilet {
        public string Name { get; private set; }

        private IQueue queue;

        private Thread consumer;

        public Toilet(string name, IQueue queue) {
            Name = name;
            this.queue = queue;
        }

        public void Consume() {
            consumer = new Thread(Run);
            consumer.Start();
        }

        private void Run() {
            while (!queue.IsCompleted) {
                IJob job;
                if (queue.TryDequeue(out job))
                    job.Process();
                else
                    Console.WriteLine("toilet is starving :-/");
            }
        }

        public void Join() {
            consumer.Join();
        }
    }
}
