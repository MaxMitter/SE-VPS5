using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MandelbrotGenerator {
    public class ParallelImageGenerator : IImageGenerator {
        public event EventHandler<EventArgs<Tuple<Area, Bitmap, TimeSpan>>> ImageGenerated;

        private Bitmap[] imageParts;
        private CancellationTokenSource cancellationSource;

        public void GenerateImage(Area area) {
            cancellationSource?.Cancel(false);
            cancellationSource = new CancellationTokenSource();

            var cols = Settings.defaultSettings.Workers;
            var partWidth = area.Width / cols;

            imageParts = new Bitmap[cols];
            int colStart = 0;
            for (int i = 0; i < cols; i++) {
                int colEnd = colStart + partWidth;

                var t = new Thread(GenerateImagePart);
                t.Start(new Tuple<Area, int, int, int, CancellationToken>(area, colStart, colEnd, i, cancellationSource.Token));

                colStart += partWidth;
            }
        }

        private void GenerateImagePart(object obj) {
            var tuple = (Tuple<Area, int, int, int, CancellationToken>)obj;
            var sw = new Stopwatch();
            sw.Start();
            var image = GenerateImagePart(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item5);
            sw.Stop();

            OnImageGenerated(tuple.Item1, image, sw.Elapsed, tuple.Item4);
        }

        private static Bitmap GenerateImagePart(Area area, int colStart, int colEnd, CancellationToken token) {
            if (token.IsCancellationRequested) return null;

            var bitmap = new Bitmap(colEnd - colStart, area.Height);
            int maxIterations;
            double zBorder;
            double cReal, cImg, zReal, zImg, zNewReal, zNewImg;

            maxIterations = Settings.DefaultSettings.MaxIterations;
            zBorder = Settings.DefaultSettings.ZBorder * Settings.DefaultSettings.ZBorder;

            //insert code

            for (int i = colStart; i < colEnd; i++) {
                for (int j = 0; j < area.Height; j++) {
                    cReal = area.MinReal + i * area.PixelWidth; // extract starting points based on the grid position
                    cImg = area.MinImg + j * area.PixelWidth;
                    zReal = 0; // sequence variable = current value
                    zImg = 0;

                    int k = 0;
                    while ((zReal * zReal + zImg * zImg < zBorder) && (k < maxIterations)) {
                        zNewReal = zReal * zReal - zImg * zImg + cReal;
                        zNewImg = 2 * zReal * zImg + cImg;
                        zReal = zNewReal;
                        zImg = zNewImg;
                        k++;
                    }
                    bitmap.SetPixel(i - colStart, j, ColorSchema.GetColor(k));
                    if (token.IsCancellationRequested) return null;
                }
            }

            //end insert

            return bitmap;
        }

        private void OnImageGenerated(Area area, Bitmap image, TimeSpan elapsed, int col) {
            imageParts[col] = image;

            if (imageParts.Any(imagepart => imagepart == null)) return;

            var result = MergeBitmaps(area);

            var handler = ImageGenerated;
            if (handler != null)
                handler(this, new EventArgs<Tuple<Area, Bitmap, TimeSpan>>(
                  new Tuple<Area, Bitmap, TimeSpan>(area, result, elapsed)
                  ));
        }

        private Bitmap MergeBitmaps(Area area) {
            var result = new Bitmap(area.Width, area.Height);

            using (Graphics g = Graphics.FromImage(result)) {
                var colStart = 0;
                foreach (var part in imageParts) {
                    g.DrawImage(part, colStart, 0);
                    colStart += part.Width;
                }
            }

            return result;
        }
    }
}
