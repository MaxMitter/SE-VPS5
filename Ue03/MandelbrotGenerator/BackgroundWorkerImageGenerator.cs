using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MandelbrotGenerator {
    public class BackgroundWorkerImageGenerator : IImageGenerator {
        public event EventHandler<EventArgs<Tuple<Area, Bitmap, TimeSpan>>> ImageGenerated;
        private BackgroundWorker worker;

        public BackgroundWorkerImageGenerator() {
            InitBackgroundWorker();
        }

        private void InitBackgroundWorker() {
            worker = new BackgroundWorker();
            worker.DoWork += Run;
            worker.WorkerSupportsCancellation = true;
            worker.RunWorkerCompleted += OnImageGenerated;
        }

        private void Run(object sender, DoWorkEventArgs e) {
            Area area = (Area)e.Argument;
            Stopwatch sw = new Stopwatch();
            var worker = (BackgroundWorker)sender;
            var token = new CancellationToken(worker.CancellationPending);

            sw.Start();
            var bitmap = SyncImageGenerator.GenerateMandelbrotSet(area, token);
            sw.Stop();

            e.Result = new Tuple<Area, Bitmap, TimeSpan>(area, bitmap, sw.Elapsed);
        }

        private void OnImageGenerated(object sender, RunWorkerCompletedEventArgs e) {
            if (e.Cancelled) {
                return;
            }

            var handler = ImageGenerated;
            if (handler != null)
                handler(this, new EventArgs<Tuple<Area, Bitmap, TimeSpan>>(
                  (Tuple<Area, Bitmap, TimeSpan>)e.Result
                  ));
        }

        public void GenerateImage(Area area) {
            if (worker.IsBusy) {
                worker.CancelAsync();
                InitBackgroundWorker();
            }

            worker.RunWorkerAsync(area);
        }
    }
}
