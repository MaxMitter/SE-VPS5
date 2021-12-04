// See https://aka.ms/new-console-template for more information

using System;
using System.Threading;
using Microsoft.VisualBasic.CompilerServices;
using RaceCondition;

namespace RaceCondition {
    class RaceCond {
        private const int N = 1000;
        private const int BUFFER_SIZE = 10;

        private double[] buffer;
        private AutoResetEvent canWrite;
        private AutoResetEvent canRead;

        public void Run() {
            buffer = new double[BUFFER_SIZE];
            //canWrite = new AutoResetEvent(true);
            //canRead = new AutoResetEvent(false);

            // start threads 
            var t1 = new Thread(Reader);
            var t2 = new Thread(Writer);
            t1.Start();
            t2.Start();

            // wait for threads 
            t1.Join();
            t2.Join();
        }

        private void Reader() {
            var readerIndex = 0;
            for (int i = 0; i < N; i++) {
                //canRead.WaitOne();
                Console.WriteLine(buffer[readerIndex]);
                //canWrite.Set();
                readerIndex = (readerIndex + 1) % BUFFER_SIZE;
            }
        }

        private void Writer() {
            var writerIndex = 0;
            for (int i = 0; i < N; i++) {
                //canWrite.WaitOne();
                buffer[writerIndex] = (double)i;
                //canRead.Set();
                writerIndex = (writerIndex + 1) % BUFFER_SIZE;
            }
        }
    }
}


public class Program {
    public static int sum = 0;

    public static void Main(string[] args) {
        var rc = new RaceCond();

        rc.Run();
    }
}