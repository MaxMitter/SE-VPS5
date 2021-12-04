using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

class LimitedConnectionsExample {
    private Semaphore semaphore;
    public LimitedConnectionsExample() {
        semaphore = new Semaphore(10, 10);
    }

    public void DownloadFilesAsync(IEnumerable<string> urls) {
        foreach (var url in urls) {
            semaphore.WaitOne();
            Thread t = new Thread(() => DownloadFile(url));
            t.Start();
        }

        Console.WriteLine("Finished");
    }

    public void DownloadFiles(IEnumerable<string> urls) {
        List<Thread> threads = new List<Thread>();

        foreach (var url in urls) {
            semaphore.WaitOne();
            Thread t = new Thread(() => DownloadFile(url));
            threads.Add(t);
            t.Start();
        }
        foreach (var t in threads) {
            t.Join();
        }        
    }

    private void DownloadFile(object url) {
        // download and store file ... 
        Thread.Sleep(2000);
        Console.WriteLine($"File downloaded using Thread #{Thread.CurrentThread.ManagedThreadId} from url {url.ToString()}");
        semaphore.Release();
    }
}

public class Program {
    public static void Main(string[] args) {
        LimitedConnectionsExample ex = new LimitedConnectionsExample();

        var l = new List<string>();
        for (int i = 0; i < 25; i++)
            l.Add($"test{i}");

        ex.DownloadFilesAsync(l);

        Thread.Sleep(10000);
    }
}