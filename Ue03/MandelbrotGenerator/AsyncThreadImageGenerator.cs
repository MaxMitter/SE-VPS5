using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MandelbrotGenerator {
  public class AsyncThreadImageGenerator : IImageGenerator {
    public event EventHandler<EventArgs<Tuple<Area, Bitmap, TimeSpan>>> ImageGenerated;
    private CancellationTokenSource cts;

    public void GenerateImage(Area area) {
      if (cts != null) cts.Cancel();
      cts = new CancellationTokenSource();
      Thread thread = new Thread(new ParameterizedThreadStart(Run));
      thread.Start(new Tuple<Area, CancellationToken>(area, cts.Token));
    }

    private void Run(object obj) {
      var tuple = (Tuple<Area, CancellationToken>)obj;
      Area area = tuple.Item1;
      CancellationToken token = tuple.Item2;

      Stopwatch sw = new Stopwatch();
      sw.Start();
      var bitmap = SyncImageGenerator.GenerateMandelbrotSet(area, token);
      sw.Stop();
      // update image - fire event!
      OnImageGenerated(area, bitmap, sw.Elapsed);
    }

    private void OnImageGenerated(Area area, Bitmap bitmap, TimeSpan elapsed) {
      var handler = ImageGenerated;
      if (handler != null)
        handler(this, new EventArgs<Tuple<Area, Bitmap, TimeSpan>>(
          new Tuple<Area, Bitmap, TimeSpan>(area, bitmap, elapsed)
          ));
    }
  }
}
